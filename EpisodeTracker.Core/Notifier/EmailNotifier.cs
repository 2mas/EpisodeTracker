using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EpisodeTracker.Storage;
using EpisodeTracker.Storage.NotificationConfig;
using MailKit.Net.Smtp;
using MimeKit;

namespace EpisodeTracker.Notifier
{
    public class EmailNotifier : INotifier
    {
        public static EmailConfiguration Configuration;
        private static List<MailboxAddress> Recipients = new List<MailboxAddress>();
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

            Configuration.Recipients.ForEach(r => Recipients.Add(new MailboxAddress(r)));
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

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(Configuration.From));
            Recipients.ForEach(x => message.To.Add(x));
            message.Subject = NotificationSubject;

            StringBuilder mailBody = new StringBuilder(NotificationIntroduction).AppendLine();
            trackedItems.ForEach(x => mailBody.AppendLine(MakeSeriesSummaryString(x)));
            message.Body = new TextPart("plain")
            {
                Text = mailBody.ToString()
            };

            using (var client = new SmtpClient())
            {
                // For demo-purposes, accept all SSL certificates (in case the server supports STARTTLS)
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                client.Connect(Configuration.Host, Configuration.Port, false);
                client.AuthenticationMechanisms.Remove("XOAUTH2");

                if (!Configuration.DefaultCredentials)
                {
                    client.Authenticate(Configuration.UserName, Configuration.Password);
                }

                client.Send(message);
                client.Disconnect(true);
            }

        }
    }
}
