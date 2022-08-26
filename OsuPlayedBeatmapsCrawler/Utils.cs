using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace OsuPlayedBeatmapsCrawler
{
    internal static class Utils
    {
        public static async Task DownloadFile(this HttpClient client ,string url, string saveFolderPath)
        {
            using var resp = await client.GetAsync(url);

            var contentdispotision = resp.Content.Headers.GetValues("content-disposition");
            var fileName = WebUtility.UrlDecode(new ContentDisposition(contentdispotision.FirstOrDefault()).FileName);
            fileName = string.IsNullOrWhiteSpace(fileName) ? throw new Exception("no content-disposition.FileName") : fileName;

            var filePath = Path.Combine(saveFolderPath, fileName);

            using var stream = await resp.Content.ReadAsStreamAsync();
            using var fileStream = File.OpenWrite(filePath);

            await stream.CopyToAsync(fileStream);
        }
    }
}
