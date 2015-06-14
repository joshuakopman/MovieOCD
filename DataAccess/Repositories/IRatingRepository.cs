using System.Collections.Generic;
using MovieOCD.Models;

namespace MovieOCD.DataAccess.Repositories
{
    public interface IRatingRepository
    {
        UserRating Add(UserRating userRating);

        UserRating Retrieve(string userid, string imdbid);

        List<UserRating> RetrieveAll(string userID);
    }
}
