using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using TourPlannerClasses.Models;
using Newtonsoft.Json.Linq;
using System.IO;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.Extensions.Configuration;

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
                .HasOne(tl => tl.Tour)  // each tourlog belongs to one tour
                .WithMany(t => t.Tourlogs)  // each tour can have many tourlogs
                .HasForeignKey(tl => tl.TourId)  // foreign key
                .OnDelete(DeleteBehavior.Cascade);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //optionsBuilder.AddInterceptors([new LogInterceptor()]);
        }
    }

    public class TourDbContextFactory : IDesignTimeDbContextFactory<TourDbContext>
    {
        public TourDbContext CreateDbContext(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddUserSecrets<TourDbContext>()
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<TourDbContext>();
            var connectionString = config["ConnectionString"];
            optionsBuilder.UseNpgsql(connectionString);

            return new TourDbContext(optionsBuilder.Options);
        }
    }

    public class ConfigReader
    {
        private readonly IConfiguration _config;

        public ConfigReader()
        {
            _config = new ConfigurationBuilder()
                .AddUserSecrets<ConfigReader>()
                .Build();
        }

        public string GetConnectionString() => _config["ConnectionString"];

        public List<string> GetApiKeys() => new()
        {
            _config["OpenRouteApiKey"],
            _config["MapBoxApiKey"]
        };
    }
}