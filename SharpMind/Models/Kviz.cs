using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharpMind.Models
{
    [Table("kvizovi")]
    public class Kviz
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("kviz_id")]
        public int KvizId { get; set; }

        [Column("kviz_naslov")]
        public string? KvizNaslov { get; set; }

        [Column("korisnik_id")]
        public int? KorisnikId { get; set; }

        [Column("tip_kviza")]
        public string? TipKviza { get; set; } // "zivoti" ili "vreme"

        [Column("vremensko_ogranicenje")]
        public int? VremenskoOgranicenje { get; set; } // npr. sekunde

        [Column("broj_zivota")]
        public int? BrojZivota { get; set; } // koristi se samo ako je tip = zivoti

        [Column("datum_kreiranja")]
        public DateTime DatumKreiranja { get; set; } = DateTime.Now;

        [ForeignKey("KorisnikId")]
        public virtual Korisnik? Kreator { get; set; }

        public virtual ICollection<KvizPitanje> KvizPitanja { get; set; } = new List<KvizPitanje>();
    }
}