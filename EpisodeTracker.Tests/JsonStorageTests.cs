using System;
using System.Collections.Generic;
using System.IO;
using EpisodeTracker.Http;
using EpisodeTracker.Storage;
using Newtonsoft.Json;
using Xunit;

namespace EpisodeTracker.Tests
{
    public class JsonStorageTests
    {
        private IStorage Storage;

        public JsonStorageTests()
        {
            this.Storage = new JsonStorage(
                Path.Combine(
                    Path.Combine(Environment.CurrentDirectory, "Data"),
                    "StorageTest.json")
                );
        }

        [Fact]
        public void LoadingJsonStorageShouldReturnAStoreModel()
        {
            var storeModel = this.Storage.GetStoreModel();
            Assert.IsType<StoreModel>(storeModel);
        }

        [Fact]
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

            this.Storage.SetStoreModel(storeModel);
            this.Storage.Save();
            StoreModel storeModelLoaded = this.Storage.GetStoreModel();

            // For comparison, only public properties
            string jsonStoreModel = JsonConvert.SerializeObject(storeModel);
            string jsonStoreModelLoaded = JsonConvert.SerializeObject(storeModelLoaded);

            Assert.Equal(jsonStoreModel, jsonStoreModelLoaded);
        }
    }
}
