using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace OsuPlayedBeatmapsCrawler.DownloadSources.DefaultImpls
{
    public class SayoDownloadSource : DownloadSourceBase
    {
        public SayoDownloadSource(HttpClient client) : base(client)
        {

        }

        public override async Task<bool> DownloadBeatmap(BeatmapSet set, string folderPath)
        {
            var sayoUrl = $"https://txy1.sayobot.cn/beatmaps/download/full/{set.ID}?server=auto";

            await Client.DownloadFile(sayoUrl, folderPath);
            return true;
        }
    }
}
