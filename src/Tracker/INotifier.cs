using EpisodeTracker.Storage;
using System.Collections.Generic;

namespace EpisodeTracker
{
    public interface INotifier
    {
        void SendNotifications(List<TrackedItem> trackedItems);
    }
}
