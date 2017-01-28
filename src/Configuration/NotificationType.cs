using System;

namespace EpisodeTracker.Configuration
{
    /// <summary>
    /// Ways of being notified when new releases comes out.
    /// </summary>
    [Flags]
    public enum NotificationType
    {
        None = 0,
        Email = 1
    }
}
