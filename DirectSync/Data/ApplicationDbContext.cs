using DirectSync.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DirectSync.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserAsset>().HasKey(t => new { t.UserId, t.AssetId });
            modelBuilder.Entity<UserKey>().HasKey(t => new { t.UserId, t.ExchangeId, t.PublicKey });

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<DirectSync.Models.Asset> Assets { get; set; }
        public DbSet<DirectSync.Models.Exchange> Exchanges { get; set; }


        // Sensitive Data Logging for Debugging Purposes
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
            optionsBuilder
                //Log parameter values
                .EnableSensitiveDataLogging();

    }

}
