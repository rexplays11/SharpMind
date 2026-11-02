
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharpMind.Models
{
    [Table("pitanja")]
    public class Pitanje
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("pitanje_id")]
        public int PitanjeId { get; set; }

        [Column("pitanje_tekst")]
        public string? PitanjeTekst { get; set; }

        [Column("pitanje_tip")]
        public string? PitanjeTip { get; set; } // ENUM u bazi se tretira kao string

        [Column("nivo_id")]
        public int NivoId { get; set; }

        [Column("blok_koda")]
        public string? BlokKod { get; set; }

        [Column("korisnik_id")]
        public int KorisnikId { get; set; }


        [ForeignKey("KorisnikId")]
        public virtual Korisnik? Kreator { get; set; }

        [ForeignKey("NivoId")]
        public virtual Nivo? Nivo { get; set; }

        public virtual ICollection<Odgovor> Odgovori { get; set; } = new List<Odgovor>();
    }
}
