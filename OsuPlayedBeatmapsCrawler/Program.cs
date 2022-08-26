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

var logFilePath = Path.Combine(logFolderPath, $"{DateTime.Now.ToShortDateString} {DateTime.Now.ToShortTimeString}.log");
void log(string message)
{
    File.AppendAllText(logFilePath, message + Environment.NewLine);
    Console.WriteLine(message);
}

Directory.CreateDirectory(beamtapSaveFolderPath);

var handler = new HttpClientHandler() { CookieContainer = new CookieContainer() };
var httpClient = new HttpClient(handler);

var msg = await httpClient.GetAsync($"https://osu.ppy.sh/beatmapsets");
var token = handler.CookieContainer.GetAllCookies().FirstOrDefault(x => x.Name == "XSRF-TOKEN").Value;

foreach (Cookie cookie in handler.CookieContainer.GetAllCookies())
{
    log($"cookie domain : {cookie.Domain} name : {cookie.Name} value : {cookie.Value}");
}

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
                log($"downloaded beatmap id : {beatmapSet.ID}");
            }
            else
            {
                log($"download beatmap failed , beatmap id : {beatmapSet.ID}");
            }
        }
        catch (Exception e)
        {
            log($"download beatmap {beatmapSet.ID} throw exception : {e.Message}");
        }
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