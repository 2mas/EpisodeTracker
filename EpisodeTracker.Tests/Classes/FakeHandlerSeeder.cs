using System;
using System.Net;
using System.Net.Http;
using System.Text;

namespace EpisodeTracker.Tests.Classes
{
    internal static class FakeHandlerSeeder
    {
        static internal void Seed(FakeHttpMessageHandler fakeHandler)
        {
            fakeHandler.AddFakeResponse(
                new Uri("https://api.thetvdb.com/search/series?name=Narcos"),
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(@"
                        {
                          ""data"": [
                            {
                                ""aliases"": [],
                                ""banner"": ""graphical/282670-g.jpg"",
                                ""firstAired"": ""2015-08-28"",
                                ""id"": 282670,
                                ""network"": ""Netflix"",
                                ""overview"": ""The true story of Colombia's infamously violent and powerful drug cartels."",
                                ""seriesName"": ""Narcos"",
                                ""status"": ""Continuing""
                            }
                          ]
                        }",
                        Encoding.UTF8,
                        "application/json"
                    )
                }
            );

            fakeHandler.AddFakeResponse(
                new Uri("https://api.thetvdb.com/series/282670"),
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(@"
                        {
                          ""data"":
                            {
                                ""aliases"": [],
                                ""banner"": ""graphical/282670-g.jpg"",
                                ""firstAired"": ""2015-08-28"",
                                ""id"": 282670,
                                ""imdbId"": ""imdbid"",
                                ""airsDayOfWeek"": ""Wednesday"",
                                ""airsTime"": ""10"",
                                ""siteRating"": 10,
                                ""siteRatingCount"": 10,
                                ""overview"": ""The true story of Colombia's infamously violent and powerful drug cartels."",
                                ""seriesName"": ""Narcos"",
                                ""status"": ""Continuing""
                            }
                        }",
                        Encoding.UTF8,
                        "application/json"
                    )
                }
            );

            fakeHandler.AddFakeResponse(
                new Uri("https://api.thetvdb.com/series/282670/episodes?page=1"),
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(@"
                        {
                        ""links"": {
                            ""first"": 1,
                            ""last"": 3,
                            ""next"": 3,
                            ""prev"": 1
                          },
                          ""data"": [
                            {
                                ""firstAired"": ""2015-08-28"",
                                ""id"": 282670,
                                ""airedEpisodeNumber"": 12,
                                ""overview"": ""The true story of Colombia's infamously violent and powerful drug cartels."",
                                ""airedSeason"": 1,
                                ""airedSeasonID"": 11212
                            },
                            {
                                ""firstAired"": ""2015-0945"",
                                ""id"": 282676,
                                ""airedEpisodeNumber"": 11,
                                ""overview"": ""The true story of Colombia's infamously violent and powerful drug cartels."",
                                ""airedSeason"": 2,
                                ""airedSeasonID"": 112144
                            },
                            {
                                ""firstAired"": ""2015-09-28"",
                                ""id"": 282671,
                                ""airedEpisodeNumber"": 13,
                                ""overview"": ""The true story of Colombia's infamously violent and powerful drug cartels."",
                                ""airedSeason"": 1,
                                ""airedSeasonID"": 11213
                            },
                            {
                                ""firstAired"": ""2015-10-28"",
                                ""id"": 282672,
                                ""airedEpisodeNumber"": 14,
                                ""overview"": ""The true story of Colombia's infamously violent and powerful drug cartels."",
                                ""airedSeason"": 2,
                                ""airedSeasonID"": 11214
                            },
                            {
                                ""firstAired"": ""2018-09-28"",
                                ""id"": 282673,
                                ""airedEpisodeNumber"": 15,
                                ""overview"": ""The true story of Colombia's infamously violent and powerful drug cartels."",
                                ""airedSeason"": 2,
                                ""airedSeasonID"": 11214
                            }
                          ]
                        }",
                        Encoding.UTF8,
                        "application/json"
                    )
                }
            );

            fakeHandler.AddFakeResponse(
                new Uri("https://api.thetvdb.com/login"),
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(@"
                        {
                            ""token"": ""testtoken""
                        }",
                        Encoding.UTF8,
                        "application/json"
                    )
                }
            );
        }
    }
}
