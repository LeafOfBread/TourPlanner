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
        Task<ObservableCollection<Tours>> SearchForTours(string tourToFind, ObservableCollection<Tours> allTours, ObservableCollection<Tourlog> allTourlogs);
    }

    public class TourService : ITourService
    {
        private readonly TourDbContext _context;
        private static readonly ILog _log = LogManager.GetLogger(typeof(TourService));
        private readonly TourLogService _tourlogService;

        public TourService(TourDbContext context, TourLogService tourlogService)
        {
            _context = context;
            _tourlogService = tourlogService;
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

                foreach(var tour in allTours)
                {
                    if (Fuzz.Ratio(name, tour.Name) > fuzzyRatio)
                    {
                        matchingTours.Add(tour);
                    }
                }

                foreach(var tourlog in allTourlogs)
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
