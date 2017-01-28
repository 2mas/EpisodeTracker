using EpisodeTracker;
using System;
using System.Collections.Generic;

namespace EpisodeTracker.Storage
{
    public class TrackedItem
    {
        /// <summary>
        /// Series name
        /// </summary>
        public string Name { get; set; }

        public long SeriesId { get; set; }

        /// <summary>
        /// Represents the date of the latest synced episode, only episodes newer will be considered new episodes.
        /// </summary>
        public DateTime TrackingPoint { get; set; }

        /// <summary>
        /// If the series is continuing or has ended
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Stores sum of seen episode
        /// </summary>
        public int TotalSeenEpisodes { get; set; }

        /// <summary>
        /// Stores sum of all available episodes
        /// </summary>
        public int TotalAiredEpisodes { get; set; }

        /// <summary>
        /// Stored data about all unseen episodes
        /// </summary>
        public List<Episode> UnSeenEpisodes { get; set; } = new List<Episode>();
    }
}
