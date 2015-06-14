using System.Collections.Generic;
namespace MovieOCD.DTO
{
    public class ClientReviewDTO
    {
        public string ID { get; set; }

        public List<ReviewDTO> Reviews {get; set;}
    }
}
