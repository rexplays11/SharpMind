using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharpMind.Models
{
    [Table("poglavlje")]
    public class Poglavlje
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("poglavlje_id")]
        public int PoglavljeId { get; set; }

        [Column("poglavlje_naziv")]
        public string PoglavljeNaziv { get; set; }


        public ICollection<Potpoglavlje> Potpoglavlja { get; set; }
    }
}