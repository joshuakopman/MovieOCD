using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Web.Script.Serialization;
using MovieOCD.BusinessLogic;
using MovieOCD.DTO;
using MovieOCD.Helpers;
using MovieOCD.Messages.Requests;
using MovieOCD.Messages.Responses;
namespace MovieOCD.ServiceManagers
{
    public class IMDBServiceManager : IServiceManager
    {
        private static string _titleKey = string.Empty;
        private static string _ratingKey = string.Empty;
        private static string _yearKey = string.Empty;
        private static string _idKey = string.Empty;
        private static string _imageKey = string.Empty;
        private static string _directorsKey = string.Empty;
        private static string _plotsummaryKey = string.Empty;
        private readonly string _imdbProvider = string.Empty;
        private readonly string _apiUrl = string.Empty;
        private readonly string _queryStringTitle = string.Empty;
        private readonly string _queryStringID = string.Empty;
        public IMDBServiceManager()
        {
            _imdbProvider = ConfigurationManager.AppSettings["IMDBProvider"];
            _apiUrl = ConfigurationManager.AppSettings[_imdbProvider + "Url"];
            switch (_imdbProvider)
            {
                case Constants.Constants.OMDBAPIProvider:
                    _titleKey = Constants.Constants.omdbtitleKey;
                    _plotsummaryKey = Constants.Constants.omdbplotsummarykey;
                    _ratingKey = Constants.Constants.omdbratingKey;
                    _yearKey = Constants.Constants.omdbyearKey;
                    _idKey = Constants.Constants.omdbidKey;
                    _imageKey = Constants.Constants.omdbimageKey;
                    _directorsKey = Constants.Constants.omdbdirectorkey;
                    _queryStringTitle = ConfigurationManager.AppSettings["OMDBTitleQS"];
                    _queryStringID = ConfigurationManager.AppSettings["OMDBIDQS"];
                    break;
                case Constants.Constants.IMDBAPIProvider:
                    _titleKey = Constants.Constants.imdbtitleKey;
                    _plotsummaryKey = Constants.Constants.imdbplotsummarykey;
                    _ratingKey = Constants.Constants.imdbratingKey;
                    _yearKey = Constants.Constants.imdbyearKey;
                    _idKey = Constants.Constants.imdbidKey;
                    _imageKey = Constants.Constants.imdbimageKey;
                    _directorsKey = Constants.Constants.imdbdirectorkey;
                    _queryStringTitle = ConfigurationManager.AppSettings["IMDBTitleQS"];
                    _queryStringID = ConfigurationManager.AppSettings["IMDBIDQS"];
                    break;
            }
        }

        public BaseServiceMovieResponse Search(BaseServiceMovieRequest baseServiceMovieRequest)
        {
            var imdbServiceMovieRequest = baseServiceMovieRequest as IMDBServiceMovieRequest;
            if (imdbServiceMovieRequest != null)
            {
                string requestedtitle = imdbServiceMovieRequest.Title;
                string year = imdbServiceMovieRequest.Year;
                var imdbServiceMovieResponse = CacheManager.Instance.CheckForEntryInCache((string.IsNullOrEmpty(year)) ? requestedtitle : requestedtitle + year, Constants.Constants.IMDBCACHEKEY) as IMDBServiceMovieResponse;

                if (imdbServiceMovieResponse != null && imdbServiceMovieResponse.ID != Constants.Constants.NotFound) //if movie is cached and not a previous service failure
                {
                    return imdbServiceMovieResponse;
                }

                imdbServiceMovieResponse = new IMDBServiceMovieResponse();

                var imdbApiResp = GetResponseFromAPI(requestedtitle, imdbServiceMovieRequest.ID);

                if (imdbApiResp != null)
                {
                    var errorCheck = imdbApiResp as Dictionary<string, object>;
                    if (errorCheck != null && (errorCheck.ContainsKey(Constants.Constants.IMDBError) || errorCheck.ContainsKey(Constants.Constants.IMDBError.ToLower()))) 
                    {
                        ClearResponse(imdbServiceMovieResponse);
                        imdbServiceMovieResponse.Message = Constants.Constants.MovieNotFound;
                        imdbServiceMovieResponse.StatusCode = Constants.Constants.Fail;
                        return imdbServiceMovieResponse;
                    }

                    var matchingMovies = new Dictionary<string, object>();
                    var suggestedMovies = new List<SuggestedMovieDTO>();

                    FindMatchingMovies(matchingMovies, suggestedMovies, imdbApiResp, requestedtitle, year);

                    imdbServiceMovieResponse.Title = ServiceManagerHelper.SetResponseProperty(matchingMovies, _titleKey);
                    imdbServiceMovieResponse.PlotSummary = ServiceManagerHelper.SetResponseProperty(matchingMovies, _plotsummaryKey);
                    imdbServiceMovieResponse.Rating = ServiceManagerHelper.SetResponseProperty(matchingMovies, _ratingKey);
                    imdbServiceMovieResponse.ID = ServiceManagerHelper.SetResponseProperty(matchingMovies, _idKey).Replace("tt","");
                    imdbServiceMovieResponse.Year = ServiceManagerHelper.SetResponseProperty(matchingMovies, _yearKey);
                    imdbServiceMovieResponse.Message = (imdbServiceMovieResponse.Title == Constants.Constants.NotFound) ? Constants.Constants.MovieNotFound : Constants.Constants.Success;
                    imdbServiceMovieResponse.SuggestedTitles = suggestedMovies;
                    imdbServiceMovieResponse.StatusCode = (imdbServiceMovieResponse.Title == Constants.Constants.NotFound) ? Constants.Constants.Fail : Constants.Constants.Pass;
                    imdbServiceMovieResponse.ImagePath = ServiceManagerHelper.SetResponseProperty(matchingMovies, _imageKey);
                    imdbServiceMovieResponse.Director = ServiceManagerHelper.SetResponseProperty(matchingMovies, _directorsKey);
                    CacheManager.Instance.InsertEntryInCache(imdbServiceMovieResponse, (string.IsNullOrEmpty(year)) ? requestedtitle : requestedtitle + year, Constants.Constants.IMDBCACHEKEY);

                }
                else
                {
                    ClearResponse(imdbServiceMovieResponse);
                }

                return imdbServiceMovieResponse;
            }

            return new IMDBServiceMovieResponse();
        }

        public void ClearResponse(BaseServiceMovieResponse resp)
        {
            var imdb = resp as IMDBServiceMovieResponse;
            if (imdb != null)
            {
                imdb.Rating = Constants.Constants.NotFound;
                imdb.Title = Constants.Constants.NotFound;
                imdb.Message = Constants.Constants.MovieNotFound;
                imdb.ID = Constants.Constants.NotFound;
                imdb.Year = Constants.Constants.NotFound;
                imdb.StatusCode = Constants.Constants.Fail;
            }
        }

        public BaseServiceReviewResponse FindReviews(BaseServiceReviewRequest req)
        {
            throw new NotImplementedException();
        }

        private void FindMatchingMovies(Dictionary<string, object> matchingMovies, List<SuggestedMovieDTO> suggestedMovies, dynamic imdbAPIResp, string requestedtitle, string year)
        {
            switch (_imdbProvider)
            {
                case Constants.Constants.IMDBAPIProvider:
                    FindMatchingIMDBAPIMovies(matchingMovies, suggestedMovies, imdbAPIResp, requestedtitle, year);
                    break;
                case Constants.Constants.OMDBAPIProvider:
                    FindMatchingOMDBAPIMovies(matchingMovies, suggestedMovies, imdbAPIResp, requestedtitle, year);
                    break;
            }
        }

        private static void FindMatchingIMDBAPIMovies(Dictionary<string, object> matchingMovies,
                                                      List<SuggestedMovieDTO> suggestedMovies, dynamic imdbAPIResp,
                                                      string requestedtitle, string year)
        {
            var suggestedNum =
                Convert.ToInt32(ConfigurationManager.AppSettings[Constants.Constants.SuggestedTitlesCount]);
            int suggCount = 0;
            bool foundMovie = false;
            var imdbAPIRespDict = imdbAPIResp as Dictionary<string, object>;
            if (imdbAPIRespDict == null)
            {
                foreach (var movie in imdbAPIResp)
                {
                    if (foundMovie == false && ServiceManagerHelper.StripPunctuation(movie[_titleKey].ToString().ToLower()) == ServiceManagerHelper.StripPunctuation(requestedtitle.ToLower()) &&
                        ((!string.IsNullOrEmpty(year) && year == movie[_yearKey].ToString()) ||
                         string.IsNullOrEmpty(year))) //if a year was provided, make sure it matches
                    {
                        DebugManager.LogWarning("Movie Match Found for: " + requestedtitle);
                        try
                        {
                            var director = string.Empty;
                            foreach (var d in movie[_directorsKey])
                            {
                                director = d;
                                break;
                            }
                            movie[_directorsKey] = director;
                            SetMovieProperties(matchingMovies, movie);
                            foundMovie = true;
                        }
                        catch (Exception ex)
                        {
                            DebugManager.LogException(ex);
                        }
                        //likely due to one of the movies missing a json property despite having a title...thanks a lot IMDB API
                    }
                    else if (suggCount < suggestedNum)
                    {
                        var dictmovie = movie as Dictionary<string, object>;
                        suggestedMovies.Add(new SuggestedMovieDTO
                            {
                                Title =  (dictmovie != null && dictmovie.ContainsKey(_titleKey)) ? movie[_titleKey] : string.Empty,
                                Year = (dictmovie != null && dictmovie.ContainsKey(_yearKey)) ? dictmovie[_yearKey].ToString() : string.Empty
                            });
                        suggCount++;
                    }
                }
            }
            else //search by id, only one movie result back
            {
                DebugManager.LogWarning("Movie Match Found for: " + requestedtitle);
                try
                {

                    var directorDictionary = imdbAPIRespDict[_directorsKey] as Object[];
                    if (directorDictionary != null) imdbAPIRespDict[_directorsKey] = directorDictionary[0];
                    SetMovieProperties(matchingMovies, imdbAPIRespDict);
                }
                catch (Exception ex)
                {
                    DebugManager.LogException(ex);
                }
                //likely due to one of the movies missing a json property despite having a title...thanks a lot IMDB API
            }
        }

        private static void FindMatchingOMDBAPIMovies(Dictionary<string, object> matchingMovies, List<SuggestedMovieDTO> suggestedMovies, dynamic imdbAPIResp, string requestedtitle, string year)
        {
            var foundMovie = imdbAPIResp as Dictionary<string, object>;
            if (foundMovie != null && foundMovie.Count > 0)
            {
                if (foundMovie[_titleKey].ToString().ToLower() == requestedtitle.ToLower() && ((!string.IsNullOrEmpty(year) && year == foundMovie[_yearKey].ToString()) || string.IsNullOrEmpty(year))) //if a year was provided, make sure it matches
                {
                    DebugManager.LogWarning("Movie Match Found for: " + requestedtitle);
                    try
                    {
                        SetMovieProperties(matchingMovies, foundMovie);
                    }
                    catch (Exception ex)
                    {
                        DebugManager.LogException(ex);
                    } 
                }
                else
                {
                    suggestedMovies.Add(new SuggestedMovieDTO { Title = foundMovie[_titleKey].ToString(), Year = foundMovie[_yearKey].ToString() });
                }
            }
        }

        private static void SetMovieProperties(Dictionary<string,object> movies, dynamic foundMovie)
        {
            try
            {
                ServiceManagerHelper.AddIfKeyDoesntExist(movies, _titleKey, foundMovie[_titleKey]);
                ServiceManagerHelper.AddIfKeyDoesntExist(movies, _ratingKey, foundMovie[_ratingKey]);
                ServiceManagerHelper.AddIfKeyDoesntExist(movies, _idKey, foundMovie[_idKey]);
                ServiceManagerHelper.AddIfKeyDoesntExist(movies, _yearKey, foundMovie[_yearKey]);
                ServiceManagerHelper.AddIfKeyDoesntExist(movies, _imageKey, foundMovie[_imageKey]);
                ServiceManagerHelper.AddIfKeyDoesntExist(movies, _directorsKey, foundMovie[_directorsKey]);
                ServiceManagerHelper.AddIfKeyDoesntExist(movies,_plotsummaryKey,foundMovie[_plotsummaryKey]);
            }
            catch (Exception ex)
            {
                DebugManager.LogException(ex);
            }
        }

        private dynamic GetResponseFromAPI(string title, string id = "")
        {
            var request = (String.IsNullOrEmpty(id)) ? WebRequest.Create(_apiUrl + _queryStringTitle + title + "&type=json&limit=15") : 
                                                       WebRequest.Create(_apiUrl + _queryStringID + id + "&type=json&limit=15");

            request.ContentType = "application/json";
            request.Method = "GET";

            try
            {
                string jsonText = ServiceManagerHelper.BuildResponse(request);

                if (!string.IsNullOrEmpty(jsonText))
                {
                    return new JavaScriptSerializer().Deserialize<dynamic>(jsonText);
                }
            }
            catch (Exception ex) 
            {
                DebugManager.LogException(ex);
            }

            return null;
        }
    }
}
