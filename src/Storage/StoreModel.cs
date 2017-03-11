using EpisodeTracker.Http;
using System.Collections.Generic;

namespace EpisodeTracker.Storage
{
    public class StoreModel
    {
        public ApiCredentials ApiCredentials { get; set; }

        public JwtToken Token { get; set; }

        public NotificationSettings NotificationSettings { get; set; }

        public List<TrackedItem> TrackedItems { get; set; }

        public StoreModel()
        {
            ApiCredentials = new ApiCredentials();
            Token = new JwtToken();
            TrackedItems = new List<TrackedItem>();
            NotificationSettings = new NotificationSettings();
        }
    }
}
