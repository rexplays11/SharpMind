using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharpMind.Models

{
    [Table("nivoi")]
    public class Nivo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("nivoId")]
        public int NivoId { get; set; }

        [Column("NazivNivoa")]
        public string NazivNivoa { get; set; }

        [Column("granicni_poeni")]
        public int GranicniPoeniNivoa { get; set; }
    }

}