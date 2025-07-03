using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;
using Microsoft.EntityFrameworkCore;
using TourPlannerClasses.DB;
using TourPlannerClasses.Models;
using FuzzySharp;
using System.Data;
using log4net;
using Microsoft.Win32;
using System.Text.Json;
using System.Windows;
using System.Windows.Markup;
using System.IO;

namespace TourPlanner.BusinessLogic.Services
{
    public interface ITourService
    {
        Task<List<Tours>> GetAllTours();
        Task<Tours> GetTourById(int id);
        Task InsertTours(Tours newTour);
        Task UpdateTour(Tours tourToUpdate);
        Task DeleteTour(Tours tourToDelete);
        Task<Tours> ImportTourFromFileAsync(string path);
        Task ExportTourToFileAsync(Tours tour, string path);
        Task<string> CreateRandomTour();
        Task<ObservableCollection<Tours>> SearchForTours(string tourToFind, ObservableCollection<Tours> allTours, ObservableCollection<Tourlog> allTourlogs);
    }

    public class TourService : ITourService
    {
        private readonly TourDbContext _context;
        private static readonly ILog _log = LogManager.GetLogger(typeof(TourService));
        private readonly TourLogService _tourlogService;
        private readonly ApiHandler _apiHandler;

        public TourService(TourDbContext context, TourLogService tourlogService, ApiHandler apiHandler)
        {
            _context = context;
            _tourlogService = tourlogService;
            _apiHandler = apiHandler;
        }


        public virtual async Task<List<Tours>> GetAllTours()
        {
            return await _context.Tours.ToListAsync();
        }

        public async Task<Tours> GetTourById(int id)
        {
            try
            {
                var foundTour = _context.Tours.Find(id);
                if (foundTour != null)
                {
                    _log.Info($"Tour successfully found: {foundTour.Name}");
                    return foundTour;
                }
            }
            catch (Exception ex)
            {
                _log.Warn($"Error finding Tour by Id: {ex.Message}");
            }
            return null;
        }

        public async Task<ObservableCollection<Tours>> SearchForTours(string name, ObservableCollection<Tours> allTours, ObservableCollection<Tourlog> allTourlogs)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return allTours;
                }

                ObservableCollection<Tours> matchingTours = new ObservableCollection<Tours>();
                const int fuzzyRatio = 40;

                foreach (var tour in allTours)
                {
                    if (Fuzz.Ratio(name, tour.Name) > fuzzyRatio)
                    {
                        matchingTours.Add(tour);
                    }
                }

                foreach (var tourlog in allTourlogs)
                {
                    if (Fuzz.Ratio(name, tourlog.Author) > fuzzyRatio || Fuzz.Ratio(name, tourlog.Comment) > fuzzyRatio)
                    {
                        if (!matchingTours.Contains(tourlog.Tour))
                            matchingTours.Add(tourlog.Tour);
                    }
                }
                return matchingTours;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error looking for tours via their name: {ex.Message}");
            }
            return new ObservableCollection<Tours>();
        }

        public async Task InsertTours(Tours newTour)
        {
            try
            {
                newTour.Id = 0;
                _context.Tours.Add(newTour);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inserting tour: {ex.Message}");
            }
        }
        public async Task UpdateTour(Tours tourToUpdate)
        {
            try
            {
                var existingTour = await _context.Tours.FindAsync(tourToUpdate.Id);

                if (existingTour == null)
                {
                    Console.WriteLine("Tour not found, update aborted.");
                    return;
                }

                // Copy new values into existingTour
                _context.Entry(existingTour).CurrentValues.SetValues(tourToUpdate);

                // Explicitly mark entity as modified
                _context.Entry(existingTour).State = EntityState.Modified;

                int changes = _context.SaveChanges();
                Console.WriteLine($"Rows affected: {changes}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating tour: {ex.Message}");
            }
        }

        public async Task DeleteTour(Tours tourToDelete)
        {
            var foundTour = await GetTourById(tourToDelete.Id);
            if (foundTour != null)
            {
                _context.Remove(tourToDelete);
                _context.SaveChanges();
            }
        }

        public async Task<Tours> ImportTourFromFileAsync(string path)
        {
            if (!File.Exists(path))
            {
                _log.Error($"Import file not found: {path}");
                return null;
            }

            var json = await File.ReadAllTextAsync(path);
            var dto = JsonSerializer.Deserialize<TourExportDto>(json);

            if (dto == null)
                return null;

            var newTour = new Tours
            {
                Name = dto.Name,
                Description = dto.Description,
                From = dto.From,
                FromLat = dto.FromLat,
                FromLng = dto.FromLng,
                To = dto.To,
                ToLat = dto.ToLat,
                ToLng = dto.ToLng,
                Duration = dto.Duration,
                Distance = dto.Distance,
                Transport = dto.Transport,
                Tourlogs = dto.Tourlogs.Select(tl => new Tourlog
                {
                    Date = tl.Date,
                    Comment = tl.Comment,
                    Difficulty = (Difficulty)tl.Difficulty,
                    TotalDistance = tl.TotalDistance,
                    TotalTime = tl.TotalTime,
                    Rating = tl.Rating,
                    Author = tl.Author
                }).ToList()
            };

            await InsertTours(newTour);

            foreach (var log in newTour.Tourlogs)
            {
                log.Tour = newTour;
                await _tourlogService.InsertTourLog(log);
            }

            return newTour;
        }

        public async Task ExportTourToFileAsync(Tours tour, string path)
        {
            if (tour == null)
            {
                _log.Warn("Attempted to export null tour.");
                return;
            }

            var dto = new TourExportDto
            {
                Name = tour.Name,
                Description = tour.Description,
                From = tour.From,
                FromLat = tour.FromLat,
                FromLng = tour.FromLng,
                To = tour.To,
                ToLat = tour.ToLat,
                ToLng = tour.ToLng,
                Duration = tour.Duration,
                Distance = tour.Distance,
                Transport = tour.Transport,
                Tourlogs = tour.Tourlogs.Select(tl => new TourlogExportDto
                {
                    Date = tl.Date,
                    Comment = tl.Comment,
                    Difficulty = (int)tl.Difficulty,
                    TotalDistance = tl.TotalDistance,
                    TotalTime = tl.TotalTime,
                    Rating = tl.Rating,
                    Author = tl.Author
                }).ToList()
            };

            var json = JsonSerializer.Serialize(dto, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            await File.WriteAllTextAsync(path, json);
            _log.Info($"Tour exported to {path}");
        }

        public async Task<string> CreateRandomTour()
        {
            var cities = new Dictionary<string, (double Lat, double Lng)>
{
    { "Vienna", (48.2082, 16.3738) },
    { "Graz", (47.0707, 15.4395) },
    { "Linz", (48.3069, 14.2858) },
    { "Salzburg", (47.8095, 13.0550) },
    { "Innsbruck", (47.2692, 11.4041) },
    { "Munich", (48.1351, 11.5820) },
    { "Prague", (50.0755, 14.4378) },
    { "Budapest", (47.4979, 19.0402) },
    { "Ljubljana", (46.0569, 14.5058) },
    { "Berlin", (52.5200, 13.4050) },
    { "Paris", (48.8566, 2.3522) },
    { "Rome", (41.9028, 12.4964) },
    { "Madrid", (40.4168, -3.7038) },
    { "Warsaw", (52.2297, 21.0122) },
    { "Amsterdam", (52.3676, 4.9041) },
    { "Brussels", (50.8503, 4.3517) },
    { "Copenhagen", (55.6761, 12.5683) },
    { "Oslo", (59.9139, 10.7522) },
    { "Stockholm", (59.3293, 18.0686) },
    { "Helsinki", (60.1699, 24.9384) },
    { "London", (51.5074, -0.1278) },
    { "Dublin", (53.3498, -6.2603) },
    { "Lisbon", (38.7223, -9.1393) },
    { "Zurich", (47.3769, 8.5417) },
    { "Geneva", (46.2044, 6.1432) },
    { "New York", (40.7128, -74.0060) },
    { "Los Angeles", (34.0522, -118.2437) },
    { "Chicago", (41.8781, -87.6298) },
    { "Toronto", (43.6532, -79.3832) },
    { "Vancouver", (49.2827, -123.1207) },
    { "Mexico City", (19.4326, -99.1332) },
    { "Rio de Janeiro", (-22.9068, -43.1729) },
    { "Buenos Aires", (-34.6037, -58.3816) },
    { "Santiago", (-33.4489, -70.6693) },
    { "Cape Town", (-33.9249, 18.4241) },
    { "Cairo", (30.0444, 31.2357) },
    { "Nairobi", (-1.286389, 36.817223) },
    { "Istanbul", (41.0082, 28.9784) },
    { "Dubai", (25.276987, 55.296249) },
    { "Tokyo", (35.6762, 139.6503) },
    { "Seoul", (37.5665, 126.9780) },
    { "Bangkok", (13.7563, 100.5018) },
    { "Singapore", (1.3521, 103.8198) },
    { "Sydney", (-33.8688, 151.2093) },
    { "Melbourne", (-37.8136, 144.9631) },
    { "Wellington", (-41.2865, 174.7762) },
    { "Auckland", (-36.8485, 174.7633) },
    { "Perth", (-31.9505, 115.8605) }
};

            var rand = new Random();

            (string Name, double Lat, double Lng) fromCoords;
            (string Name, double Lat, double Lng) toCoords;

            int timeout = 1000;

            while (true)
            {
                var cityNames = cities.Keys.ToList();

                var fromCityName = cityNames[rand.Next(cityNames.Count)];
                var toCityName = cityNames[rand.Next(cityNames.Count)];

                if (fromCityName == toCityName)
                {
                    timeout--;
                    continue;
                }

                fromCoords = (fromCityName, cities[fromCityName].Lat, cities[fromCityName].Lng);
                toCoords = (toCityName, cities[toCityName].Lat, cities[toCityName].Lng);

                double distanceKm = _apiHandler.Haversine(
                    fromCoords.Lat, fromCoords.Lng,
                    toCoords.Lat, toCoords.Lng);

                if (distanceKm <= 6000)
                {
                    break;
                }
                timeout--;
                if (timeout < 0)
                    MessageBox.Show("Timeout reached! Random Tour could not be generated.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            string profile = "driving-car";

            var routeInfo = await _apiHandler.GetRouteDirections(
                (float)fromCoords.Lng,
                (float)fromCoords.Lat,
                (float)toCoords.Lng,
                (float)toCoords.Lat,
                profile
            );

            var newTour = new Tours
            {
                Name = $"Random Tour {Guid.NewGuid().ToString().Substring(0, 8)}",
                Description = $"A randomly generated tour from {fromCoords.Name} to {toCoords.Name}.",
                From = fromCoords.Name,
                FromLat = fromCoords.Lat,
                FromLng = fromCoords.Lng,
                To = toCoords.Name,
                ToLat = toCoords.Lat,
                ToLng = toCoords.Lng,
                Duration = TimeSpan.FromSeconds(routeInfo.DurationSeconds),
                Distance = Math.Round(routeInfo.DistanceMeters / 1000.0, 2),
                Transport = TransportType.Car // or randomize if you want
            };

            await InsertTours(newTour);

            return "";
        }
    }

    public class TourExportDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string From { get; set; }
        public double FromLat { get; set; }
        public double FromLng { get; set; }
        public string To { get; set; }
        public double ToLat { get; set; }
        public double ToLng { get; set; }
        public TimeSpan Duration { get; set; }
        public double Distance { get; set; }
        public TransportType Transport { get; set; }
        public List<TourlogExportDto> Tourlogs { get; set; } = new();
    }

    public class TourlogExportDto
    {
        public DateTime Date { get; set; }
        public string Comment { get; set; }
        public int Difficulty { get; set; }
        public double TotalDistance { get; set; }
        public TimeSpan TotalTime { get; set; }
        public int Rating { get; set; }
        public string Author { get; set; }
    }
}
