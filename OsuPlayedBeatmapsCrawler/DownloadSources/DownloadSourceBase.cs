using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsuPlayedBeatmapsCrawler.DownloadSources
{
    public abstract class DownloadSourceBase
    {
        public DownloadSourceBase(HttpClient client)
        {
            Client = client;
        }

        public HttpClient Client { get; }

        public abstract Task<bool> DownloadBeatmap(BeatmapSet beatmapset,string folderPath);
    }
}
