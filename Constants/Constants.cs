
namespace MovieOCD.Constants
{
    public class Constants
    {
        //Errors 
        public const string NotFound = "N/A";
        public const string IMDBError = "Error";

        //Count
        public const string SuggestedTitlesCount = "SuggestedTitlesCount";

        //Provider Display Names
        public const string IMDBDisplayName = "IMDB";
        public const string RTDisplayName = "Rotten Tomatoes";
        public const string NetflixDisplayName = "Netflix";

        //Provider Cache Keys
        public const string RTCACHEKEY = "RT|";
        public const string NETFLIXCACHEKEY = "Netflix|";
        public const string IMDBCACHEKEY = "IMDB|";

        /* IMDB Serialization Keys */
            //imdbapi
            public const string IMDBAPIProvider = "IMDBAPI";
            public const string imdbtitleKey = "title";
            public const string imdbratingKey = "rating";
            public const string imdbidKey = "imdb_id";
            public const string imdbyearKey = "year";
            public const string imdbimageKey = "poster";
            public const string imdbdirectorkey = "directors";
            public const string imdbplotsummarykey = "plot_simple";
            //omdbapi
            public const string OMDBAPIProvider = "OMDBAPI";
            public const string omdbtitleKey = "Title";
            public const string omdbratingKey = "imdbRating";
            public const string omdbidKey = "imdbID";
            public const string omdbyearKey = "Year";
            public const string omdbimageKey = "Poster";
            public const string omdbdirectorkey = "Director";
            public const string omdbplotsummarykey = "Plot";

        //Max Ratings
        public const string NetflixMaxRating = "5";
        public const string IMDBMaxRating = "10";
        public const string RTMaxRating = "100";

        //Status Codes
        public const string Success = "Success";
        public const string MovieNotFound = "Movie Not Found";

        public const string Pass = "1";
        public const string Fail = "0";
    }
}