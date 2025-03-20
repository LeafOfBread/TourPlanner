using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Data;
using System.Windows;
using TourPlanner.ViewModels;
using TourPlannerClasses.DB;
using TourPlannerClasses.Models;
using TourPlannerClasses.Services;
using TourPlannerClasses.Tour;
using Newtonsoft.Json.Linq;
using System.IO;
using TourPlanner.HelperClasses;

namespace TourPlanner
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            var services = new ServiceCollection();

            //database
            JsonReader reader = new JsonReader();
            var connectionString = reader.GetConnectionString();
            services.AddDbContext<TourDbContext>(options =>
            options.UseNpgsql(connectionString),
            ServiceLifetime.Scoped);

            //services
            services.AddScoped<TourService>();
            services.AddScoped<TourLogService>();
            services.AddScoped<InputValidator>();

            services.AddSingleton<TourViewModel>();

            services.AddSingleton<MainWindow>();

            ServiceProvider = services.BuildServiceProvider();

            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
    }

}
