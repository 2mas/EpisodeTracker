using System.Collections.Generic;

namespace EpisodeTracker
{
    public class TemporaryData
    {
        /// <summary>
        /// Used to display searchresults with a counter-number like 1-4 instead of real ID´s.
        /// Makes things easiser when using a CLI for instance
        /// </summary>
        private List<Series> latestSearch;
        public List<Series> LatestSearch { get => latestSearch; set => latestSearch = value; }

        private Series latestViewById;
        public Series LatestViewById { get => latestViewById; set => latestViewById = value; }
    }
}
