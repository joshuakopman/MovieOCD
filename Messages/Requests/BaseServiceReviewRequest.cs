namespace MovieOCD.Messages.Requests
{
    public class BaseServiceReviewRequest : BaseServiceRequest
    {
        public string Title { get; set; }

        public string Year { get; set; }
    }
}
