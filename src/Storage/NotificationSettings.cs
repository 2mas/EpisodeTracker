using EpisodeTracker.Storage.NotificationConfig;
using System.Collections.Generic;

namespace EpisodeTracker.Storage
{
    public class NotificationSettings
    {
        public List<INotifierConfiguration> Configurations;

        public NotificationSettings()
        {
            this.Configurations = new List<INotifierConfiguration>();
        }
    }
}