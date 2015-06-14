using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Validation;
using System.Linq;
using MovieOCD.BusinessLogic;
using MovieOCD.DataAccess.Context;
using MovieOCD.Models;

namespace MovieOCD.DataAccess.Repositories
{
    public class UserRatingRepository : IRatingRepository
    {
        public UserRating Add(UserRating userRating)
        {
            using (var movieOCDDB = new MovieOCDDB())
            {
              /*   var hasExistingIMDBRecord = from ur in MovieOCDDB.UserRating
                                               join u in MovieOCDDB.User on ur.UserID equals u.UserID
                                               where ur.IMDBID == UserRating.IMDBID && ur.UserID == UserRating.UserID
                                               select ur;
              */
                var hasExistingIMDBRecord = movieOCDDB.UserRating.FirstOrDefault(x => x.IMDBID == userRating.IMDBID && x.UserID == userRating.UserID);
              //  var hasExistingTitleUserRecord = MovieOCDDB.UserRating.Where(x => x.Title == UserRating.Title && x.UserID == UserRating.UserID);

                if (hasExistingIMDBRecord != null)
                {
                    hasExistingIMDBRecord.Rating = userRating.Rating;
                    movieOCDDB.SaveChanges();
                    return movieOCDDB.UserRating.First(x => x.UserRatingID == hasExistingIMDBRecord.UserRatingID);
                }
           /*     else if (hasExistingTitleUserRecord.Any())
                {
                    var existingEntry = hasExistingTitleUserRecord.First();
                    existingEntry.Rating = UserRating.Rating;
                    MovieOCDDB.SaveChanges();
                    return MovieOCDDB.UserRating.Where(x => x.UserRatingID == existingEntry.UserRatingID).First() as UserRating;
                }*/
                movieOCDDB.UserRating.Add(userRating);
                movieOCDDB.Entry(userRating).State = EntityState.Added;
                try
                {
                    movieOCDDB.SaveChanges();
                }
                catch (DbEntityValidationException dbEx)
                {
                    foreach (var validationErrors in dbEx.EntityValidationErrors)
                    {
                        foreach (var validationError in validationErrors.ValidationErrors)
                        {
                            Elmah.ErrorSignal.FromCurrentContext()
                                 .Raise(
                                     new Exception("Property: " + validationError.PropertyName + " Error: " +
                                                   validationError.ErrorMessage));
                        }
                    }
                }
            }

            return Retrieve(userRating.UserID,userRating.IMDBID);
        }

        public UserRating Retrieve(string userid, string imdbid)
        {
            using (var db = new MovieOCDDB())
            {
                var queryResults = (from rating in db.UserRating
                                   where (rating.IMDBID == imdbid && rating.UserID == userid)
                                   select rating).FirstOrDefault();

                if (queryResults != null)
                {
                    return queryResults;
                }
             }

             return new UserRating();
        }

        public List<UserRating> RetrieveAll(string userID)
        {
            using (var db = new MovieOCDDB())
            {
                var queryResults = (from rating in db.UserRating
                                   where (rating.UserID == userID)
                                   select rating).ToList<UserRating>();

                DebugManager.LogWarning("UserID: "+userID+" Query Results: "+queryResults.FirstOrDefault());

                if (queryResults.Count > 0)
                {
                    return queryResults;
                }
                

            }

            return new List<UserRating>();
        }
    }
}