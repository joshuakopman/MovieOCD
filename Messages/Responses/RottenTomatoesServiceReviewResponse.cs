using System.Collections.Generic;
using MovieOCD.DTO;
namespace MovieOCD.Messages.Responses
{
    public class RottenTomatoesServiceReviewResponse : BaseServiceReviewResponse
    {
        public string ID { get; set; }

        public List<ReviewDTO> Reviews { get; set; }
    }
}
