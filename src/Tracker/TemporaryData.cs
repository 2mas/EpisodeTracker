using System.Collections.Generic;

namespace EpisodeTracker
{
    public class TemporaryData
    {
        /// <summary>
        /// Used to display searchresults with a counter-number like 1-4 instead of real ID´s.
        /// Makes things easiser when using a CLI for instance
        /// </summary>
        public List<Series> LatestSearch;

        public Series LatestViewById;
    }
}
