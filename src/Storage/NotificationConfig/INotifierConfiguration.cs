using System;

namespace EpisodeTracker.Storage.NotificationConfig
{
    public interface INotifierConfiguration
    {
        Type GetNotifierType();
    }
}