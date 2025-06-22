using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using TourPlannerClasses.DB;
using TourPlanner.UI.HelperClasses;
using TourPlanner.UI.ViewModels;
using TourPlanner.BusinessLogic.Services;
using log4net;
using log4net.Config;
using System.IO;

namespace TourPlanner
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; }
        private static readonly ILog _log = LogManager.GetLogger(typeof(App));

        protected override void OnStartup(StartupEventArgs e)   
        {
            var services = new ServiceCollection();

            XmlConfigurator.Configure(new FileInfo("LoggerConfig.xml"));
            _log.Info("TourPlanner application starting...");
            _log.Info($"Current working directory: {Directory.GetCurrentDirectory()}");

            //database
            ConfigReader reader = new ConfigReader();
            var connectionString = reader.GetConnectionString();
            services.AddDbContext<TourDbContext>(options =>
            options.UseNpgsql(connectionString),
            ServiceLifetime.Scoped);

            //services
            services.AddScoped<TourService>();
            services.AddScoped<TourLogService>();
            services.AddScoped<InputValidator>();

            services.AddSingleton<MainViewModel>();

            services.AddSingleton<MainWindow>();

            ServiceProvider = services.BuildServiceProvider();

            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
    }

}
