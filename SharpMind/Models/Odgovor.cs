using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharpMind.Models
{
    [Table("odgovori")]
    public class Odgovor
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("odgovor_id")]
        public int OdgovorId { get; set; }

        [Column("pitanje_id")]
        public int PitanjeId { get; set; }

        [ForeignKey("PitanjeId")]
        public virtual Pitanje? Pitanje { get; set; }

        [Column("odgovor_tekst")]
        [Required]
        public string OdgovorTekst { get; set; }

        [Column("is_tacno")]
        public bool OdgovorTacnost { get; set; } // tinyint(1) mapiramo na bool
    }
}