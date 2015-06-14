using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieOCD.Models
{
    [Table("EditorPicks", Schema = "dbo")]
    public class EditorPick
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("EditorPickID", TypeName = "int", Order = 1)]
        public int EditorPickID { get; set; }
       
        [Key]
        [Column("IMDBID", TypeName = "varchar", Order = 2)]
        public string IMDBID { get; set; }
      
        [Key]
        [Column("EditorName", TypeName = "varchar", Order = 3)]
        public string EditorName { get; set; }

        public string Year { get; set; }

        public string Title { get; set; }

    }
}
