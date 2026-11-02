using Microsoft.EntityFrameworkCore;
using SharpMind.Models;


namespace SharpMind.Data
{
    public class SharpMindContext : DbContext
    {
        public SharpMindContext(DbContextOptions<SharpMindContext> options)
            : base(options) { }

        public DbSet<Korisnik> Korisnici { get; set; }
        public DbSet<Uloga> Uloge { get; set; }
        public DbSet<Nivo> Nivoi { get; set; }
        public DbSet<Poglavlje> Poglavlja { get; set; }
        public DbSet<Potpoglavlje> Potpoglavlja { get; set; }
        public DbSet<Lekcija> Lekcije { get; set; }
        public DbSet<Rezultat> Rezultati { get; set; }
        public DbSet<Pitanje> Pitanja { get; set; }
        public DbSet<Odgovor> Odgovori { get; set; }
        public DbSet<Kviz> Kvizovi { get; set; }
        public DbSet<KvizPitanje> KvizPitanja { get; set; }
    }
}