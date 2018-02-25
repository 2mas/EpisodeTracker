using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EpisodeTracker.Storage
{
    /// <summary>
    /// Contains credentials and endpoint to use the Api
    /// </summary>
    public class ApiCredentials
    {
        /// <summary>
        /// Api Endpoint
        /// </summary>
        public string ApiUrl { get; set; }

        /// <summary>
        /// Api key for Api usage
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// Your Api username
        /// </summary>
        public string ApiUser { get; set; }

        /// <summary>
        /// Your Api userkey
        /// </summary>
        public string ApiUserkey { get; set; }

        /// <summary>
        /// Checks that properties are set and that ApiUrl is a well-formed Uri
        /// </summary>
        [JsonIgnore]
        public bool IsValid
        {
            get
            {
                return System.Uri.IsWellFormedUriString(this.ApiUrl, System.UriKind.Absolute)
                    && !String.IsNullOrEmpty(this.ApiKey)
                    && !String.IsNullOrEmpty(this.ApiUser)
                    && !String.IsNullOrEmpty(this.ApiUserkey);
            }
        }

        public Dictionary<string, string> GetValidationErrors()
        {
            var errors = new Dictionary<string, string>();

            if (IsValid)
                return errors;

            if (!System.Uri.IsWellFormedUriString(this.ApiUrl, System.UriKind.Absolute))
                errors.Add("ApiUrl", "Malformed Url");

            if (String.IsNullOrEmpty(this.ApiKey))
                errors.Add("ApiKey", "Missing ApiKey");

            if (String.IsNullOrEmpty(this.ApiUser))
                errors.Add("ApiUser", "Missing ApiUser");

            if (String.IsNullOrEmpty(this.ApiUserkey))
                errors.Add("ApiUserkey", "Missing ApiUserkey");

            return errors;
        }
    }
}