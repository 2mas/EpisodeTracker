using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using EpisodeTracker.Storage;
using EpisodeTracker.Tests.Classes;
using Xunit;

namespace EpisodeTracker.Tests
{
    public class EpisodeTrackerTests
    {
        private Tracker Tracker;

        public EpisodeTrackerTests()
        {
            FakeHttpMessageHandler fakeHandler = new FakeHttpMessageHandler();
            FakeHandlerSeeder.Seed(fakeHandler);

            this.Tracker = new Tracker(
                    new HttpClient(fakeHandler),
                    new InMemoryStorage()
                );
        }

        [Fact]
        public void StartingWithoutConfigShouldReturnConfigurationException() {
            FakeHttpMessageHandler fakeHandler = new FakeHttpMessageHandler();
            FakeHandlerSeeder.Seed(fakeHandler);

            Assert.Throws<Exceptions.ApiCredentialException>(() =>
                new Tracker(
                    new HttpClient(fakeHandler),
                    new JsonStorage(
                    Path.Combine(
                        Path.Combine(
                            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                            "Data"), 
                            "EpisodeTracker.json"
                        )
                    )
                )
            );
        }

        [Fact]
        public async Task AfterASearchTheResultShouldBeAvailableInTemporaryDataModel()
        {
            string searchText = "Narcos";
            List<Series> result = await this.Tracker.SearchSeriesAsync(searchText);
            Assert.Equal("Narcos", this.Tracker.TmpData.LatestSearch.First().Name);
        }

        [Fact]
        public async Task RetrievingListOfTrackedSeriesShouldGiveCorrectNumberOfItems()
        {
            this.Tracker.Storage.GetStoreModel().TrackedItems = new List<TrackedItem>();
            await this.Tracker.Track(123);
            await this.Tracker.Track(456);

            Assert.Equal(2, this.Tracker.GetTrackedItems().Count);
        }

        [Fact]
        public void ViewingInfoAboutASeriesShouldGiveCorrectId()
        {
            Series series = this.Tracker.ViewSeriesInformationByIdAsync((long)282670).Result;
            Assert.Equal(282670, series.Id);
        }

        [Fact]
        public void ViewingEpisodesBySeriesIdShouldGiveCorrectEpisodeCount()
        {
            List<Episode> episodes = this.Tracker.ViewEpisodesBySeriesIdAsync((long)282670).Result;
            Assert.Equal(3, episodes.Count());
        }

        [Fact]
        public void ViewingEpisodeSummaryShouldGiveCorrectNumberOfSeasonsAndEpisodes()
        {
            Series series = this.Tracker.ViewSeriesInformationByIdAsync((long)282670).Result;
            Assert.Equal("1, 2", series.AiredSeasons);
            Assert.Equal(3, series.AiredEpisodes);
        }

        [Fact]
        public async Task AddingTrackedSeriesShouldShowUpInStoreModel()
        {
            this.Tracker.Storage.GetStoreModel().TrackedItems = new List<TrackedItem>();
            await this.Tracker.Track(123);
            Assert.Contains(this.Tracker.GetTrackedItems(), d => d.SeriesId == 123);
        }

        [Fact]
        public async Task RemovingTrackedSeriesShouldUpdateStoreModel()
        {
            await this.Tracker.Track(123);
            this.Tracker.UnTrack(123);
            Assert.DoesNotContain(this.Tracker.GetTrackedItems(), d => d.SeriesId == 123);
        }

        [Fact]
        public async Task AddingDuplicateTrackedSeriesShouldBeIgnored()
        {
            this.Tracker.Storage.GetStoreModel().TrackedItems = new List<TrackedItem>();
            await this.Tracker.Track(123);
            await this.Tracker.Track(123);
            Assert.True(this.Tracker.GetTrackedItems().Count(d => d.SeriesId == 123) == 1);
        }

        [Fact]
        public async Task SearchForNarcosWithFakeHandlerShouldReturnOneHit()
        {
            string searchText = "Narcos";
            List<Series> result = await this.Tracker.SearchSeriesAsync(searchText);

            Assert.Single(result);
        }

        [Fact]
        public async Task SearchForSeriesThatDoesntExistShouldGiveEmptyResult()
        {
            string searchText = "Socran";
            List<Series> result = await this.Tracker.SearchSeriesAsync(searchText);

            Assert.Empty(result);
        }

        [Fact]
        public async Task AfterRetrievingNewEpisodeInformationAboutTrackedItemTotalEpisodesUpdates()
        {
            this.Tracker.Storage.GetStoreModel().TrackedItems = new List<TrackedItem>();
            this.Tracker.Storage.GetStoreModel().TrackedItems.Add(new TrackedItem()
            {
                SeriesId = 282670,
                Name = "Narcos",
                TotalSeenEpisodes = 1,
                TrackingPoint = Convert.ToDateTime("2015-08-28"),
                Status = "Continuing"
            });

            var seriesWithNewEpisodes = await this.Tracker.CheckForNewEpisodesAsync();

            Assert.Equal(2, seriesWithNewEpisodes.First().TotalAiredEpisodes - seriesWithNewEpisodes.First().TotalSeenEpisodes);
            Assert.Equal(2, seriesWithNewEpisodes.Sum(x => x.UnSeenEpisodes.Count));
        }

        [Fact]
        public async Task AfterRetrievingNewEpisodeInformationAboutTrackedItemTheNewEpisodeShouldShowUpInUnseenEpisodesInStoreModel()
        {
            this.Tracker.Storage.GetStoreModel().TrackedItems = new List<TrackedItem>();
            this.Tracker.Storage.GetStoreModel().TrackedItems.Add(new TrackedItem()
            {
                SeriesId = 282670,
                Name = "Narcos",
                TotalSeenEpisodes = 1,
                TrackingPoint = Convert.ToDateTime("2015-08-28"),
                Status = "Continuing"
            });

            var seriesWithNewEpisodes = await this.Tracker.CheckForNewEpisodesAsync();
            this.Tracker.SendNotifications(seriesWithNewEpisodes);
            this.Tracker.UpdateTrackingPoint(seriesWithNewEpisodes);

            Assert.Equal(282671, this.Tracker.Storage.GetStoreModel().TrackedItems.First().UnSeenEpisodes.First().Id);
        }

        [Fact]
        public void MarkingUnseenEpisodesAsSeenShouldGiveCorrectStoreModelBack()
        {
            var unSeenEpisodes = new List<Episode>();
            unSeenEpisodes.Add(new Episode()
            {
                Id = 321,
                FirstAired = DateTime.Now,
                Number = 1,
                Season = 1,
                SeasonId = 123
            });

            this.Tracker.Storage.GetStoreModel().TrackedItems = new List<TrackedItem>();
            this.Tracker.Storage.GetStoreModel().TrackedItems.Add(new TrackedItem()
            {
                SeriesId = 282670,
                Name = "Narcos",
                TotalSeenEpisodes = 1,
                TrackingPoint = Convert.ToDateTime("2015-08-28"),
                Status = "Continuing",
                UnSeenEpisodes = unSeenEpisodes
            });

            Assert.Single(this.Tracker.Storage.GetStoreModel().TrackedItems.First().UnSeenEpisodes);

            this.Tracker.MarkEpisodesAsSeen(new long[] { 321 });

            Assert.Empty(this.Tracker.Storage.GetStoreModel().TrackedItems.First().UnSeenEpisodes);
            Assert.Equal(2, this.Tracker.Storage.GetStoreModel().TrackedItems.First().TotalSeenEpisodes);
        }
    }
}
