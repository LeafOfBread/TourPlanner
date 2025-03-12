using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.Windows.Controls;
using TourPlannerClasses.Models;

namespace TourPlannerClasses.DB
{
    public class TourDbContext : DbContext
    {
        public DbSet<Tours> Tours { get; set; }
        public DbSet<Tourlog> TourLogs { get; set; }

        public TourDbContext(DbContextOptions<TourDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Tourlog>()
                .HasOne(tl => tl.Tour)  // Each Tourlog belongs to one Tour
                .WithMany(t => t.Tourlogs)  // Each Tour can have many Tourlogs
                .HasForeignKey(tl => tl.TourId)  // Foreign key reference
                .OnDelete(DeleteBehavior.Cascade);
        }

    }

    public class TourDbContextFactory : IDesignTimeDbContextFactory<TourDbContext>
    {
        public TourDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<TourDbContext>();
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=TourDB;Username=postgres;Password=fhtw");

            return new TourDbContext(optionsBuilder.Options);
        }
    }
}