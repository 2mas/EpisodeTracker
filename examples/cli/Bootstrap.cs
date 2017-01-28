using EpisodeTracker.Storage;
using EpisodeTracker;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

namespace EpisodeTracker.CLI
{
    static class Bootstrap
    {
        public static void Initialize()
        {
            var storage = new JsonStorage(
                Path.Combine(
                    Path.Combine(
                        Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                        , "Data"),
                        "EpisodeTracker.json")
                    );

            Program.episodeTracker = new Tracker(
                new System.Net.Http.HttpClient(),
                storage,
                new List<INotifier>() {
                    new EmailNotifier()
                }
            );
        }
    }
}
