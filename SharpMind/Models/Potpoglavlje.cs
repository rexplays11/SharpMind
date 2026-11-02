using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharpMind.Models
{
    [Table("potpoglavlje")]
    public class Potpoglavlje
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("potpoglavlje_id")]
        public int PotpoglavljeId { get; set; }

        [Column("naziv_potpoglavlja")]
        public string NazivPotpoglavlja { get; set; }

        [Column("poglavlje_id")]
        public int PoglavljeId { get; set; }

        public Poglavlje Poglavlje { get; set; }

        public virtual ICollection<Lekcija> Lekcije { get; set; } = new List<Lekcija>();
    }
}