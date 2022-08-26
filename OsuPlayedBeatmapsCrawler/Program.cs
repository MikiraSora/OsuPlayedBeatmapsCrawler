using CommandLine;
using Newtonsoft.Json;
using OsuPlayedBeatmapsCrawler;
using OsuPlayedBeatmapsCrawler.DownloadSources;
using OsuPlayedBeatmapsCrawler.DownloadSources.DefaultImpls;
using System.Net;
using System.Net.Mime;

#region Command

var commandParseResult = Parser.Default.ParseArguments<CommandOption>(args);
if (commandParseResult.Errors.Any())
    return;
var opt = commandParseResult.Value;

if (File.Exists(opt.SecertFilePath))
{
    var lines = File.ReadAllLines(opt.SecertFilePath);
    opt.UserName = lines[0];
    opt.Password = lines[1];
}


if (string.IsNullOrWhiteSpace(opt.UserName) || string.IsNullOrWhiteSpace(opt.Password))
{
    Console.WriteLine("option 'username' or 'password' or 'secert' is empty.");
    return;
}

var userName = opt.UserName;
var password = opt.Password;
var beamtapSaveFolderPath = opt.BeamtapSaveFolderPath;
var cursor = opt.Cursor;
var logFolderPath = opt.LogFolderPath;
var downloadSourceName = opt.DownloadSourceName;

#endregion

Directory.CreateDirectory(beamtapSaveFolderPath);
Directory.CreateDirectory(logFolderPath);

#region Log

var logFileNamePart = $"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}".Replace("\\", "-").Replace("/", "-");
var logFilePath = Path.Combine(logFolderPath, $"{logFileNamePart}.log");
var failedLogFilePath = Path.Combine(logFolderPath, $"{logFileNamePart}.failed.log");

void logFailed(BeatmapSet set)
{
    File.AppendAllText(failedLogFilePath, JsonConvert.SerializeObject(set, Formatting.None) + Environment.NewLine);
}

void log(string message)
{
    File.AppendAllText(logFilePath, message + Environment.NewLine);
    Console.WriteLine(message);
}

#endregion

var handler = new HttpClientHandler() { CookieContainer = new CookieContainer() };
var httpClient = new HttpClient(handler);

var msg = await httpClient.GetAsync($"https://osu.ppy.sh/beatmapsets");
var token = handler.CookieContainer.GetAllCookies().FirstOrDefault(x => x.Name == "XSRF-TOKEN").Value;

#region DownloadSource

DownloadSourceManager.RegisterDownloadSource<SayoDownloadSource>("sayo");
DownloadSourceManager.RegisterDownloadSource<OfficalDownloadSource>("offical");

#endregion

#region Login

var loginReq = new HttpRequestMessage();
loginReq.Method = HttpMethod.Post;
loginReq.RequestUri = new Uri("https://osu.ppy.sh/session");
loginReq.Headers.Add("origin", "https://osu.ppy.sh");
loginReq.Headers.Add("referer", "https://osu.ppy.sh/home");
loginReq.Content = new FormUrlEncodedContent(new[]
{
    KeyValuePair.Create( "_token",  token),
    KeyValuePair.Create( "username",  userName),
    KeyValuePair.Create( "password",  password)
});

var loginResult = await httpClient.SendAsync(loginReq);
if (!loginResult.IsSuccessStatusCode)
{
    log($"login failed : {loginResult.StatusCode} {await loginResult.Content.ReadAsStringAsync()}");
    return;
}

#endregion

int? totalBeatmapCount = null;

#region Fetch & Download

var downloadSource = DownloadSourceManager.GetSource(downloadSourceName, httpClient);
log($"download source : {downloadSourceName} -> {downloadSource.GetType().Name}"); 
log("");

async Task<(IEnumerable<BeatmapSet>, string)> FetchBeatmapSets(string cursor = "")
{
    var url = "https://osu.ppy.sh/beatmapsets/search?";
    //build query
    url += "played=played&";
    url += "sort=ranked_asc";
    if (!string.IsNullOrWhiteSpace(cursor))
        url += $"&cursor_string={cursor}";

    var fetchResult = await httpClient.GetStringAsync(url);
    var queryResult = JsonConvert.DeserializeObject<BeatmapSetQueryResult>(fetchResult);

    totalBeatmapCount = totalBeatmapCount ?? queryResult.TotalCount;

    return (queryResult.BeatmapSets, queryResult.CursorString);
}

Task<bool> DownloadBeatmap(BeatmapSet set) => downloadSource.DownloadBeatmap(set, beamtapSaveFolderPath);

var downloadGood = 0;
var downloadBad = 0;

while (true)
{
    (var beatmapSets, var nextCursor) = await FetchBeatmapSets(cursor);

    foreach (var beatmapSet in beatmapSets)
    {
        log($"{beatmapSet.ID,-10} {beatmapSet.ArtistDisplay,-30}  -  {beatmapSet.TitleDisplay}");

        try
        {
            if (await DownloadBeatmap(beatmapSet))
            {
                downloadGood++;
                log($"downloaded beatmap id : {beatmapSet.ID}");
            }
            else
            {
                downloadBad++;
                log($"download beatmap failed , beatmap id : {beatmapSet.ID}");
                logFailed(beatmapSet);
            }
        }
        catch (Exception e)
        {
            downloadBad++;
            log($"download beatmap {beatmapSet.ID} throw exception : {e.Message}");
            logFailed(beatmapSet);
        }

        log($"crawler download progress : {(downloadGood + downloadBad) * 100f / totalBeatmapCount} (GOOD : {downloadGood} / BAD : {downloadBad} / TOTAL : {totalBeatmapCount})");
        log("");
    }

    if (string.IsNullOrWhiteSpace(nextCursor) || !beatmapSets.Any())
    {
        log($"stop fetching");
        break;
    }

    log($"--------------");
    log($"cursor change {cursor} -> {nextCursor}");
    log($"--------------");
    cursor = nextCursor;
}

#endregion

log("ALL DONE! BYE~");