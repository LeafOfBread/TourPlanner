using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using TourPlannerClasses.Models;
using Newtonsoft.Json.Linq;
using System.IO;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

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
            ConfigReader reader = new ConfigReader();
            var optionsBuilder = new DbContextOptionsBuilder<TourDbContext>();
            var connectionString = reader.GetConnectionString();
            optionsBuilder.UseNpgsql(connectionString);
            return new TourDbContext(optionsBuilder.Options);
        }
    }

    public class ConfigReader
    {
        private static readonly string ConfigFilePath = GetConfigFilePath();
        public string GetConnectionString()
        {
            string json = File.ReadAllText(ConfigFilePath);
            var jsonObj = JObject.Parse(json);
            var test = GetApiKeys();
            return jsonObj["ConnectionString"]?.ToString();
        }

        private static string GetConfigFilePath()
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string configFileName = "dbconfig.json";

            string[] possiblePaths =
            {
                Path.Combine(basePath, configFileName),
                Path.Combine(basePath, "..", "..", "..", "..", "TourPlanner.DataAccess", "DB", configFileName)
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

        public virtual List<string> GetApiKeys()
        {
            //initialize list
            List<string> ApiKeys = new List<string>();

            string json = File.ReadAllText(ConfigFilePath);
            var jsonObject = JObject.Parse(json);

            //read both apikeys from config file
            string ApiKey1 = jsonObject["OpenRouteApiKey"]?.ToString();
            string ApiKey2 = jsonObject["MapBoxApiKey"]?.ToString();

            //add to list and return both keys
            ApiKeys.Add(ApiKey1);
            ApiKeys.Add(ApiKey2);

            return ApiKeys;
        }

        public ConfigReader() { }
    }
}