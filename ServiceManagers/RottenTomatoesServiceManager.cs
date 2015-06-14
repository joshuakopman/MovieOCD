using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Web.Script.Serialization;
using MovieOCD.BusinessLogic;
using MovieOCD.DTO;
using MovieOCD.Helpers;
using MovieOCD.Messages.Requests;
using MovieOCD.Messages.Responses;

namespace MovieOCD.ServiceManagers
{
    public class RottenTomatoesServiceManager : IServiceManager
    {
        public BaseServiceMovieResponse Search(BaseServiceMovieRequest baseServiceMovieRequest)
        {
            var title = baseServiceMovieRequest.Title;
            var year = baseServiceMovieRequest.Year;
            var rtServiceResponse = CacheManager.Instance.CheckForEntryInCache((string.IsNullOrEmpty(year)) ? title : title + year, Constants.Constants.RTCACHEKEY) as RottenTomatoesServiceMovieResponse;

            if (rtServiceResponse != null) //cache hit
            {
                return rtServiceResponse;
            }

            rtServiceResponse = new RottenTomatoesServiceMovieResponse();

            var responseDictionary = CreateRequestAndDeserializeResponse(title, "Movie");

            var imdbID = ServiceManagerHelper.GetLinkedIMDBID(title, year);

            if (responseDictionary != null && responseDictionary.ContainsKey("movies"))
            {
                var filteredMovies = FilterResultsByLinkedID(responseDictionary["movies"] as ArrayList, imdbID, title, year);

                if (filteredMovies != null)
                 {
                     var altIds = new Dictionary<string,object>();
                     if (filteredMovies.ExactMatches != null && filteredMovies.ExactMatches.ContainsKey("alternate_ids"))
                     {
                         altIds = filteredMovies.ExactMatches["alternate_ids"] as Dictionary<string, object>;
                     }
                     if (altIds != null && (imdbID != Constants.Constants.NotFound && altIds.ContainsKey("imdb") && !imdbID.Contains(altIds["imdb"].ToString())))
                     {
                        ClearResponse(rtServiceResponse);
                        rtServiceResponse.Message = "No Returned RT Movies Matched IMDB IDs";
                        return rtServiceResponse;
                     }

                     var movieFoundRatings = (filteredMovies.ExactMatches!= null && filteredMovies.ExactMatches.ContainsKey("ratings")) ? filteredMovies.ExactMatches["ratings"] as Dictionary<string, object> : new Dictionary<string, object>();
                     var movieFoundImages = (filteredMovies.ExactMatches!= null && filteredMovies.ExactMatches.ContainsKey("posters")) ? filteredMovies.ExactMatches["posters"] as Dictionary<string, object> : new Dictionary<string, object>();

                    rtServiceResponse.ID = ServiceManagerHelper.SetResponseProperty(filteredMovies.ExactMatches, "id");
                    rtServiceResponse.Title = ServiceManagerHelper.SetResponseProperty(filteredMovies.ExactMatches, "title");
                    rtServiceResponse.Year = ServiceManagerHelper.SetResponseProperty(filteredMovies.ExactMatches, "year");
                    rtServiceResponse.Rating = ServiceManagerHelper.SetResponseProperty(movieFoundRatings, "critics_score");
                    rtServiceResponse.ImagePath = ServiceManagerHelper.SetResponseProperty(movieFoundImages, "profile");
                    rtServiceResponse.LargeImagePath = ServiceManagerHelper.SetResponseProperty(movieFoundImages, "original");
                    rtServiceResponse.SuggestedTitles = (filteredMovies.SuggestedTitles != null && filteredMovies.SuggestedTitles.Any()) ? CreateSuggestedMovieDTO(filteredMovies.SuggestedTitles) : null;
                    rtServiceResponse.Message = (rtServiceResponse.Title != Constants.Constants.NotFound) ? Constants.Constants.Success : Constants.Constants.MovieNotFound;
                    rtServiceResponse.StatusCode = (rtServiceResponse.Title != Constants.Constants.NotFound) ? Constants.Constants.Pass : Constants.Constants.Fail;
                    rtServiceResponse.IMDBID = altIds != null && (altIds.ContainsKey("imdb")) ? altIds["imdb"].ToString() : Constants.Constants.NotFound;
                    CacheManager.Instance.InsertEntryInCache(rtServiceResponse, (string.IsNullOrEmpty(year)) ? title : title + year, Constants.Constants.RTCACHEKEY);

                    return rtServiceResponse;
                 }
            }

            ClearResponse(rtServiceResponse);
            rtServiceResponse.Message = Constants.Constants.MovieNotFound;

            return rtServiceResponse;
        }

        private List<SuggestedMovieDTO> CreateSuggestedMovieDTO(IEnumerable<Dictionary<string, object>> suggestedTitles)
        {
            return suggestedTitles.Select(MapSuggestedTitlesPOCOToDTO).ToList();
        }

        private SuggestedMovieDTO MapSuggestedTitlesPOCOToDTO(Dictionary<string, object> suggestedTitle)
        {
            return new SuggestedMovieDTO()
                {
                    Title = suggestedTitle["title"].ToString(),
                    Year = suggestedTitle["year"].ToString()
                };
        }

        public void ClearResponse(BaseServiceMovieResponse resp)
        {
            var rtresp = resp as RottenTomatoesServiceMovieResponse;
            if (rtresp == null) return;
            rtresp.Title = Constants.Constants.NotFound;
            rtresp.Rating = Constants.Constants.NotFound;
            rtresp.ImagePath = Constants.Constants.NotFound;
            rtresp.Year = Constants.Constants.NotFound;
            rtresp.SuggestedTitles = null;
            rtresp.StatusCode = Constants.Constants.Fail;
        }

        public BaseServiceReviewResponse FindReviews(BaseServiceReviewRequest req)
        {
            var rtreq = req as RottenTomatoesServiceReviewRequest;
            if (rtreq != null)
            {
                var dict = CreateRequestAndDeserializeResponse(rtreq.ID, "review");
                var rtResponse = new RottenTomatoesServiceReviewResponse {Reviews = new List<ReviewDTO>()};
                if (dict != null && dict.ContainsKey("reviews"))
                {
                    var allReviews = dict["reviews"] as ArrayList;
                    if (allReviews != null)
                    {
                        var allReviewDict = allReviews.Cast<Dictionary<string, object>>();
                        int revCount = Convert.ToInt32(ConfigurationManager.AppSettings["ReviewsCount"]);
                        int currReviewCount = 0;
                        foreach (var rev in allReviewDict)
                        {
                            if (rev["quote"].ToString().Length <= 0) continue;
                            var author = !string.IsNullOrEmpty(rev["critic"].ToString()) ? rev["critic"].ToString() : rev["publication"].ToString();
                            if (string.IsNullOrEmpty(author)) continue;
                            if (currReviewCount < revCount)
                            {
                                rtResponse.Reviews.Add(new ReviewDTO { Critic = author, Review = rev["quote"].ToString() });
                                currReviewCount++;
                            }
                        }
                    }
                }
                rtResponse.ID = rtreq.ID;
                return rtResponse;
            }
            return new RottenTomatoesServiceReviewResponse();
        }

        private static Dictionary<string, object> CreateRequestAndDeserializeResponse(string title, string type)
        {
            var rtAPIKey = ConfigurationManager.AppSettings["RTAPIKey"];
            var request = (type == "Movie") ? WebRequest.Create(ConfigurationManager.AppSettings["RTMovieAPIUrl"] + rtAPIKey + "&q=" + title + "&page_limit = 1")
                                            : WebRequest.Create(String.Format(ConfigurationManager.AppSettings["RTReviewAPIUrl"] + rtAPIKey, title));
            request.ContentType = "application/json";
            request.Method = "GET";

            try
            {
                string jsonText = ServiceManagerHelper.BuildResponse(request);

                if (!string.IsNullOrEmpty(jsonText))
                {
                    return new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(jsonText);
                }
            }
            catch (Exception ex) 
            {
                DebugManager.LogException(ex);
            }

            return null;
        }

        private static RottenTomatoesPOCO FilterResultsByLinkedID(ArrayList movies, string linkedID, string titleToFind, string year = "")
        {
            IEnumerable<Dictionary<string, object>> matchedMovies;
            IEnumerable<Dictionary<string, object>> suggestedMovies;
            var allMovies = movies.Cast<Dictionary<string, object>>();
            var rtPOCO = new RottenTomatoesPOCO();

            if (linkedID != Constants.Constants.NotFound) //find by linkedIMDB ID
            {
                matchedMovies = from matchingMovies in allMovies.AsParallel()                          
                                let dictionary =  matchingMovies.ContainsKey("alternate_ids") ? matchingMovies["alternate_ids"] as Dictionary<string, object>: null
                                where  dictionary != null && dictionary.ContainsKey("imdb") && linkedID.Contains(dictionary["imdb"].ToString())
                                select matchingMovies;

                if (matchedMovies.Any())
                {
                    suggestedMovies = from nonmatchingMovies in allMovies.AsParallel()  
                                      let dictionary = nonmatchingMovies.ContainsKey("alternate_ids") ? nonmatchingMovies["alternate_ids"] as Dictionary<string, object> : null
                                      where dictionary != null && dictionary.ContainsKey("imdb") && !linkedID.Contains(dictionary["imdb"].ToString())
                                      select nonmatchingMovies;

                    rtPOCO.ExactMatches = matchedMovies.First();
                    rtPOCO.SuggestedTitles = suggestedMovies.Take(Convert.ToInt32(ConfigurationManager.AppSettings[Constants.Constants.SuggestedTitlesCount]));

                    return rtPOCO;
                }
            }
            DebugManager.LogWarning("Linked IMDB ID Not Found: Doing title match For Rotten Tomatoes for "+titleToFind);

            rtPOCO.SuggestedTitles = allMovies.AsParallel().Where(x => x.ContainsKey("title") && x["title"].ToString().ToLower() != titleToFind.ToLower());

            if (year == "")
            {
                rtPOCO.ExactMatches = allMovies.AsParallel().FirstOrDefault(
                        x => x.ContainsKey("title") && ServiceManagerHelper.StripPunctuation(x["title"].ToString().ToLower()) == ServiceManagerHelper.StripPunctuation(titleToFind.ToLower()));
                return rtPOCO;
            }
            rtPOCO.ExactMatches=
              allMovies.AsParallel().FirstOrDefault(
                  x => x.ContainsKey("title") && ServiceManagerHelper.StripPunctuation(x["title"].ToString().ToLower()) == ServiceManagerHelper.StripPunctuation(titleToFind.ToLower()) && x.ContainsKey("year") && x["year"].ToString() == year);
            return rtPOCO;
        }
 
     } 

    public class RottenTomatoesPOCO
    {
            public Dictionary<string, object> ExactMatches { get; set; }
            public IEnumerable<Dictionary<string, object>> SuggestedTitles { get; set; }
    }
}

//Suggested title logic
/*    var suggestedMovies = from nonmatchingMovies in allMovies.Cast<Dictionary<string, object>>()
                         where nonmatchingMovies.ContainsKey("alternate_ids")
                         && !imdbID.Contains((nonmatchingMovies["alternate_ids"] as Dictionary<string, object>)["imdb"].ToString())
                         select nonmatchingMovies; 
      var suggTitlesList = new List<string>();
      foreach (var suggMovie in suggestedMovies.Take(Convert.ToInt32(ConfigurationManager.AppSettings["SuggestedTitlesCount"])))
      {
          suggTitlesList.Add(suggMovie["title"].ToString());
      }

      rtServiceResponse.SuggestedTitles = suggTitlesList;*/
