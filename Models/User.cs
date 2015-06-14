using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace MovieOCD.Models
{
    [Table("Users", Schema = "dbo")]
    public class User
    {
       [Key]
       [Column("UserID", TypeName = "varchar")]
       public string UserID { get; set; }

       [Column("UserName", TypeName = "varchar")]
       public string UserName { get; set; }

       [Column("FirstName", TypeName = "nvarchar")]
       public string FirstName { get; set; }

       [Column("LastName", TypeName = "nvarchar")]
       public string LastName { get; set; }

       [Column("DateOfBirth", TypeName = "nvarchar")]
       public string DateOfBirth { get; set; }
    }
}
