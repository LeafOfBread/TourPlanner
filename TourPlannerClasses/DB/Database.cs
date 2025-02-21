using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.Windows.Controls;
using TourPlannerClasses.Tour;

namespace TourPlannerClasses.DB
{
    public class TourDbContextFactory : IDesignTimeDbContextFactory<TourDbContext>
    {
        public TourDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<TourDbContext>();
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=TourDB;Username=postgres;Password=fhtw");

            return new TourDbContext(optionsBuilder.Options);
        }
    }

    public class TourDbContext : DbContext
    {
        public DbSet<Tours> Tours { get; set; }

        public TourDbContext(DbContextOptions<TourDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Ignore any non-entity classes like GroupStyleSelector
            modelBuilder.Ignore<GroupStyleSelector>();
        }

    }
}