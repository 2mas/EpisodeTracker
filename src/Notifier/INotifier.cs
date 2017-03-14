using EpisodeTracker.Storage;
using System;
using System.Collections.Generic;

namespace EpisodeTracker.Notifier
{
    public interface INotifier
    {
        void Setup(StoreModel storeModel);
        void SendNotifications(List<TrackedItem> trackedItems);
    }
}
