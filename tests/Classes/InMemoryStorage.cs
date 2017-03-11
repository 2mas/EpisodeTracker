using System;
using EpisodeTracker.Storage;
using EpisodeTracker.Storage.NotificationConfig;

namespace EpisodeTracker.Tests.Classes
{
    class InMemoryStorage : IStorage
    {
        private StoreModel StoreModel;

        public InMemoryStorage()
        {
            var Recipients = new System.Collections.Generic.List<System.Net.Mail.MailAddress>();
            Recipients.Add(new System.Net.Mail.MailAddress("thomas.welen@gmail.com"));

            this.StoreModel = new StoreModel
            {
                ApiCredentials = new ApiCredentials
                {
                    ApiUrl = "https://api.thetvdb.com/",
                    ApiKey = "ApiKey",
                    ApiUser = "ApiUser",
                    ApiUserkey = "ApiUserKey"
                },
                NotificationSettings = new NotificationSettings
                {
                    Configurations = new System.Collections.Generic.List<INotifierConfiguration>()
                    {
                        new EmailConfiguration
                        {
                            DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.SpecifiedPickupDirectory,
                            PickupDirectoryLocation = @"C:\Downloads\kod\net\EpisodeTracker\TestResults\TestMails\",
                            From = "thomas.welen@gmail.com",
                            Recipients = Recipients
                        }
                    }
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

        public void Save(StoreModel model)
        {
        }
    }
}
