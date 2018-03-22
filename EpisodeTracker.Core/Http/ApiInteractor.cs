using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using EpisodeTracker.Storage;
using Newtonsoft.Json;

namespace EpisodeTracker.Http
{
    /// <summary>
    /// Interacts with TheTVDB API at https://api.thetvdb.com
    /// </summary>
    public class ApiInteractor
    {
        #region members
        private readonly HttpClient Client;

        private readonly ApiCredentials ApiCredentials;

        /// <summary>
        /// Jwt-token validity
        /// </summary>
        private readonly TimeSpan tokenExpiration = new TimeSpan(24, 0, 0);
        #endregion

        public ApiInteractor(HttpClient client, ApiCredentials apiCredentials)
        {
            ApiCredentials = apiCredentials;

            if (!ApiCredentials.IsValid)
            {
                throw new Exceptions.ApiCredentialException(
                    "Invalid ApiCredentials, please set credentials in configfile. See Inner Exception for details",
                    new FormatException(String.Join(", ", ApiCredentials.GetValidationErrors().ToArray()))
                );
            }

            Client = client;
            Client.BaseAddress = new Uri(ApiCredentials.ApiUrl);
            Client.DefaultRequestHeaders.Accept.Clear();
            Client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json")
            );
            Client.DefaultRequestHeaders.AcceptLanguage.Add(
                new StringWithQualityHeaderValue("en-US")
            );
        }

        #region interactions
        internal async Task<List<Series>> SearchSeriesAsync(string searchText)
        {
            var searchHits = new List<Series>();

            HttpResponseMessage response = await Client.GetAsync($"search/series?name={searchText}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return searchHits;

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();
            dynamic responseObject = JsonConvert.DeserializeObject(result);

            foreach (var hit in responseObject.data)
            {
                searchHits.Add(new Series
                {
                    Name = hit.seriesName,
                    FirstAired = hit.firstAired,
                    Id = hit.id,
                    Status = hit.status,
                    Banner = hit.banner,
                    Overview = hit.overview
                });
            }

            return searchHits;
        }

        /// <summary>
        /// Queries API for episodes that has been aired.
        /// Excludes special-episodes (season 0 - optional), faulty dates and future episodes
        /// </summary>
        /// <param name="id"></param>
        /// <param name="page"></param>
        /// <param name="includeSpecials"></param>
        /// <returns>List of all episodes ordered by date with the latest last</returns>
        internal async Task<List<Episode>> GetEpisodesBySeriesIdAsync(long id, int page = 1, bool includeSpecials = false)
        {
            var episodes = new List<Episode>();
            HttpResponseMessage response = await Client.GetAsync($"series/{id}/episodes?page={page}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return episodes;

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();
            dynamic responseObject = JsonConvert.DeserializeObject(result);


            foreach (var episode in responseObject.data)
            {
                // Exclude special-episodes (optional), not yet aired episodes, and faulty dates
                if (!DateTime.TryParse(episode.firstAired.ToString(), out DateTime tmpDate)
                    || tmpDate > DateTime.Now
                    || (episode.airedSeason == 0 && !includeSpecials))
                    continue;

                episodes.Add(new Episode()
                {
                    FirstAired = Convert.ToDateTime(episode.firstAired.ToString()),
                    Id = episode.id,
                    Number = episode.airedEpisodeNumber,
                    Overview = episode.overview,
                    Season = episode.airedSeason,
                    SeasonId = episode.airedSeasonID
                });
            }

            //links: first, last, next, prev
            if (responseObject.links.next != null)
            {
                episodes.AddRange(await GetEpisodesBySeriesIdAsync(id, (int)responseObject.links.next, includeSpecials));
            }

            episodes = episodes.OrderBy(x => x.FirstAired).ToList();

            return episodes;
        }

        internal async Task<Series> SearchSeriesByIdAsync(long id)
        {
            HttpResponseMessage response = await Client.GetAsync($"series/{id}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return new Series();

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();
            dynamic responseObject = JsonConvert.DeserializeObject(result);

            var item = responseObject.data;
            Series series = new Series()
            {
                Name = item.seriesName,
                FirstAired = item.firstAired,
                Id = item.id,
                Status = item.status,
                Banner = item.banner,
                Overview = item.overview,
                ImdbId = item.imdbId,
                AirDay = item.airsDayOfWeek,
                AirTime = item.airsTime,
                TVDBRating = item.siteRating,
                TVDBRatingCount = item.siteRatingCount
            };

            return series;
        }
        #endregion

        #region token
        /// <summary>
        /// Requests a new token from TVDB API, sets authorization-headers and returns the token.
        /// </summary>
        /// <returns>JwtToken with expiration in 24 hrs</returns>
        internal async Task<JwtToken> GetTokenAndSetAuthorizationHeadersAsync()
        {
            object Auth = new
            {
                apikey = ApiCredentials.ApiKey,
                username = ApiCredentials.ApiUser,
                userkey = ApiCredentials.ApiUserkey
            };

            HttpResponseMessage response = await Client.PostAsync(
                "login",
                new StringContent(
                    JsonConvert.SerializeObject(Auth),
                    Encoding.UTF8,
                    "application/json"
                )
            );
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();
            dynamic responseObject = JsonConvert.DeserializeObject(result);

            JwtToken token = new JwtToken();
            token.Token = responseObject.token;
            token.Expiration = DateTime.Now.Add(this.tokenExpiration);

            this.AddTokenToAuthorizationHeader(token);

            return token;
        }

        /// <summary>
        /// Sets token to authorization-header
        /// </summary>
        /// <param name="token"></param>
        internal void AddTokenToAuthorizationHeader(JwtToken token)
        {
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
        }

        #endregion
    }
}
