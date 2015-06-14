using System.Web;
using System.Web.Caching;
using MovieOCD.Messages.Responses;
using System.Collections;
using System;
using System.Configuration;
namespace MovieOCD.BusinessLogic
{
    public class CacheManager
    {
        private static CacheManager _instance;
        private static Cache _cache;
        private static int _cacheExpiration;

        private CacheManager() 
        { 
            _cache = HttpRuntime.Cache;
            _cacheExpiration = Convert.ToInt32(ConfigurationManager.AppSettings["CacheExpiration"]);
        }

        public static CacheManager Instance
        {
            get { return _instance ?? (_instance = new CacheManager()); }
        }

        public BaseServiceMovieResponse CheckForEntryInCache(string title, string cKey)
        {
            if (_cache[cKey + title] != null)
            {
                return _cache[cKey + title] as BaseServiceMovieResponse;
            }

            return null;
        }

        public void InsertEntryInCache(BaseServiceMovieResponse serviceResponse, string title, string cachekey)
        {
            DebugManager.LogWarning("About to cache movie");
            if (serviceResponse.Rating != Constants.Constants.NotFound)
            {
                _cache.Insert(cachekey + title, serviceResponse, null, DateTime.Now.AddMinutes(_cacheExpiration), Cache.NoSlidingExpiration);
            }
        }

        public void Clear()
        {
            foreach(DictionaryEntry item in _cache)
            {
                _cache.Remove(item.Key.ToString());
            }
        }
    }


}