using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsuPlayedBeatmapsCrawler
{
    public class BeatmapSet
    {
        [JsonProperty("artist_unicode")]
        public string ArtistUnicode { get; set; }
        [JsonProperty("artist")]
        public string Artist { get; set; }

        public string ArtistDisplay => string.IsNullOrWhiteSpace(ArtistUnicode) ? Artist : ArtistUnicode;

        [JsonProperty("title_unicode")]
        public string TitleUnicode { get; set; }
        [JsonProperty("title")]
        public string Title { get; set; }

        public string TitleDisplay => string.IsNullOrWhiteSpace(TitleUnicode) ? Title : TitleUnicode;

        [JsonProperty("id")]
        public int ID { get; set; }

        [JsonProperty("ranked_date")]
        public DateTime RankedTime { get; set; }
    }
}
