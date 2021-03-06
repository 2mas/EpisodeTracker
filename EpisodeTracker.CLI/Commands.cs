﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using ConsoleTables;
using EpisodeTracker.Storage;
using EpisodeTracker.Storage.NotificationConfig;

namespace EpisodeTracker.CLI
{
    static class Commands
    {
        internal static void Search(string searchString)
        {
            List<Series> result = new List<Series>();

            result = Program.episodeTracker.SearchSeriesAsync(searchString).Result;

            var trackedItems = Program.episodeTracker.GetTrackedItems();

            if (result.Count > 0)
            {
                int counter = 1;
                ConsoleTable table = new ConsoleTable("Number", "Tracked", "Id", "Name", "Status");

                result.ForEach(
                        res => table.AddRow(
                            counter++,
                            trackedItems.Any(d => d.SeriesId == res.Id) ? "X" : "",
                            res.Id,
                            res.Name,
                            res.Status
                        )
                    );

                table.Write();

                Program.Output.WriteLine();
                Program.Output.WriteLine("Use command: -t/--track {Number} to track");
                Program.Output.WriteLine();
            }
            else
            {
                Program.Output.WriteLine("No results");
                Program.Output.WriteLine();
            }
        }

        internal static void List(string type)
        {
            if (type.Trim().ToLower() != "tracked" && type.Trim().ToLower() != "unseen")
            {
                Program.Output.WriteLine("Possible list-arguments: 'tracked/unseen'");
                Program.Output.WriteLine();
                return;
            }

            var trackedItems = Program.episodeTracker.GetTrackedItems();

            if (trackedItems.Count > 0)
            {
                int counter = 1;

                if (type.Trim().ToLower() == "tracked")
                {
                    ConsoleTable table = new ConsoleTable("Number", "Series", "Trackingpoint", "Status", "Series Id", "Unseen episodes");

                    trackedItems.ForEach(
                            item =>
                            {
                                string unseenEpisodes = String.Join(
                                    ", ",
                                    item.UnSeenEpisodes
                                        .OrderBy(x => Convert.ToDateTime(x.FirstAired))
                                        .Select(x => x.ToString())
                                );

                                table.AddRow(
                                  counter++,
                                  item.Name,
                                  item.TrackingPoint.ToShortDateString(),
                                  item.Status,
                                  item.SeriesId,
                                  unseenEpisodes
                              );
                            }
                        );

                    table.Write();

                    Program.Output.WriteLine();
                    Program.Output.WriteLine("Use command: --untrack {Number} to untrack");
                    Program.Output.WriteLine();
                }
                if (type.Trim().ToLower() == "unseen")
                {
                    ConsoleTable table = new ConsoleTable("Number", "Series", "Episode", "Aired", "EpisodeId");
                    trackedItems = trackedItems.Where(x => x.UnSeenEpisodes.Count > 0).ToList();

                    trackedItems.ForEach(
                            item =>
                            {
                                item.UnSeenEpisodes.ForEach(unseen =>
                                {
                                    table.AddRow(
                                        counter++,
                                        item.Name,
                                        unseen.ToString(),
                                        unseen.FirstAired.ToShortDateString(),
                                        unseen.Id
                                    );
                                });
                            }
                        );

                    table.Write();

                    Program.Output.WriteLine();
                    Program.Output.WriteLine("Use command: --seen {Number} to mark this and earlier episodes as seen");
                    Program.Output.WriteLine();
                }
            }
            else
            {
                Program.Output.WriteLine("Nothing tracked");
                Program.Output.WriteLine();
            }
        }

        internal static void Configuration(string configuration)
        {
            if (configuration.Length < 3
                || !(configuration.ToLower() == "list")
                && !configuration.ToLower().Contains("add"))
            {
                Program.Output.WriteLine("Possible configuration-arguments: list | add {ConfigurationClass}");
                Program.Output.WriteLine();
                return;
            }
            else
            {
                var storeModel = Program.episodeTracker.Storage.GetStoreModel();

                if (configuration.ToLower() == "list")
                {
                    Program.Output.WriteLine("Listing configuration, keys and values");
                    Program.Output.WriteLine("-------------------------------------");
                    Program.Output.WriteLine();

                    var apiCredentials = storeModel.ApiCredentials;

                    Program.Output.WriteLine("# ApiCredentials");
                    Program.Output.WriteLine();
                    Program.Output.WriteLine($"ApiUrl = {apiCredentials.ApiUrl}");
                    Program.Output.WriteLine($"ApiKey = {apiCredentials.ApiKey}");
                    Program.Output.WriteLine($"ApiUser = {apiCredentials.ApiUser}");
                    Program.Output.WriteLine($"ApiUserkey = {apiCredentials.ApiUserkey}");
                    Program.Output.WriteLine();

                    var notificationConfigurations = storeModel.NotificationSettings.Configurations;

                    Program.Output.WriteLine("# NotificationConfigurations");

                    var implementations = NotificationConfigHelper.GetINotifierImplementationTypes();

                    string notifierConfigNames = String.Join(", ", implementations.Select(i => i.Name));

                    Program.Output.Write($"Available notifiers: {notifierConfigNames}. Use --config add 'notifier config name' to add one");
                    Program.Output.WriteLine();

                    notificationConfigurations.ForEach(config =>
                    {
                        var properties = config.GetType().GetProperties(System.Reflection.BindingFlags.Public).ToList();

                        Program.Output.WriteLine(config.GetNotifierType().Name);
                        properties.ForEach(p =>
                        {
                            Program.Output.WriteLine($"{p.Name}={p.GetValue(p.Name)}");
                        });

                        Program.Output.WriteLine();
                    });
                }
                if (configuration.Substring(0, 3).ToLower() == "add")
                {
                    var addCommand = configuration.Substring(4, configuration.Length - 4);

                    var implementations = NotificationConfigHelper.GetINotifierImplementationTypes();

                    string notifierConfigNames = String.Join(", ", implementations.Select(i => i.Name));

                    if (!implementations.Any(i => i.Name == addCommand))
                    {
                        Program.Output.Write($"Available notifiers: {notifierConfigNames}. Use --config add notifiername to add one");
                        Program.Output.WriteLine();
                    }
                    else
                    {
                        var notifierConfigType = implementations.First(i => i.Name == addCommand);
                        var configElement = Activator.CreateInstance(notifierConfigType);

                        if (!storeModel.NotificationSettings.Configurations.Any(c => c.GetType() == notifierConfigType))
                        {
                            storeModel.NotificationSettings.Configurations.Add((INotifierConfiguration)configElement);

                            Program.episodeTracker.SaveStoreModel();

                            Program.Output.Write($"{addCommand} added, please provide correct parameters in json-settings");
                            Program.Output.WriteLine();
                        }
                        else
                        {
                            Program.Output.Write($"{addCommand} already exists.");
                            Program.Output.WriteLine();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Marks a series as seen, or all series
        /// </summary>
        /// <param name="seen"></param>
        internal static void SeenSeries(string seen)
        {
            int number = 0;
            List<long> episodeIds = new List<long>();

            if (seen.ToLower() != "all" && !Int32.TryParse(seen, out number))
            {
                Program.Output.WriteLine("Possible seen-arguments: 'all'/number");
                Program.Output.WriteLine();
                return;
            }

            var trackedItems = Program.episodeTracker.GetTrackedItems();

            if (trackedItems.Count > 0)
            {
                if (seen.ToLower() == "all")
                {
                    trackedItems.ForEach(x => episodeIds.AddRange(x.UnSeenEpisodes.Select(d => d.Id)));

                    Program.episodeTracker.MarkEpisodesAsSeen(episodeIds.ToArray());

                    Program.Output.WriteLine("Unseen episodes cleared for all series");
                    Program.Output.WriteLine();
                }
                else
                {
                    if (number > 0 && trackedItems.Count >= number)
                    {
                        TrackedItem trackedItem = trackedItems[--number];
                        episodeIds.AddRange(trackedItem.UnSeenEpisodes.Select(x => x.Id));

                        Program.episodeTracker.MarkEpisodesAsSeen(episodeIds.ToArray());

                        Program.Output.WriteLine($"Unseen episodes cleared for series: {trackedItem.Name }");
                        Program.Output.WriteLine();
                    }
                    else
                    {
                        Program.Output.WriteLine("Number out of range");
                        Program.Output.WriteLine();
                        return;
                    }
                }
            }
            else
            {
                Program.Output.WriteLine("Nothing tracked");
                Program.Output.WriteLine();
            }
        }

        /// <summary>
        /// Marks an unseen episode and previous episodes as seen
        /// </summary>
        /// <param name="seen"></param>
        internal static void Seen(int number)
        {
            var trackedItems = Program.episodeTracker.GetTrackedItems()
                .Where(x => x.UnSeenEpisodes.Count > 0).ToList(); ;

            if (trackedItems.Count > 0)
            {
                Episode episodeSeenUntil = null;

                int counter = 1;
                trackedItems.ForEach(
                    item =>
                    {
                        item.UnSeenEpisodes.ForEach(unseen =>
                        {
                            if (counter++ == number)
                                episodeSeenUntil = unseen;
                        });
                    }
                );

                if (episodeSeenUntil != null)
                {
                    // get earlier episodes in same series
                    List<Episode> episodes = trackedItems
                        .Find(x => x.UnSeenEpisodes.Exists(u => u.Id == episodeSeenUntil.Id))
                        .UnSeenEpisodes.Where(x => x.FirstAired <= episodeSeenUntil.FirstAired).ToList();

                    Program.episodeTracker.MarkEpisodesAsSeen(episodes.Select(x => x.Id).ToArray());
                }
            }

            Program.Output.WriteLine("Episode(s) marked as seen");
            Program.Output.WriteLine();
        }

        internal static void Update()
        {
            Program.Output.WriteLine("Checking for updates...");

            try
            {
#if DEBUG
                var watch = System.Diagnostics.Stopwatch.StartNew();
#endif
                List<TrackedItem> seriesWithNewEpisodes = Program.episodeTracker.CheckForNewEpisodesAsync().Result;
#if DEBUG
                watch.Stop();
                Program.Output.WriteLine($"Time elapsed: { watch.ElapsedMilliseconds } ms");
#endif
                if (seriesWithNewEpisodes.Count > 0)
                {
                    Program.episodeTracker.SendNotifications(seriesWithNewEpisodes);
                    Program.episodeTracker.UpdateTrackingPoint(seriesWithNewEpisodes);
                    Program.Output.WriteLine($"{ seriesWithNewEpisodes.Count } series has { seriesWithNewEpisodes.Sum(x => x.UnSeenEpisodes.Count) } new episodes. Notifications has been sent");
                    Program.Output.WriteLine();
                }
                else
                {
                    Program.Output.WriteLine("No new episodes available");
                    Program.Output.WriteLine();
                }
            }
            catch (OperationCanceledException)
            {
                Program.Output.WriteLine("The operation was cancelled...");
            }
        }

        internal static void View(int seriesId)
        {
            Series series;

            if (Program.episodeTracker.TmpData.LatestViewById != null
                && Program.episodeTracker.TmpData.LatestViewById.Id == seriesId)
            {
                series = Program.episodeTracker.TmpData.LatestViewById;
            }
            else
            {
                series = Program.episodeTracker.ViewSeriesInformationByIdAsync(seriesId).Result;
            }

            if (series.Id == 0)
            {
                Program.Output.WriteLine("Series not found, check the id");
                Program.Output.WriteLine();
                return;
            }

            foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(series))
            {
                if (!descriptor.IsBrowsable)
                    continue;

                string name = descriptor.Name;
                object value = descriptor.GetValue(series);

                if (name == "Overview")
                {
                    Program.Output.WriteLine();
                    Program.Output.WriteLine($"{name}:");
                    Program.Output.WriteLine($"{value}");
                    Program.Output.WriteLine();
                }
                else if (name != "Banner")
                {
                    Program.Output.WriteLine($"{name}: {value}");
                }
            }

            Program.Output.WriteLine();
        }

        internal static void ViewEpisodes(int seriesId)
        {
            Series series;
            List<Episode> episodes;

            if (Program.episodeTracker.TmpData.LatestViewById != null
                && Program.episodeTracker.TmpData.LatestViewById.Id == seriesId)
            {
                series = Program.episodeTracker.TmpData.LatestViewById;
            }
            else
            {
                series = Program.episodeTracker.ViewSeriesInformationByIdAsync(seriesId).Result;
            }

            episodes = Program.episodeTracker.ViewEpisodesBySeriesIdAsync(seriesId).Result;

            Program.Output.WriteLine($"Episodes for series: {series.Name}");
            Program.Output.WriteLine();

            var trackedItems = Program.episodeTracker.GetTrackedItems();
            bool isTracked = trackedItems.Count > 0 && trackedItems.Any(d => d.SeriesId == series.Id);
            TrackedItem trackedItem = isTracked ? trackedItems.First(d => d.SeriesId == series.Id) : null;

            ConsoleTable table = new ConsoleTable("Number", "Episode", "Aired", "Seen");

            int counter = 1;
            episodes.ForEach(
                    res => table.AddRow(
                        $"{counter++}",
                        $"S{res.Season} E{res.Number}",
                        $"{res.FirstAired}",
                        (isTracked && res.FirstAired > trackedItem.TrackingPoint) ? "No" : "Yes"
                    )
                );

            table.Write();
            Program.Output.WriteLine();
        }

        internal static void Track(int number)
        {
            if (Program.episodeTracker.TmpData.LatestSearch != null
                && Program.episodeTracker.TmpData.LatestSearch.Count >= number--)
            {
                Series toBeTracked = Program.episodeTracker.TmpData.LatestSearch[number];

                Program.episodeTracker.Track(toBeTracked.Id).Wait();
                Program.Output.WriteLine($"Series '{toBeTracked.Name}' added to tracking list");
                Program.Output.WriteLine();
            }
            else
            {
                Program.Output.WriteLine("Invalid number");
                Program.Output.WriteLine();
            }
        }

        internal static void UnTrack(int number)
        {
            var trackedItems = Program.episodeTracker.GetTrackedItems();

            if (trackedItems.Count >= number--)
            {
                string name = trackedItems[number].Name;
                Program.episodeTracker.UnTrack(trackedItems[number].SeriesId);
                Program.Output.WriteLine($"Series '{name}' removed from tracking list");
                Program.Output.WriteLine();
            }
            else
            {
                Program.Output.WriteLine("Invalid number");
                Program.Output.WriteLine();
            }
        }
    }
}
