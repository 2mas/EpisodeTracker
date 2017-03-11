using EpisodeTracker.Storage;
using EpisodeTracker.Tests.Classes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace EpisodeTracker.Tests
{
    [TestClass]
    public class EpisodeTrackerTests
    {
        private Tracker Tracker;

        [TestInitialize]
        public void Setup()
        {
            FakeHttpMessageHandler fakeHandler = new FakeHttpMessageHandler();
            FakeHandlerSeeder.Seed(fakeHandler);

            this.Tracker = new Tracker(
                    new HttpClient(fakeHandler),
                    new InMemoryStorage()
                );
        }

        [TestMethod]
        public async Task AfterASearchTheResultShouldBeAvailableInTemporaryDataModel()
        {
            string searchText = "Narcos";
            List<Series> result = await this.Tracker.SearchSeriesAsync(searchText);
            Assert.AreEqual("Narcos", this.Tracker.TmpData.LatestSearch.First().Name);
        }

        [TestMethod]
        public async Task RetrievingListOfTrackedSeriesShouldGiveCorrectNumberOfItems()
        {
            this.Tracker.Storage.GetStoreModel().TrackedItems = new List<TrackedItem>();
            await this.Tracker.Track(123);
            await this.Tracker.Track(456);

            Assert.AreEqual(2, this.Tracker.GetTrackedItems().Count);
        }

        [TestMethod]
        public void ViewingInfoAboutASeriesShouldGiveCorrectId()
        {
            Series series = this.Tracker.ViewSeriesInformationByIdAsync((long)282670).Result;
            Assert.AreEqual(282670, series.Id);
        }

        [TestMethod]
        public void ViewingEpisodesBySeriesIdShouldGiveCorrectEpisodeCount()
        {
            List<Episode> episodes = this.Tracker.ViewEpisodesBySeriesIdAsync((long)282670).Result;
            Assert.AreEqual(3, episodes.Count());
        }

        [TestMethod]
        public void ViewingEpisodeSummaryShouldGiveCorrectNumberOfSeasonsAndEpisodes()
        {
            Series series = this.Tracker.ViewSeriesInformationByIdAsync((long)282670).Result;
            Assert.AreEqual("1, 2", series.AiredSeasons);
            Assert.AreEqual(3, series.AiredEpisodes);
        }

        [TestMethod]
        public async Task AddingTrackedSeriesShouldShowUpInStoreModel()
        {
            this.Tracker.Storage.GetStoreModel().TrackedItems = new List<TrackedItem>();
            await this.Tracker.Track(123);
            Assert.IsTrue(this.Tracker.GetTrackedItems().Any(d => d.SeriesId == 123));
        }

        [TestMethod]
        public async Task RemovingTrackedSeriesShouldUpdateStoreModel()
        {
            await this.Tracker.Track(123);
            this.Tracker.UnTrack(123);
            Assert.IsFalse(this.Tracker.GetTrackedItems().Any(d => d.SeriesId == 123));
        }

        [TestMethod]
        public async Task AddingDuplicateTrackedSeriesShouldBeIgnored()
        {
            this.Tracker.Storage.GetStoreModel().TrackedItems = new List<TrackedItem>();
            await this.Tracker.Track(123);
            await this.Tracker.Track(123);
            Assert.IsTrue(this.Tracker.GetTrackedItems().Count(d => d.SeriesId == 123) == 1);
        }

        [TestMethod]
        public async Task SearchForNarcosWithFakeHandlerShouldReturnOneHit()
        {
            string searchText = "Narcos";
            List<Series> result = await this.Tracker.SearchSeriesAsync(searchText);

            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public async Task SearchForSeriesThatDoesntExistShouldGiveEmptyResult()
        {
            string searchText = "Socran";
            List<Series> result = await this.Tracker.SearchSeriesAsync(searchText);

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task AfterRetrievingNewEpisodeInformationAboutTrackedItemANotificationShouldBeSentAndTotalEpisodesUpdated()
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

            Assert.AreEqual(2, seriesWithNewEpisodes.First().TotalAiredEpisodes - seriesWithNewEpisodes.First().TotalSeenEpisodes);
            Assert.AreEqual(2, seriesWithNewEpisodes.Sum(x => x.UnSeenEpisodes.Count));
        }

        [TestMethod]
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

            Assert.AreEqual(282671, this.Tracker.Storage.GetStoreModel().TrackedItems.First().UnSeenEpisodes.First().Id);
        }

        [TestMethod]
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

            Assert.AreEqual(1, this.Tracker.Storage.GetStoreModel().TrackedItems.First().UnSeenEpisodes.Count);

            this.Tracker.MarkEpisodesAsSeen(new long[] { 321 });

            Assert.AreEqual(0, this.Tracker.Storage.GetStoreModel().TrackedItems.First().UnSeenEpisodes.Count);
            Assert.AreEqual(2, this.Tracker.Storage.GetStoreModel().TrackedItems.First().TotalSeenEpisodes);
        }

        [TestMethod]
        public void SendingEmailNotification()
        {
            this.Tracker.Storage.GetStoreModel().TrackedItems = new List<TrackedItem>();
            this.Tracker.Storage.GetStoreModel().TrackedItems.Add(new TrackedItem()
            {
                SeriesId = 282670,
                Name = "Narcos",
                TotalSeenEpisodes = 240,
                TrackingPoint = DateTime.Now.AddDays(-1),
                Status = "Continuing",
                UnSeenEpisodes = new List<Episode>()
            });

            this.Tracker.SendNotifications(this.Tracker.Storage.GetStoreModel().TrackedItems);
        }
    }
}
