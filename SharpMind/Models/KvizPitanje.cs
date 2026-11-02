using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharpMind.Models
{
    [Table("kviz_pitanja")]
    public class KvizPitanje
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("kviz_pitanje_id")]
        public int KvizPitanjeId { get; set; }

        [Column("kviz_id")]
        public int KvizId { get; set; }

        [Column("pitanje_id")]
        public int PitanjeId { get; set; }

        [ForeignKey("KvizId")]
        public virtual Kviz? Kviz { get; set; }

        [ForeignKey("PitanjeId")]
        public virtual Pitanje? Pitanje { get; set; }
    }
}