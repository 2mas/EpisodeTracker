using Newtonsoft.Json;
using System;

namespace EpisodeTracker
{
    public class Episode
    {
        public long Id { get; set; }

        public long Number { get; set; }

        public long Season { get; set; }

        [JsonIgnore]
        public long SeasonId { get; set; }

        [JsonIgnore]
        public string Overview { get; set; }

        public DateTime FirstAired { get; set; }

        public override string ToString()
        {
            return $"S{ (Season < 10 ? "0" : "") + Season.ToString() }E{ (Number < 10 ? "0" : "") + Number.ToString()}";
        }
    }
}
