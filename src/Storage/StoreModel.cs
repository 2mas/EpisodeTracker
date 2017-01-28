using EpisodeTracker.Http;
using System.Collections.Generic;

namespace EpisodeTracker.Storage
{
    public class StoreModel
    {
        public JwtToken Token { get; set; }

        public List<TrackedItem> TrackedItems { get; set; }

        public StoreModel()
        {
            Token = new JwtToken();
            TrackedItems = new List<TrackedItem>();
        }
    }
}
