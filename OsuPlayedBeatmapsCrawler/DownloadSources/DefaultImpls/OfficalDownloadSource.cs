using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsuPlayedBeatmapsCrawler.DownloadSources.DefaultImpls
{
    public class OfficalDownloadSource : DownloadSourceBase
    {
        public OfficalDownloadSource(HttpClient client) : base(client)
        {

        }

        public override async Task<bool> DownloadBeatmap(BeatmapSet beatmapset, string folderPath)
        {
            var url = $"https://osu.ppy.sh/beatmapsets/{beatmapset.ID}/download";

            await Client.DownloadFile(url, folderPath);
            return true;
        }
    }
}
