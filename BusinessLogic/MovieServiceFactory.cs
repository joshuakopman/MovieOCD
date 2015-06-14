using MovieOCD.ServiceManagers;
using MovieOCD.Messages.Requests;

namespace MovieOCD.BusinessLogic
{
    public class MovieServiceFactory
    {
        public static IServiceManager GetServiceManagerByRequest(BaseServiceRequest request)
        {
            if (request is RottenTomatoesServiceMovieRequest || request is RottenTomatoesServiceReviewRequest)
            {
                return new RottenTomatoesServiceManager();
            }
            if (request is IMDBServiceMovieRequest)
            {
                return new IMDBServiceManager();
            }
            if (request is NetflixServiceMovieRequest)
            {
                return new NetflixServiceManager();
            }
            return new IMDBServiceManager();
        }
    }
}
