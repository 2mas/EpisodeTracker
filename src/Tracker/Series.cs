using System.ComponentModel;

namespace EpisodeTracker
{
    public class Series
    {
        [Browsable(true)]
        public string Name { get; set; }

        [Browsable(true)]
        public long Id { get; set; }

        [Browsable(false)]
        public string Banner { get; set; }

        [Browsable(true)]
        public string Status { get; set; }

        [Browsable(true)]
        public string ImdbId { get; set; }

        [Browsable(true)]
        public string AiredSeasons { get; set; }

        [Browsable(true)]
        public int AiredEpisodes { get; set; }

        [Browsable(true)]
        public string FirstAired { get; set; }

        [Browsable(true)]
        public string AirDay { get; set; }

        [Browsable(true)]
        public string AirTime { get; set; }

        [Browsable(true)]
        public double TVDBRating { get; set; }

        [Browsable(true)]
        public long TVDBRatingCount { get; set; }

        [Browsable(true)]
        public string Overview { get; set; }
    }
}
