namespace MovieOCD.Messages.Responses
{
    public class UserRatingResponse : BaseResponse 
    {
        public string UserRatingID { get; set; }

        public string UserID { get; set; }

        public string IMDBID { get; set; }

    //    public string Title { get; set; }

        public string Rating { get; set; }
    }
}