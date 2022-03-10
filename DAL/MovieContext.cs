using WebScraperPrimary.Models;
using Microsoft.EntityFrameworkCore;

namespace WebScraperPrimary.DAL
{
    public class MovieContext : DbContext
    {
        public DbSet<MovieMaster> MovieMaster { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsbuilder)
        {
            optionsbuilder.UseNpgsql("User ID=postgres;Password=TCEP;Host=localhost;Port=5432;Database=postgres;Pooling=true;");
        }
    }
}