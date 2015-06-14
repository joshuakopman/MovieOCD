namespace MovieOCD.Messages.Requests
{
    public class UserRatingRequest 
    {
        public string UserID { get; set; }

        public string IMDBID { get; set; }

      //  public string Title { get; set; }

        public string Rating { get; set; }
    }
}
