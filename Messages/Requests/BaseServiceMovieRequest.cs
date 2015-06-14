namespace MovieOCD.Messages.Requests
{
    public class BaseServiceMovieRequest : BaseServiceRequest
    {
        public string Title { get; set; }

        public string Year { get; set; }
    }
}
