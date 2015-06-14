using System;
using System.Collections.Generic;
using System.Web.Http;
using MovieOCD.BusinessLogic;
using MovieOCD.Messages.Requests;
using MovieOCD.Messages.Responses;

namespace MovieOCD.Controllers
{
    public class UserController : ApiController
    {
        private readonly UserRatingManager _userMgr;

        public UserController()
        {
            _userMgr = new UserRatingManager();
        }

        public UserRatingResponse Post(UserRatingRequest userRatingRequest)
        {
            try
            {
                return _userMgr.AddUserRating(userRatingRequest);
            }
            catch (Exception ex)
            {
                DebugManager.LogException(ex);
                return new UserRatingResponse {Status = Constants.Constants.Fail, Message = "Error Adding Rating"};
            }
        }

        public List<UserRatingResponse> GetUserRatingsByUserId(string uid)
        {
            try
            {
                DebugManager.LogWarning("Hit User Ratings Controller with userid: "+uid);
                return _userMgr.GetUserRatings(uid);
            }
            catch (Exception ex)
            {
                DebugManager.LogException(ex);
                return new List<UserRatingResponse>{new UserRatingResponse{ Message = "Invalid User ID",Status = Constants.Constants.Fail}};
            }
        }

        public UserRatingResponse GetUserRatingByImdbId(string userid, string imdbid)
        {
            try
            {
                return _userMgr.GetUserRating(userid, imdbid);
            }
            catch (Exception ex)
            {
                DebugManager.LogException(ex);
                return new UserRatingResponse { Message = "Invalid User Or Movie ID", Status = Constants.Constants.Fail};
            }
        }
    }
}
