using System.Collections.Generic;
using MovieOCD.DataAccess.Repositories;
using MovieOCD.Mappers;
using MovieOCD.Messages.Requests;
using MovieOCD.Messages.Responses;

namespace MovieOCD.BusinessLogic
{
    public class UserRatingManager
    {
        private readonly UserRatingsMapper _mapper;
        private readonly IRatingRepository _userratingrepository;

        public UserRatingManager()
        {
           _mapper = new UserRatingsMapper();
           _userratingrepository = new UserRatingRepository();
        }

        public UserRatingManager(IRatingRepository repoIOC)
        {
            _mapper = new UserRatingsMapper();
            _userratingrepository = repoIOC;
        }

        public UserRatingResponse AddUserRating(UserRatingRequest userRatingRequest)
        {
            var urModel = _mapper.MapFromRequestToModel(userRatingRequest);

            var updatedModel =  _userratingrepository.Add(urModel);

            return _mapper.MapFromModelToResponse(updatedModel);
        }

        public List<UserRatingResponse> GetUserRatings(string userID)
        {
            var updatedModel = _userratingrepository.RetrieveAll(userID);

            return _mapper.MapFromModelListToResponse(updatedModel);
        }

        public UserRatingResponse GetUserRating(string userID, string imdbId)
        {
            var retrievedModel = _userratingrepository.Retrieve(userID, imdbId);

            return _mapper.MapFromModelToResponse(retrievedModel);
        }
    }
}