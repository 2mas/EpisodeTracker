﻿using System;
using EpisodeTracker.Storage;
using EpisodeTracker.Storage.NotificationConfig;
using System.Collections.Generic;

namespace EpisodeTracker.Tests.Classes
{
    class InMemoryStorage : IStorage
    {
        private StoreModel StoreModel;

        public InMemoryStorage()
        {
            var Recipients =  new List<string>() { "thomas.welen@gmail.com" };

            this.StoreModel = new StoreModel
            {
                ApiCredentials = new ApiCredentials
                {
                    ApiUrl = "https://api.thetvdb.com/",
                    ApiKey = "ApiKey",
                    ApiUser = "ApiUser",
                    ApiUserkey = "ApiUserKey"
                }
            };
        }

        public StoreModel GetStoreModel()
        {
            return this.StoreModel;
        }

        public StoreModel Load()
        {
            return this.StoreModel;
        }

        public void Save()
        {
        }

        public void SetStoreModel(StoreModel storeModel)
        {
            throw new NotImplementedException();
        }
    }
}
