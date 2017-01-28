using EpisodeTracker.Configuration;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Dynamic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using EpisodeTracker;

namespace EpisodeTracker.Http
{
    /// <summary>
    /// Interacts with TheTVDB API at https://api.thetvdb.com
    /// </summary>
    public class ApiInteractor
    {
        #region members
        private static HttpClient Client;

        /// <summary>
        /// Jwt-token validity
        /// </summary>
        private readonly TimeSpan tokenExpiration = new TimeSpan(24, 0, 0);
        #endregion

        public ApiInteractor(HttpClient client)
        {
            Client = client;
            Client.BaseAddress = ConfigFile.Settings.ApiSettings.ApiUrl;
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

            dynamic responseObject = await response.Content.ReadAsAsync<ExpandoObject>();
            foreach(var hit in responseObject.data)
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

            dynamic responseObject = await response.Content.ReadAsAsync<ExpandoObject>();
            DateTime tmpDate;

            foreach (var episode in responseObject.data)
            {
                // Exclude special-episodes (optional), not yet aired episodes, and faulty dates
                if (!DateTime.TryParse(episode.firstAired, out tmpDate)
                    || tmpDate > DateTime.Now
                    || (episode.airedSeason == 0 && !includeSpecials))
                    continue;

                episodes.Add(new Episode()
                {
                    FirstAired = Convert.ToDateTime(episode.firstAired),
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
                episodes.AddRange(await GetEpisodesBySeriesIdAsync(id, (int) responseObject.links.next, includeSpecials));
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

            dynamic responseObject = await response.Content.ReadAsAsync<ExpandoObject>();
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
                apikey = ConfigFile.Settings.ApiSettings.ApiKey,
                username = ConfigFile.Settings.ApiSettings.ApiUser,
                userkey = ConfigFile.Settings.ApiSettings.ApiUserKey
            };

            HttpResponseMessage response = await Client.PostAsJsonAsync("login", Auth);
            response.EnsureSuccessStatusCode();

            dynamic responseObject = await response.Content.ReadAsAsync<ExpandoObject>();

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
