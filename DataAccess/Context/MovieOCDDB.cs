using System.Data.Entity;
using MovieOCD.Models;

namespace MovieOCD.DataAccess.Context
{
    public class MovieOCDDB : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MovieOCDDB" /> class.
        /// </summary>
        public MovieOCDDB() : base("rockshas_UserRatingsDb") {}

        public DbSet<UserRating> UserRating { get; set; }

        public DbSet<User> User { get; set; }

        public DbSet<EditorPick> EditorPick { get; set; }
    }
}