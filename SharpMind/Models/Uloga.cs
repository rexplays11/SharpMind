using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharpMind.Models
{
    [Table("uloge")]
    public class Uloga
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("uloga_id")]
        public int UlogaId { get; set; }

        [Column("uloga_naziv")]
        public string UlogaNaziv { get; set; }
    }
}