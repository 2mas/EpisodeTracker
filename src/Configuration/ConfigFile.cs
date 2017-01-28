using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;

namespace EpisodeTracker.Configuration
{
    /// <summary>
    /// Class responsible of reading the episodeTracker-section in app.config
    /// </summary>
    internal class ConfigFile : ConfigurationSection
    {
        private static ConfigFile settings = ConfigurationManager.GetSection("episodeTracker") as ConfigFile;

        internal static ConfigFile Settings
        {
            get
            {
                return settings;
            }
        }

        internal ConfigFile()
        {
            NotificationSettings = new NotificationSettingsElement();
            ApiSettings = new ApiSettingsElement();
        }

        [ConfigurationProperty("notificationSettings", IsRequired = true)]
        internal NotificationSettingsElement NotificationSettings
        {
            get { return (NotificationSettingsElement) this["notificationSettings"]; }
            set { this["notificationSettings"] = value; }
        }

        [ConfigurationProperty("apiSettings", IsRequired = true)]
        internal ApiSettingsElement ApiSettings
        {
            get { return (ApiSettingsElement)this["apiSettings"]; }
            set { this["apiSettings"] = value; }
        }

        #region sub-elements
        internal class NotificationSettingsElement : ConfigurationElement
        {
            [ConfigurationProperty("notificationType", IsRequired = true, DefaultValue = Configuration.NotificationType.None)]
            internal NotificationType NotificationType
            {
                get { return (NotificationType) this["notificationType"]; }
                set { this["notificationType"] = value; }
            }

            [ConfigurationProperty("notificationEmails", IsRequired = false, DefaultValue = "")]
            [TypeConverter(typeof(CommaDelimitedStringCollectionConverter))]
            internal StringCollection NotificationEmails
            {
                get {
                    return (StringCollection) this["notificationEmails"];
                }
                set { this["notificationEmails"] = value; }
            }

            internal NotificationSettingsElement()
            {
            }
        }

        internal class ApiSettingsElement : ConfigurationElement
        {
            [ConfigurationProperty("apiUrl", IsRequired = true)]
            internal Uri ApiUrl
            {
                get { return (Uri) this["apiUrl"]; }
                set { this["apiUrl"] = value; }
            }

            [ConfigurationProperty("apiKey", IsRequired = true)]
            internal string ApiKey
            {
                get { return (string)this["apiKey"]; }
                set { this["apiKey"] = value; }
            }

            [ConfigurationProperty("apiUser", IsRequired = true)]
            internal string ApiUser
            {
                get { return (string)this["apiUser"]; }
                set { this["apiUser"] = value; }
            }

            [ConfigurationProperty("apiUserKey", IsRequired = true)]
            internal string ApiUserKey
            {
                get { return (string)this["apiUserKey"]; }
                set { this["apiUserKey"] = value; }
            }
        }
        #endregion
    }
}
