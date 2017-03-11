using EpisodeTracker.Notifier;
using System;
using System.Collections.Generic;
using System.Net.Mail;

namespace EpisodeTracker.Storage.NotificationConfig
{
    public class EmailConfiguration : INotifierConfiguration
    {
        public SmtpDeliveryMethod DeliveryMethod { get; set; }

        public string PickupDirectoryLocation { get; set; }

        public string From { get; set; }

        public bool DefaultCredentials { get; set; }

        public bool EnableSsl { get; set; }

        public string Host { get; set; }

        public int Port { get; set; }

        public string Password { get; set; }

        public string UserName { get; set; }

        public List<MailAddress> Recipients { get; set; }

        /// <summary>
        /// Associated Notifier-implementation
        /// </summary>
        /// <returns></returns>
        public Type GetNotifierType()
        {
            return typeof(EmailNotifier);
        }
    }
}