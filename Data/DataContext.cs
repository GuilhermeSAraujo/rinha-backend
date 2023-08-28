using Microsoft.EntityFrameworkCore;
using RinhaDeBackend.Models;

namespace RinhaDeBackend.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseNpgsql("Host=localhost;Username=postgres;Password=admin;Database=postgres");
        }

        public DbSet<Pessoa> Pessoa { get; set; }

    }
}
