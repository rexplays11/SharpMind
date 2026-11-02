
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharpMind.Models
{
    [Table("lekcija")]
    public class Lekcija
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("lekcija_id")]
        public int LekcijaId { get; set; }

        [Column("lekcija_naslov")]
        public string LekcijaNaslov { get; set; }

        [Column("lekcija_tekst")]
        public string? LekcijaTekst { get; set; }

        [Column("poglavlje_id")]
        public int? PoglavljeId { get; set; }

        [Column("potpoglavlje_id")]
        public int? PotpoglavljeId { get; set; }

        [Column("korisnik_id")]
        public int KorisnikId { get; set; }

        [ForeignKey("PoglavljeId")]
        public virtual Poglavlje? Poglavlje { get; set; }

        [ForeignKey("PotpoglavljeId")]
        public virtual Potpoglavlje? Potpoglavlje { get; set; }

        [ForeignKey("NivoId")]
        public virtual Nivo? Nivo { get; set; } 

        [ForeignKey("KorisnikId")]
        public virtual Korisnik? Kreator { get; set; }

        [Column("nivo_id")]
        public int? NivoId { get; set; }


    }
}