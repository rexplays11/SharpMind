
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharpMind.Models
{
    [Table("rezultati")]
    public class Rezultat
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("rezultat_id")]
        public int RezultatId { get; set; }

        [Column("korisnik_id")]
        public int KorisnikId { get; set; }

        [Column("nivo_id")]
        public int? NivoId { get; set; }

        [Column("tip_aktivnosti")]
        public string TipAktivnosti { get; set; } // "TEST" ili "KVIZ"

        [Column("osvojeni_poeni")]
        public int OsvojeniPoeni { get; set; }

        [Column("ukupno_poena")]
        public int UkupnoPoena { get; set; }

        [Column("datum_polaganja")]
        [DataType(DataType.Date)]
        public DateTime DatumPolaganja { get; set; }

        [ForeignKey("KorisnikId")]
        public virtual Korisnik? Korisnik { get; set; }

        [ForeignKey("NivoId")]
        public virtual Nivo? Nivo { get; set; }
    }
}