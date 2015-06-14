using MovieOCD.Messages.Responses;
using MovieOCD.Messages.Requests;
namespace MovieOCD.ServiceManagers
{
    public interface IServiceManager
    {
        BaseServiceMovieResponse Search(BaseServiceMovieRequest baseServiceMovieRequest);

        void ClearResponse(BaseServiceMovieResponse resp);

        BaseServiceReviewResponse FindReviews(BaseServiceReviewRequest request);
    }
}
