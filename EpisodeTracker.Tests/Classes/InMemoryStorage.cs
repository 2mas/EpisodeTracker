using System;
using EpisodeTracker.Storage;

namespace EpisodeTracker.Tests.Classes
{
    class InMemoryStorage : IStorage
    {
        private StoreModel StoreModel;

        public InMemoryStorage()
        {
            this.StoreModel = new StoreModel
            {
                ApiCredentials = new ApiCredentials
                {
                    ApiUrl = "https://api.thetvdb.com/",
                    ApiKey = "ApiKey",
                    ApiUser = "ApiUser",
                    ApiUserkey = "ApiUserKey"
                },
                Token = new Http.JwtToken
                {
                    Expiration = DateTime.Now.AddDays(1),
                    Token = "valid"
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
