using System;
using System.Collections.Generic;
using MovieOCD.DTO;
using MovieOCD.Messages.Responses;

namespace MovieOCD.BusinessLogic
{
    public static class MockMovieResponseFactory
    {
        private static readonly Dictionary<string, MovieOCDResponse> Mocks = BuildMocks();

        public static MovieOCDResponse TryGet(string movieName, string year)
        {
            if (string.IsNullOrWhiteSpace(movieName))
            {
                return null;
            }

            var key = Normalize(movieName, year);
            if (Mocks.ContainsKey(key))
            {
                return Mocks[key];
            }

            // Allow showcase searches without year (e.g., "pulp fiction")
            var titleOnlyPrefix = string.Format("{0}|", movieName.Trim().ToLowerInvariant());
            foreach (var pair in Mocks)
            {
                if (pair.Key.StartsWith(titleOnlyPrefix, StringComparison.Ordinal))
                {
                    return pair.Value;
                }
            }

            return null;
        }

        private static string Normalize(string movieName, string year)
        {
            return string.Format("{0}|{1}", movieName.Trim().ToLowerInvariant(), (year ?? string.Empty).Trim());
        }

        private static Dictionary<string, MovieOCDResponse> BuildMocks()
        {
            return new Dictionary<string, MovieOCDResponse>
            {
                {
                    Normalize("Pulp Fiction", "1994"),
                    new MovieOCDResponse
                    {
                        ID = "0110912",
                        Title = "Pulp Fiction",
                        Director = "Quentin Tarantino",
                        Year = "1994",
                        ImagePath = "/Images/pulp-fiction-poster.jpg",
                        LargeImagePath = "/Images/pulp-fiction-poster.jpg",
                        PlotSummary = "The lives of two mob hitmen, a boxer, and others intertwine in four tales of violence and redemption.",
                        RatingProviders = BuildRatings("0110912", "8.9", "4.6", "92"),
                        SuggestedTitles = new List<SuggestedMovieDTO>
                        {
                            new SuggestedMovieDTO { Title = "Jackie Brown", Year = "1997" },
                            new SuggestedMovieDTO { Title = "Reservoir Dogs", Year = "1992" }
                        },
                        StatusMessages = BuildStatusMessages()
                    }
                },
                {
                    Normalize("Inception", "2010"),
                    new MovieOCDResponse
                    {
                        ID = "1375666",
                        Title = "Inception",
                        Director = "Christopher Nolan",
                        Year = "2010",
                        ImagePath = "/Images/inception-poster.jpg",
                        LargeImagePath = "/Images/inception-poster.jpg",
                        PlotSummary = "A thief who steals corporate secrets through dream-sharing technology is given one impossible task: inception.",
                        RatingProviders = BuildRatings("1375666", "8.8", "4.4", "87"),
                        SuggestedTitles = new List<SuggestedMovieDTO>
                        {
                            new SuggestedMovieDTO { Title = "Interstellar", Year = "2014" },
                            new SuggestedMovieDTO { Title = "Memento", Year = "2000" }
                        },
                        StatusMessages = BuildStatusMessages()
                    }
                },
                {
                    Normalize("The Matrix", "1999"),
                    new MovieOCDResponse
                    {
                        ID = "0133093",
                        Title = "The Matrix",
                        Director = "Lana Wachowski, Lilly Wachowski",
                        Year = "1999",
                        ImagePath = "/Images/matrix-poster.jpg",
                        LargeImagePath = "/Images/matrix-poster.jpg",
                        PlotSummary = "A computer hacker learns about the true nature of reality and his role in the war against its controllers.",
                        RatingProviders = BuildRatings("0133093", "8.7", "4.5", "83"),
                        SuggestedTitles = new List<SuggestedMovieDTO>
                        {
                            new SuggestedMovieDTO { Title = "The Matrix Reloaded", Year = "2003" },
                            new SuggestedMovieDTO { Title = "Dark City", Year = "1998" }
                        },
                        StatusMessages = BuildStatusMessages()
                    }
                }
            };
        }

        private static List<RatingDTO> BuildRatings(string imdbId, string imdbRating, string netflixRating, string rtRating)
        {
            return new List<RatingDTO>
            {
                new RatingDTO
                {
                    ID = imdbId,
                    DisplayName = "IMDB",
                    Rating = imdbRating,
                    MaxRating = "10",
                    LogoUrl = "/Images/imdb.jpg",
                    Status = "1"
                },
                new RatingDTO
                {
                    ID = imdbId,
                    DisplayName = "Netflix",
                    Rating = netflixRating,
                    MaxRating = "5",
                    LogoUrl = "/Images/netflix.jpg",
                    Status = "1"
                },
                new RatingDTO
                {
                    ID = imdbId,
                    DisplayName = "Rotten Tomatoes",
                    Rating = rtRating,
                    MaxRating = "100",
                    LogoUrl = "/Images/tomatoes.jpg",
                    Status = "1"
                }
            };
        }

        private static List<ErrorMessageDTO> BuildStatusMessages()
        {
            return new List<ErrorMessageDTO>
            {
                new ErrorMessageDTO { Provider = "Netflix", Message = "OK (Mock Data)" },
                new ErrorMessageDTO { Provider = "IMDB", Message = "OK (Mock Data)" },
                new ErrorMessageDTO { Provider = "Rotten Tomatoes", Message = "OK (Mock Data)" }
            };
        }
    }
}
