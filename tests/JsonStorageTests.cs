using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using EpisodeTracker.Storage;
using EpisodeTracker.Http;

namespace EpisodeTracker.Tests
{
    [TestClass]
    public class JsonStorageTests
    {
        private IStorage Storage;

        [TestInitialize]
        public void Setup()
        {
            this.Storage = new JsonStorage(
                Path.Combine(
                    Path.Combine(Environment.CurrentDirectory, "Data"),
                    "StorageTest.json")
                );
        }

        [TestMethod]
        public void LoadingJsonStorageShouldReturnAStoreModel()
        {
            var storeModel = this.Storage.GetStoreModel();
            Assert.IsInstanceOfType(storeModel, typeof(StoreModel));
        }

        [TestMethod]
        public void SavingAStoreModelAndLoadingItBackFromJsonFileShouldGiveSameObjectBack()
        {
            StoreModel storeModel = new StoreModel();
            storeModel.Token = new JwtToken()
            {
                Token = "testtoken",
                Expiration = DateTime.Now.AddDays(1)
            };

            storeModel.TrackedItems = new List<TrackedItem>();

            storeModel.TrackedItems.Add(
                new TrackedItem()
                {
                    TrackingPoint = DateTime.Now,
                    SeriesId = 1,
                    Status = "ended",
                    Name = "Narcos"
                }
            );

            this.Storage.Save(storeModel);
            StoreModel storeModelLoaded = this.Storage.GetStoreModel();

            // For comparison, only public properties
            string jsonStoreModel = JsonConvert.SerializeObject(storeModel);
            string jsonStoreModelLoaded = JsonConvert.SerializeObject(storeModelLoaded);

            Assert.AreEqual(jsonStoreModel, jsonStoreModelLoaded);
        }
    }
}
