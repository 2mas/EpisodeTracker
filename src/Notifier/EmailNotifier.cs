using EpisodeTracker.Storage;
using EpisodeTracker.Storage.NotificationConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;

namespace EpisodeTracker.Notifier
{
    public class EmailNotifier : INotifier
    {
        public static EmailConfiguration Configuration;
        private static SmtpClient Client;
        private static List<MailAddress> Recipients = new List<MailAddress>();
        private const string NotificationSubject = "EpisodeTracker - New episodes";
        private const string NotificationIntroduction = "New episodes available for series: ";

        /// <summary>
        /// Creates an SmtpClient from provided configuration-values
        /// </summary>
        /// <param name="storeModel"></param>
        public void Setup(StoreModel storeModel)
        {
            if (storeModel.NotificationSettings == null ||
                storeModel.NotificationSettings.Configurations == null ||
                !storeModel.NotificationSettings.Configurations
                .Any(c => c.GetType() == typeof(EmailConfiguration)))
            {
                throw new TypeLoadException($"Configuration-class does not exist for { this.GetType().ToString() }");
            }

            Configuration = (EmailConfiguration)storeModel.NotificationSettings.Configurations
                .Find(c => c.GetType() == typeof(EmailConfiguration));

            Client = new SmtpClient(Configuration.Host, Configuration.Port)
            {
                PickupDirectoryLocation = Configuration.PickupDirectoryLocation,
                DeliveryMethod = Configuration.DeliveryMethod,
                EnableSsl = Configuration.EnableSsl,
                UseDefaultCredentials = Configuration.DefaultCredentials
            };

            if (!Client.UseDefaultCredentials)
                Client.Credentials = new System.Net.NetworkCredential(
                    Configuration.UserName, 
                    Configuration.Password
                );

            Configuration.Recipients.ForEach(r => Recipients.Add(new MailAddress(r)));
        }

        public void SendNotifications(List<TrackedItem> trackedItems)
        {
            if (Recipients.Count < 1 || trackedItems.Count < 1)
                return;

            Func<TrackedItem, string> MakeSeriesSummaryString = (trackedItem) =>
            {
                return trackedItem.Name + ": " + String.Join(
                        ", ",
                        trackedItem.UnSeenEpisodes.Select(x => x.ToString())
                    );
            };

            MailMessage message = new MailMessage();
            message.From = new MailAddress(Configuration.From);
            Recipients.ForEach(x => message.To.Add(x));
            message.Subject = NotificationSubject;

            StringBuilder mailBody = new StringBuilder(NotificationIntroduction).AppendLine();
            trackedItems.ForEach(x => mailBody.AppendLine(MakeSeriesSummaryString(x)));
            message.Body = mailBody.ToString();

            Client.Send(message);
        }
    }
}
