using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using MovieOCD.DTO;
using MovieOCD.Messages.Requests;
using MovieOCD.Messages.Responses;
using System;

namespace MovieOCD.BusinessLogic
{
    public class MovieManager
    {
        public MovieOCDResponse RetrieveMovieData(string movieName, string year)
        {
            var movieOCDResponse = new MovieOCDResponse {RatingProviders = new List<RatingDTO>()};

            var imdbresp = GetMovieInfo(new IMDBServiceMovieRequest { Title = movieName, Year = year }) as IMDBServiceMovieResponse;
            if (imdbresp != null)
                movieOCDResponse.RatingProviders.Add(new RatingDTO { ID = imdbresp.ID, Status = imdbresp.StatusCode, Rating = imdbresp.Rating, DisplayName = Constants.Constants.IMDBDisplayName, MaxRating = Constants.Constants.IMDBMaxRating, LogoUrl = ConfigurationManager.AppSettings["IMDBLogo"] });

            var netflixresp = GetMovieInfo(new NetflixServiceMovieRequest { Title = movieName, Year = year }) as NetflixServiceMovieResponse;
            if (netflixresp != null)
                movieOCDResponse.RatingProviders.Add(new RatingDTO {Status = netflixresp.StatusCode, Rating = netflixresp.Rating, DisplayName = Constants.Constants.NetflixDisplayName, MaxRating = Constants.Constants.NetflixMaxRating, LogoUrl = ConfigurationManager.AppSettings["NetflixLogo"] });

            var rtresp = GetMovieInfo(new RottenTomatoesServiceMovieRequest { Title = movieName, Year = year }) as RottenTomatoesServiceMovieResponse;
            if (rtresp != null)
            {
                movieOCDResponse.RatingProviders.Add(new RatingDTO
                    {
                        ID = rtresp.ID,
                        Status = rtresp.StatusCode,
                        Rating = rtresp.Rating,
                        DisplayName = Constants.Constants.RTDisplayName,
                        MaxRating = Constants.Constants.RTMaxRating,
                        LogoUrl = ConfigurationManager.AppSettings["RTLogo"]
                    });

                TryGetMovieInfoByID(imdbresp, movieOCDResponse, movieName, year, rtresp.IMDBID);
            }

            BuildResponseProperties(new Dictionary<string, BaseServiceMovieResponse> { { Constants.Constants.RTDisplayName, rtresp }, { Constants.Constants.NetflixDisplayName, netflixresp }, { Constants.Constants.IMDBDisplayName, imdbresp } }, movieOCDResponse);
            if (imdbresp != null && imdbresp.ID != Constants.Constants.NotFound)
            {
               movieOCDResponse.EditorPicks = new EditorPickManager().CheckForPick(imdbresp.ID);
            }

          /*  if (imdbresp != null && imdbresp.ID != Constants.Constants.NotFound)
            {
                var ratingresp = new UserRatingManager().GetUserRating(imdbresp.ID);
                movieOCDResponse.RatingProviders.Add( new RatingDTO(){CheckForPick(imdbresp.ID);}
            }*/

            return movieOCDResponse;
        }

        public List<ReviewDTO> RetrieveReviewData(string movieID)
        {
            var rtresp = GetReviewsInfo(new RottenTomatoesServiceReviewRequest { ID = movieID }) as RottenTomatoesServiceReviewResponse;
            return rtresp != null ? rtresp.Reviews : new List<ReviewDTO>();
        }

        private static List<ErrorMessageDTO> MergeStatusMessages(Dictionary<string,BaseServiceMovieResponse> responses)
        {
            var messages = new List<ErrorMessageDTO>
                {
                    new ErrorMessageDTO
                        {
                            Message = responses[Constants.Constants.NetflixDisplayName].Message,
                            Provider = Constants.Constants.NetflixDisplayName
                        },
                    new ErrorMessageDTO
                        {
                            Message = responses[Constants.Constants.IMDBDisplayName].Message,
                            Provider = Constants.Constants.IMDBDisplayName
                        },
                    new ErrorMessageDTO
                        {
                            Message = responses[Constants.Constants.RTDisplayName].Message,
                            Provider = Constants.Constants.RTDisplayName
                        }
                };

            return messages;
        }

        private static BaseServiceMovieResponse GetMovieInfo(BaseServiceRequest request)
        {
            var serviceManager = MovieServiceFactory.GetServiceManagerByRequest(request);
            return serviceManager.Search(request as BaseServiceMovieRequest);
        }

        private static BaseServiceReviewResponse GetReviewsInfo(BaseServiceRequest request)
        {
            var serviceManager = MovieServiceFactory.GetServiceManagerByRequest(request);
            return serviceManager.FindReviews(request as BaseServiceReviewRequest);
        }

        private static string SetResponseProperty(string rt, string nx, string imdb)
        {
            if (!string.IsNullOrEmpty(rt) && rt != Constants.Constants.NotFound)
            {
                return rt;
            }
            if (!string.IsNullOrEmpty(nx) && nx != Constants.Constants.NotFound)
            {
                return nx;
            }
            if (!string.IsNullOrEmpty(imdb) && imdb != Constants.Constants.NotFound)
            {
                return imdb;
            }

            return string.Empty;
        }

        private static void BuildResponseProperties(Dictionary<string,BaseServiceMovieResponse> responses, MovieOCDResponse movieOCDResponse)
        {
            var rtresp = responses[Constants.Constants.RTDisplayName] as RottenTomatoesServiceMovieResponse;
            var netflixresp = responses[Constants.Constants.NetflixDisplayName] as NetflixServiceMovieResponse;
            var imdbresp = responses[Constants.Constants.IMDBDisplayName] as IMDBServiceMovieResponse;

            movieOCDResponse.ID = SetResponseProperty(rtresp.IMDBID, string.Empty, imdbresp.ID);
            movieOCDResponse.LargeImagePath = SetResponseProperty(rtresp.LargeImagePath, string.Empty, imdbresp.ImagePath);
            movieOCDResponse.ImagePath = SetResponseProperty(rtresp.ImagePath, netflixresp.ImagePath, imdbresp.ImagePath);
            movieOCDResponse.Title = SetResponseProperty(rtresp.Title, netflixresp.Title, imdbresp.Title);
            movieOCDResponse.Year = SetResponseProperty(rtresp.Year, netflixresp.Year, imdbresp.Year);
            movieOCDResponse.SuggestedTitles = MergeSuggestedTitles(responses);
            movieOCDResponse.StatusMessages = MergeStatusMessages(responses);
            movieOCDResponse.Director = SetResponseProperty(rtresp.Director, netflixresp.Director, imdbresp.Director);
            movieOCDResponse.PlotSummary = imdbresp.PlotSummary;
        }

        /// <summary>
        /// Shuffles the suggested titles
        /// Combines titles from two provider sources (up to 5 titles) and returns a list without duplicates
        /// </summary>
        /// <param name="providers">The providers.</param>
        /// <returns></returns>
        private static List<SuggestedMovieDTO> MergeSuggestedTitles(IDictionary<string, BaseServiceMovieResponse> providers)
        {
            var suggTitlesListDTO = new List<SuggestedMovieDTO>();
            var rtSuggCount = 0;
            var totalsuggcount = Convert.ToInt32(ConfigurationManager.AppSettings[Constants.Constants.SuggestedTitlesCount]);
            var rtsuggtitles = providers[Constants.Constants.RTDisplayName].SuggestedTitles;

            if (rtsuggtitles != null && rtsuggtitles.Any())
            {
                suggTitlesListDTO.AddRange(rtsuggtitles.Take(totalsuggcount));
                rtSuggCount = rtsuggtitles.Count();
            }

            var nxsuggtitles = providers[Constants.Constants.NetflixDisplayName].SuggestedTitles;
            if (nxsuggtitles != null && nxsuggtitles.Any())
            {
                suggTitlesListDTO.AddRange(nxsuggtitles.Take(totalsuggcount - rtSuggCount));
            }

            return suggTitlesListDTO.Distinct().ToList();  //remove duplicates between providers
        }

        /// <summary>
        /// Method to retrieve IMDB Info if an exact title match wasn't found on initial request. 
        /// Attempts to retrieve based on IMDB ID returned by Rotten Tomatoes response
        /// </summary>
        /// <param name="imdbresponse">The imdbresponse.</param>
        /// <param name="movieOCDResp">The movie OCD resp.</param>
        /// <param name="movieName">Name of the movie.</param>
        /// <param name="year">The year.</param>
        /// <param name="movieid">The movieid.</param>
        private static void TryGetMovieInfoByID(IMDBServiceMovieResponse imdbresponse, MovieOCDResponse movieOCDResp, string movieName, string year, string movieid)
        {
            if (imdbresponse.Rating == Constants.Constants.NotFound && !String.IsNullOrEmpty(movieid)) //OMDB/IMDB API Failed For Exact Title, try to retrieve by IMDBID from RT
            {
                imdbresponse = GetMovieInfo(new IMDBServiceMovieRequest { Title = movieName, Year = year, ID = movieid }) as IMDBServiceMovieResponse;
                if (imdbresponse != null)
                {
                    var existingIMDBDTO = movieOCDResp.RatingProviders.AsParallel().FirstOrDefault(x => x.DisplayName == Constants.Constants.IMDBDisplayName);
                    if (existingIMDBDTO != null)
                    {
                        existingIMDBDTO.ID = imdbresponse.ID;
                        existingIMDBDTO.Status = imdbresponse.StatusCode;
                        existingIMDBDTO.Rating = imdbresponse.Rating;
                        existingIMDBDTO.DisplayName = Constants.Constants.IMDBDisplayName;
                        existingIMDBDTO.MaxRating = Constants.Constants.IMDBMaxRating;
                        existingIMDBDTO.LogoUrl = ConfigurationManager.AppSettings["IMDBLogo"];
                    }
                    else
                    {
                        movieOCDResp.RatingProviders.Add(new RatingDTO
                        {
                            ID = imdbresponse.ID,
                            Status = imdbresponse.StatusCode,
                            Rating = imdbresponse.Rating,
                            DisplayName = Constants.Constants.IMDBDisplayName,
                            MaxRating = Constants.Constants.IMDBMaxRating,
                            LogoUrl = ConfigurationManager.AppSettings["IMDBLogo"]
                        });
                    }
                }
            }
        }
    }
}
