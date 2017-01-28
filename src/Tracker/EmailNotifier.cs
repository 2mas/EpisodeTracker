﻿using System.Collections.Generic;
using EpisodeTracker.Storage;
using System;
using System.Linq;
using System.Net.Mail;
using EpisodeTracker.Configuration;
using System.Text;

namespace EpisodeTracker
{
    public class EmailNotifier : INotifier
    {
        private static SmtpClient Client = new SmtpClient();
        private static NotificationType NotificationType = ConfigFile.Settings.NotificationSettings.NotificationType;
        private const string NotificationSubject = "EpisodeTracker - New episodes";
        private const string NotificationIntroduction = "New episodes available for series: ";

        public bool SendNotifications(StoreModel storeModel, List<TrackedItem> trackedItems)
        {
            if ((NotificationType & NotificationType.Email) == NotificationType.Email)
            {
                return SendEmail(storeModel, trackedItems);
            }

            return true;
        }

        private static bool SendEmail(StoreModel storeModel, List<TrackedItem> trackedItems)
        {
            List<string> emails = ConfigFile.Settings.NotificationSettings.NotificationEmails.Cast<string>().ToList();

            if (emails.Count() < 1)
                return true;

            try
            {
                MailMessage message = new MailMessage();

                emails.ForEach(x => message.To.Add(x));
                message.Subject = NotificationSubject;

                Func<TrackedItem, string> MakeSeriesSummaryString = (trackedItem) =>
                {
                    return trackedItem.Name + ": " + String.Join(
                            ", ",
                            trackedItem.UnSeenEpisodes.Select(x => x.ToString())
                        );
                };

                StringBuilder mailBody = new StringBuilder(NotificationIntroduction).AppendLine();
                trackedItems.ForEach(x => mailBody.AppendLine(MakeSeriesSummaryString(x)));

                message.Body = mailBody.ToString();

                Client.Send(message);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
