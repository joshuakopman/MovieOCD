using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Xml;
using System.Xml.Serialization;
using MovieOCD.BusinessLogic;
using MovieOCD.DTO;
using MovieOCD.Helpers;
using MovieOCD.Messages.Requests;
using MovieOCD.Messages.Responses;
using MovieOCD.Models;
using OAuth;
using CoverArt = MovieOCD.Models.catalog_titlesCatalog_titleBox_art;
using MovieTitle = MovieOCD.Models.catalog_titlesCatalog_titleTitle;
using NetflixModel = MovieOCD.Models.catalog_titles;

namespace MovieOCD.ServiceManagers
{
    public class NetflixServiceManager : IServiceManager
    {
        private readonly string _consumerKey;
        private readonly string _secretKey;
        private readonly OAuthBase _oauth;
        private readonly List<SuggestedMovieDTO> _suggestedTitles;
        private string _foundyear;
        private string _foundtitle;
        private string _foundImage;

        public NetflixServiceManager()
        {
            _consumerKey = ConfigurationManager.AppSettings["OAuthConsumerKey"];
            _secretKey = ConfigurationManager.AppSettings["OAuthConsumerSecret"];
            _oauth = new OAuthBase();
            _suggestedTitles = new List<SuggestedMovieDTO>();
            _foundyear = Constants.Constants.NotFound;
            _foundtitle = Constants.Constants.NotFound;
        }

        public void ClearResponse(BaseServiceMovieResponse resp)
        {
            var nxresp = resp as NetflixServiceMovieResponse;
            if (nxresp != null)
            {
                nxresp.Title = Constants.Constants.NotFound;
                nxresp.Rating = Constants.Constants.NotFound;
                nxresp.Year = Constants.Constants.NotFound;
                nxresp.SuggestedTitles = null;
                nxresp.StatusCode = Constants.Constants.Fail;
            }
        }

        private string ExtractRatingFromModel(NetflixModel netflixModel, string exactTitle, string requestYear)
        {
            string foundTitleRating = Constants.Constants.NotFound;
            int suggCount = 0;
            var suggestedNum = Convert.ToInt32(ConfigurationManager.AppSettings[Constants.Constants.SuggestedTitlesCount]);
            var allTitleObjects = (netflixModel != null) ? netflixModel.catalog_title : null;
            var foundMovie = false;
            if (allTitleObjects != null)
            {
                foreach (var titleObj in allTitleObjects)
                {
                    var movieTitle = titleObj.Items[Convert.ToInt32(ItemsChoiceType.title)] as MovieTitle;
                    if (movieTitle != null)
                    {
                        var movieYear = titleObj.Items.AsParallel().FirstOrDefault(x => x is int && (x.ToString().StartsWith("19") || x.ToString().StartsWith("20")) && x.ToString().Length == 4);
                        if (movieYear != null)
                        {
                            var movieYearString = (Convert.ToInt32(movieYear) != 0) ? movieYear.ToString() : string.Empty;

                            if (ServiceManagerHelper.StripPunctuation(movieTitle.regular.ToLower()) == ServiceManagerHelper.StripPunctuation(exactTitle.ToLower()) &&
                                ((!string.IsNullOrEmpty(requestYear) && requestYear == movieYearString) || string.IsNullOrEmpty(requestYear)) && foundMovie == false) //title and year match
                            {

                                var matchingRating = titleObj.Items.FirstOrDefault(x => x is decimal);
                                var matchingImage = titleObj.Items.FirstOrDefault(x => x.GetType() == typeof(CoverArt));

                                if (matchingRating != null)
                                {
                                    foundTitleRating = matchingRating.ToString();
                                }
                                if (matchingImage != null)
                                {
                                    var catalogTitlesCatalogTitleBoxArt = matchingImage as CoverArt;
                                    if (catalogTitlesCatalogTitleBoxArt != null)
                                        _foundImage = catalogTitlesCatalogTitleBoxArt.large;
                                }

                                _foundtitle = movieTitle.regular;
                                _foundyear = movieYearString;
                                foundMovie = true;
                            }
                            else if (suggCount < suggestedNum) //year doesnt match, added to suggested title
                            {
                                _suggestedTitles.Add(new SuggestedMovieDTO { Title = movieTitle.regular, Year = movieYearString });
                                suggCount++;
                            }
                        }
                    }
                }
            }

            return foundTitleRating;
        }

        public BaseServiceMovieResponse Search(BaseServiceMovieRequest baseServiceMovieRequest)
        {
            string title = baseServiceMovieRequest.Title;
            string year = baseServiceMovieRequest.Year;
            var nxServiceResponse = CacheManager.Instance.CheckForEntryInCache((string.IsNullOrEmpty(year)) ? title : title + year, Constants.Constants.NETFLIXCACHEKEY) as NetflixServiceMovieResponse;
            
            if (nxServiceResponse != null)
            {
                return nxServiceResponse;
            }
           
            nxServiceResponse = new NetflixServiceMovieResponse();

            var netflixModel = Deserialize(CreateAPIRequest(title, ConfigurationManager.AppSettings["NetflixMaxResults"]));
            
            nxServiceResponse.Rating = ExtractRatingFromModel(netflixModel, title, year);
            nxServiceResponse.Year = _foundyear;
            nxServiceResponse.SuggestedTitles = _suggestedTitles;
            nxServiceResponse.Message = (nxServiceResponse.Rating != Constants.Constants.NotFound) ? Constants.Constants.Success : Constants.Constants.MovieNotFound;
            nxServiceResponse.StatusCode = (nxServiceResponse.Rating != Constants.Constants.NotFound) ? Constants.Constants.Pass : Constants.Constants.Fail;
            nxServiceResponse.Title = _foundtitle;
            nxServiceResponse.ImagePath = _foundImage;

            CacheManager.Instance.InsertEntryInCache(nxServiceResponse, (string.IsNullOrEmpty(year)) ? title : title + year, Constants.Constants.NETFLIXCACHEKEY);

            return nxServiceResponse;
        }

        public BaseServiceReviewResponse FindReviews(BaseServiceReviewRequest req)
        {
            throw new NotImplementedException();
        }

        private NetflixModel Deserialize(WebRequest request)
        {
            var netflixModel = new NetflixModel();
            request.Method = "GET";

            try
            {
                string xmlText = ServiceManagerHelper.BuildResponse(request);

                if (!string.IsNullOrEmpty(xmlText))
                {
                    var doc = new XmlDocument();
                    doc.LoadXml(xmlText);
                    netflixModel = new XmlSerializer(typeof(NetflixModel)).Deserialize(new XmlNodeReader(doc.DocumentElement)) as NetflixModel;
                }
            }
            catch(Exception ex)
            {
                DebugManager.LogException(ex);
            }

            return netflixModel;
        }

        private WebRequest CreateAPIRequest(string movietitle, string maxresults)
        {
            string normalizedUrl;
            string normalizedRequestParameters;
            var requestUrl = new Uri(ConfigurationManager.AppSettings["NetflixAPIUrl"]);
            _oauth.AddQueryParameter("term", movietitle);
            _oauth.AddQueryParameter("max_results", maxresults);

            string sig = _oauth.GenerateSignature
            (
                requestUrl,
                _consumerKey,
                _secretKey,
                null,
                null,
                "GET",
                _oauth.GenerateTimeStamp(),
                _oauth.GenerateNonce(),
                out normalizedUrl,
                out normalizedRequestParameters
            );

            return WebRequest.Create(requestUrl + "?" + normalizedRequestParameters + "&oauth_signature=" + _oauth.UrlEncode(sig));
        }

      /*  private OAuthTokenDTO GetTokenAndTokenSecret()
        {
            OAuthBase oauth = new OAuthBase();
            string normalUrl = string.Empty;
            string normalizedRequestParameters = string.Empty;
            string fullUrl = string.Empty;
            string timestamp = oauth.GenerateTimeStamp();
            string nonce = oauth.GenerateNonce();
            string consumerKey = ConfigurationManager.AppSettings["OAuthConsumerKey"];
            string secretKey = ConfigurationManager.AppSettings["OAuthConsumerSecret"];
            string tokenURL = "http://api-public.netflix.com/oauth/request_token?oauth_version=1.0&oauth_consumer_key=" +
                consumerKey +
                "&oauth_signature_method=HMAC-SHA1&oauth_timestamp=" +
                timestamp + "&oauth_nonce=" + nonce;

            //get token
            string sig = oauth.GenerateSignature
             (
                 new Uri("http://api-public.netflix.com/oauth/request_token?"),
                 consumerKey,
                 secretKey,
                 string.Empty,
                 string.Empty,
                 "GET",
                 timestamp,
                 nonce,
                 out normalUrl,
                 out normalizedRequestParameters
             );

            fullUrl = tokenURL + "&oauth_signature=" + sig;
            var request = WebRequest.Create(fullUrl);
            request.Method = "GET";

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            string tokenResp;
            using (var sr = new StreamReader(response.GetResponseStream()))
            {
                tokenResp = sr.ReadToEnd();
            }

            NameValueCollection query = HttpUtility.ParseQueryString(tokenResp);

            return new OAuthTokenDTO() { Token = query["oauth_token"], TokenSecret = query["oauth_token_secret"] };
        } */
    }

}
