using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using MovieOCD.BusinessLogic;
using MovieOCD.Messages.Responses;

namespace MovieOCD.Helpers
{
    public class ServiceManagerHelper
    {
        public static string GetLinkedIMDBID(string title, string year)
        {
            var imdbEntry = CacheManager.Instance.CheckForEntryInCache((string.IsNullOrEmpty(year)) ? title : title + year, Constants.Constants.IMDBCACHEKEY) as IMDBServiceMovieResponse;
            return (imdbEntry != null) ? imdbEntry.ID : Constants.Constants.NotFound;
        }

        public static string SetResponseProperty(Dictionary<string, object> movieDict, string key)
        {
            return (movieDict != null && movieDict.Count > 0 && movieDict.ContainsKey(key)) ? movieDict[key].ToString() : Constants.Constants.NotFound;
        }

        public static void AddIfKeyDoesntExist(Dictionary<string, object> dictionary, string key, dynamic value)
        {
            if (!dictionary.ContainsKey(key))
            {
                dictionary.Add(key, value);
            }
        }

        public static string BuildResponse(WebRequest request)
        {
            string jsonText;
            using (var response = (HttpWebResponse)request.GetResponse())
            {
                using (var sr = new StreamReader(response.GetResponseStream()))
                {
                    jsonText = sr.ReadToEnd();
                }
            }
            return jsonText;
        }

        public static string StripPunctuation(string movieTitle)
        {
           return Regex.Replace(movieTitle, @"[^\w\s]", "");
        }
    }
}