using System.Collections.Generic;
using MovieOCD.DTO;

namespace MovieOCD.Messages.Responses
{
    public class BaseServiceMovieResponse
    {
        public string Title { get; set; }

        public string Rating { get; set; }

        public string Year { get; set; }

        public List<SuggestedMovieDTO> SuggestedTitles { get; set; }

        public string Message { get; set; }

        public string StatusCode { get; set; }

        public string ImagePath { get; set; }

        public string LargeImagePath { get; set; }

        public string Director { get; set; }
    }
}
