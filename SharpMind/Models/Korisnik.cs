
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharpMind.Models
{
    [Table("korisnici")]
    public class Korisnik
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("korisnik_id")]
        public int KorisnikId { get; set; }

        [Column("korisnicko_ime")]
        public string KorisnikIme { get; set; }

      
        [Column("lozinka")]
        public string KorisnikLozinka { get; set; }
       

        [Column("datum_registracije")]
        public DateTime KorisnikDatumRegistracije { get; set; }
        [Required]
        [Column("uloga_id")]
        public int UlogaId { get; set; }
        [Required]
        [Column("nivoId")]
        public int NivoId { get; set; }

        [Column("profesor_id")]
        public int? ProfesorId { get; set; }

        [Column("ukupno_poena")]
        public int UkupnoPoena { get; set; } = 0;

        [ForeignKey("ProfesorId")]
        public virtual Korisnik? Profesor { get; set; }

        [ForeignKey("NivoId")]
        public virtual Nivo? Nivo { get; set; }
    
    [ForeignKey("UlogaId")]
        public virtual Uloga? Uloga { get; set; }
    }
    }