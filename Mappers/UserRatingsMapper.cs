using System.Collections.Generic;
using System.Linq;
using MovieOCD.Messages.Responses;
using MovieOCD.Models;
using MovieOCD.Messages.Requests;

namespace MovieOCD.Mappers
{
    public class UserRatingsMapper
    {
        public UserRatingResponse MapFromModelToResponse(UserRating updatedModel)
        {
            var userRatingResponse = new UserRatingResponse
                {
                    IMDBID = updatedModel.IMDBID,
                    Rating = updatedModel.Rating,
                    UserID = updatedModel.UserID,
                    UserRatingID = updatedModel.UserRatingID.ToString()
                };
            //     userRatingResponse.Title = updatedModel.Title;

            return userRatingResponse;
        }

        public UserRating MapFromRequestToModel(UserRatingRequest userRatingRequest)
        {
            var userRating = new UserRating
                {
                    IMDBID = userRatingRequest.IMDBID,
                    Rating = userRatingRequest.Rating,
                    UserID = userRatingRequest.UserID
                };
            //  UserRating.Title = userRatingRequest.Title;

            return userRating;
        }

        public List<UserRatingResponse> MapFromModelListToResponse(List<UserRating> updatedModels)
        {
            return updatedModels.Select(MapFromModelToResponse).ToList();
        }
    }
}