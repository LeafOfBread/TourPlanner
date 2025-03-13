using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.Windows.Controls;
using TourPlannerClasses.Models;
using TourPlannerClasses.Services;
using TourPlannerClasses.Tour;
using Newtonsoft.Json.Linq;
using System.IO;

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
            JsonReader reader = new JsonReader();
            var optionsBuilder = new DbContextOptionsBuilder<TourDbContext>();
            var connectionString = reader.GetConnectionString();
            optionsBuilder.UseNpgsql(connectionString);

            return new TourDbContext(optionsBuilder.Options);
        }
    }

    public class JsonReader
    {
        private static readonly string ConfigFilePath = GetConfigFilePath();
        public string GetConnectionString()
        {
            string json = File.ReadAllText(ConfigFilePath);
            var jsonObj = JObject.Parse(json);
            return jsonObj["ConnectionString"]?.ToString();
        }

        private static string GetConfigFilePath()
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string configFileName = "dbconfig.json";

            string[] possiblePaths =
            {
                Path.Combine(basePath, configFileName),
                Path.Combine(basePath, "..", "..", "..", "..", "TourPlannerClasses", "DB", configFileName)
            };

            foreach (string path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    return Path.GetFullPath(path);
                }
            }
            throw new FileNotFoundException("Could not find dbconfig.json in expected locations.", ConfigFilePath);
        }
        public JsonReader() { }
    }
}