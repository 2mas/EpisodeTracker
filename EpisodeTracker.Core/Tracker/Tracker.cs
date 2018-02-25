using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using EpisodeTracker.Http;
using EpisodeTracker.Notifier;
using EpisodeTracker.Storage;

namespace EpisodeTracker
{
    /// <summary>
    /// Main Tracker-object that exposes all available operations to user of library
    /// </summary>
    public class Tracker
    {
        #region members
        /// <summary>
        /// ApiInteractor communicates with the API through HttpClient
        /// </summary>
        private ApiInteractor ApiInteractor;

        /// <summary>
        /// Storage for tracked series and time-limited API-token
        /// </summary>
        public IStorage Storage;

        /// <summary>
        /// Sends notifications when new episodes are available
        /// </summary>
        private List<INotifier> Notifiers;

        /// <summary>
        /// Current token for Api-usage
        /// </summary>
        private JwtToken Token;

        /// <summary>
        /// Holds temporary data such as previous searchresults
        /// </summary>
        public TemporaryData TmpData { get; private set; }
        #endregion

        /// <summary>
        /// Creates the main tracker object
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="storage">Choice of storage</param>
        public Tracker(HttpClient httpClient, IStorage storage)
        {
            this.Storage = storage;
            this.TmpData = new TemporaryData();

            SetupNotifications();

            this.ApiInteractor = new ApiInteractor(httpClient, this.Storage.GetStoreModel().ApiCredentials);

            // Check for token in storage
            if (HasValidTokenInStorage())
            {
                SetValidTokenFromStorage();
            }
        }

        /// <summary>
        /// Loads and setups user-implementations of Notifiers, defined in config
        /// </summary>        
        private void SetupNotifications() {
            this.Notifiers = new List<INotifier>();

            // Add notifiers based on user-configuration
            if (this.Storage.GetStoreModel().NotificationSettings.Configurations.Any())
            {
                this.Storage.GetStoreModel().NotificationSettings.Configurations.ForEach(
                    notificationConfig =>
                {
                    if (notificationConfig.GetNotifierType().GetInterface(typeof(INotifier).Name) != null)
                    {
                        this.Notifiers.Add((INotifier)Activator.CreateInstance(notificationConfig.GetNotifierType()));
                    }
                });

                if (this.Notifiers.Any())
                {
                    this.Notifiers.ForEach(x => x.Setup(this.Storage.GetStoreModel()));
                }
            }
        }

        #region commands
        /// <summary>
        /// Gets a search-result with basic info of each series
        /// </summary>
        /// <param name="searchText"></param>
        /// <returns></returns>
        public async Task<List<Series>> SearchSeriesAsync(string searchText)
        {
            CheckTokenBeforeApiCall();

            List<Series> result = await this.ApiInteractor.SearchSeriesAsync(searchText);
            TmpData.LatestSearch = result;

            return result;
        }

        /// <summary>
        /// View more information on a specific series
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Series> ViewSeriesInformationByIdAsync(long id)
        {
            CheckTokenBeforeApiCall();
            Series seriesResult = await this.ApiInteractor.SearchSeriesByIdAsync(id);
            List<Episode> episodes = await this.ApiInteractor.GetEpisodesBySeriesIdAsync(id);

            seriesResult.AiredSeasons = String.Join(", ", episodes.Select(x => x.Season).Distinct());
            seriesResult.AiredEpisodes = episodes.Count;
            //await this.ApiInteractor.SetEpisodeSummaryBySeriesAsync(seriesResult);
            TmpData.LatestViewById = seriesResult;
            return seriesResult;
        }

        /// <summary>
        /// Gets all episodes in this series
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<List<Episode>> ViewEpisodesBySeriesIdAsync(long id)
        {
            CheckTokenBeforeApiCall();
            List<Episode> episodesResult = await this.ApiInteractor.GetEpisodesBySeriesIdAsync(id);
            return episodesResult;
        }

        /// <summary>
        /// Queries API for aired episodes and compares to earlier synced episodes.
        /// </summary>
        public async Task<List<TrackedItem>> CheckForNewEpisodesAsync()
        {
            List<TrackedItem> hasNewEpisodes = new List<TrackedItem>();
            List<TrackedItem> trackedItems = this.GetTrackedItems();

            if (!trackedItems.Any())
                return hasNewEpisodes;

            CheckTokenBeforeApiCall();

            List<Tuple<TrackedItem, Task<List<Episode>>>> tasks = new List<Tuple<TrackedItem, Task<List<Episode>>>>();

            foreach (TrackedItem trackedItem in trackedItems)
            {
                tasks.Add(
                    new Tuple<TrackedItem, Task<List<Episode>>>(
                        trackedItem,
                        this.ApiInteractor.GetEpisodesBySeriesIdAsync(trackedItem.SeriesId))
                    );
            }

            await Task.WhenAll(tasks.Select(x => x.Item2).ToArray());

            foreach (var task in tasks)
            {
                List<Episode> episodes = task.Item2.Result;
                var trackedItem = task.Item1;

                if (episodes.Count > trackedItem.TotalAiredEpisodes
                    && trackedItem.TrackingPoint < Convert.ToDateTime(episodes.Last().FirstAired))
                {
                    trackedItem.UnSeenEpisodes = episodes.Skip(trackedItem.TotalSeenEpisodes).ToList();
                    trackedItem.TotalAiredEpisodes = episodes.Count;
                    hasNewEpisodes.Add(trackedItem);
                }
            }

            return hasNewEpisodes;
        }

        /// <summary>
        /// Sends notifications with configured notifiers
        /// </summary>
        /// <param name="updatedSeries"></param>
        /// <returns></returns>
        public bool SendNotifications(List<TrackedItem> updatedSeries)
        {
            this.Notifiers.ForEach(x => x.SendNotifications(updatedSeries));
            return true;
        }

        /// <summary>
        /// Creates an initial tracking of a series, setting all aired episodes as unseen
        /// </summary>
        /// <param name="seriesId"></param>
        /// <returns></returns>
        public async Task Track(long seriesId)
        {
            if (!this.GetTrackedItems().Exists(d => d.SeriesId == seriesId))
            {
                CheckTokenBeforeApiCall();
                Series seriesToTrack;

                if (TmpData.LatestSearch != null && TmpData.LatestSearch.Any(d => d.Id == seriesId))
                {
                    seriesToTrack = TmpData.LatestSearch.First(d => d.Id == seriesId);
                }
                else if (TmpData.LatestViewById?.Id == seriesId)
                {
                    seriesToTrack = TmpData.LatestViewById;
                }
                else
                {
                    seriesToTrack = await this.ApiInteractor.SearchSeriesByIdAsync(seriesId);
                }

                // Get total count of episodes from this point
                List<Episode> episodes = await this.ApiInteractor.GetEpisodesBySeriesIdAsync(seriesId);

                this.Storage.GetStoreModel().TrackedItems.Add(new TrackedItem()
                {
                    SeriesId = seriesId,
                    TrackingPoint = DateTime.MinValue,
                    Name = seriesToTrack.Name,
                    Status = seriesToTrack.Status,
                    TotalSeenEpisodes = 0,
                    TotalAiredEpisodes = episodes.Count,
                    UnSeenEpisodes = episodes
                });

                SaveStoreModel();
            }
        }

        public void UnTrack(long seriesId)
        {
            if (this.GetTrackedItems().Exists(d => d.SeriesId == seriesId))
            {
                TrackedItem item = this.Storage.GetStoreModel().TrackedItems.First(d => d.SeriesId == seriesId);
                this.Storage.GetStoreModel().TrackedItems.Remove(item);
            }

            SaveStoreModel();
        }

        public List<TrackedItem> GetTrackedItems()
        {
            return this.Storage.GetStoreModel().TrackedItems.OrderBy(d => d.Name).ToList();
        }

        #endregion

        public void SaveStoreModel()
        {
            this.Storage.Save();
        }

        /// <summary>
        /// Marks episodes as seen for tracked items that contains them
        /// </summary>
        /// <param name="episodeIds"></param>
        public void MarkEpisodesAsSeen(long[] episodeIds)
        {
            var trackedItems = this.GetTrackedItems()
                .Where(x => x.UnSeenEpisodes.Exists(d => episodeIds.Contains(d.Id))).ToList();

            if (trackedItems.Count > 0)
            {
                trackedItems.ForEach(item =>
                {
                    item.TotalSeenEpisodes += item.UnSeenEpisodes.Count(x => episodeIds.Contains(x.Id));
                    item.UnSeenEpisodes.RemoveAll(x => episodeIds.Contains(x.Id));
                });

                this.SaveStoreModel();
            }
        }

        /// <summary>
        /// Update TrackingPoint to the latest episode and save to storage
        /// </summary>
        /// <param name="hasNewEpisodes"></param>
        public void UpdateTrackingPoint(List<TrackedItem> hasNewEpisodes)
        {
            hasNewEpisodes.ForEach(
                x =>
                {
                    x.TrackingPoint = x.UnSeenEpisodes.Last().FirstAired;
                }
            );

            SaveStoreModel();
        }

        #region token-handling
        /// <summary>
        /// Is run before every API-call to make sure the token is valid.
        /// If not, gets a token from API, sets its to API and saves it to storeModel
        /// </summary>
        private void CheckTokenBeforeApiCall()
        {
            if (this.Token == null || !this.Token.IsValid())
            {
                GetTokenFromAPI();
                SaveStoreModel();
            }
        }

        /// <summary>
        /// Checks in storage for a valid token to use
        /// </summary>
        /// <returns></returns>
        private bool HasValidTokenInStorage()
        {
            if (this.Storage.GetStoreModel().Token != null)
            {
                return !String.IsNullOrEmpty(this.Storage.GetStoreModel().Token.Token)
                    && this.Storage.GetStoreModel().Token.Expiration > DateTime.Now;
            }

            return false;
        }

        /// <summary>
        /// Sets the token to use from storage and sets it to be used by API
        /// </summary>
        private void SetValidTokenFromStorage()
        {
            this.Token = new JwtToken();
            this.Token.Token = this.Storage.GetStoreModel().Token.Token;
            this.Token.Expiration = this.Storage.GetStoreModel().Token.Expiration;

            this.ApiInteractor.AddTokenToAuthorizationHeader(this.Token);
        }

        /// <summary>
        /// Gets a token from API and sets it to the token-property of the EpisodeTracker
        /// </summary>
        private void GetTokenFromAPI()
        {
            this.Token = this.ApiInteractor.GetTokenAndSetAuthorizationHeadersAsync().Result;
            this.Storage.GetStoreModel().Token.Token = this.Token.Token;
            this.Storage.GetStoreModel().Token.Expiration = this.Token.Expiration;
        }
        #endregion
    }
}
