using Newtonsoft.Json;
using OsuPlayedBeatmapsCrawler;
using System.Net;
using System.Net.Mime;


#region Setting

var userName = Secret.UserName;
var password = Secret.Password;
var beamtapSaveFolderPath = "./beatmaps";
var cursor = "";
var logFolderPath = "./logs";

#endregion

Directory.CreateDirectory(beamtapSaveFolderPath);
Directory.CreateDirectory(logFolderPath);

var logFilePath = Path.Combine(logFolderPath, $"{DateTime.Now.ToShortDateString} {DateTime.Now.ToShortTimeString}.log");
void log(string message)
{
    File.AppendAllText(logFilePath, message + Environment.NewLine);
    Console.WriteLine(message);
}


var handler = new HttpClientHandler() { CookieContainer = new CookieContainer() };
var httpClient = new HttpClient(handler);

var msg = await httpClient.GetAsync($"https://osu.ppy.sh/beatmapsets");
var token = handler.CookieContainer.GetAllCookies().FirstOrDefault(x => x.Name == "XSRF-TOKEN").Value;

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

async Task<bool> DownloadBeatmap(BeatmapSet set)
{
    var sayoUrl = $"https://txy1.sayobot.cn/beatmaps/download/full/{set.ID}?server=auto";
    using var resp = await httpClient.GetAsync(sayoUrl);

    var contentdispotision = resp.Content.Headers.GetValues("content-disposition");
    var fileName = WebUtility.UrlDecode(new ContentDisposition(contentdispotision.FirstOrDefault()).FileName);
    fileName = string.IsNullOrWhiteSpace(fileName) ? $"{set.ID}.osz" : fileName;

    var filePath = Path.Combine(beamtapSaveFolderPath, fileName);

    using var stream = await resp.Content.ReadAsStreamAsync();
    using var fileStream = File.OpenWrite(filePath);

    await stream.CopyToAsync(fileStream);
    //log($"beatmap {set.ID} saved to {filePath}");

    return true;//todo check
}

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
            }
        }
        catch (Exception e)
        {
            downloadBad++;
            log($"download beatmap {beatmapSet.ID} throw exception : {e.Message}");
        }

        log($"crawler download progress : {(downloadGood + downloadBad) * 100f / totalBeatmapCount} (GOOD : {downloadGood} / BAD : {downloadBad} / TOTAL : {totalBeatmapCount})");
    }

    if (string.IsNullOrWhiteSpace(nextCursor) || !beatmapSets.Any())
    {
        log($"stop fetching");
        break;
    }

    log($"cursor change {cursor} -> {nextCursor}");
    cursor = nextCursor;
}

log($"login user = {loginResult.Content.ReadAsStream()}");