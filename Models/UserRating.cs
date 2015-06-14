using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieOCD.Models
{
    [Table("UserRatings", Schema = "dbo")]
    public class UserRating
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("UserRatingID", TypeName = "int")]
        public int UserRatingID { get; set; }

    //    [ForeignKey("User")]
        [Column("UserID", TypeName = "varchar", Order = 1)]
        [Required]
        [Key]
        public string UserID { get; set; }

        [Column("IMDBID", TypeName = "varchar", Order = 2)]
        [Required]
        [Key]
        public string IMDBID { get; set; }

        [Column("Rating", TypeName = "varchar")]
        [Required]
        public string Rating { get; set; }

     //   public virtual User User { get; set; }
    }
}