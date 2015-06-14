using System.Collections.Generic;
using MovieOCD.DTO;

namespace MovieOCD.Messages.Responses
{
    public class MovieOCDResponse
    {
        public string ID { get; set; }

        public string Title { get; set; }

        public string Director { get; set; }

        public string Year { get; set; }

        public string ImagePath { get; set; }

        public string LargeImagePath { get; set; }

        public List<RatingDTO> RatingProviders { get; set; }

        public List<SuggestedMovieDTO> SuggestedTitles { get; set; }

        public List<ErrorMessageDTO> StatusMessages { get; set; }

        public List<EditorPickResponse> EditorPicks { get; set; }

        public string PlotSummary{ get; set; }

    }
}
