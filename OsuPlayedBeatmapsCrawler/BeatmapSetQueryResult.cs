using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsuPlayedBeatmapsCrawler
{
    public class BeatmapSetQueryResult
    {
        [JsonProperty("beatmapsets")]
        public List<BeatmapSet> BeatmapSets { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }

        [JsonProperty("cursor_string")]
        public string CursorString { get; set; }
    }
}
