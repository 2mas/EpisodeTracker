using EpisodeTracker.Storage;
using System.Collections.Generic;

namespace EpisodeTracker
{
    public interface INotifier
    {
        bool SendNotifications(StoreModel storeModel, List<TrackedItem> trackedItems);
    }
}
