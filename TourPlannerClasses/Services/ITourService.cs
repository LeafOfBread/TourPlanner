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

namespace TourPlanner.BusinessLogic.Services
{
    public interface ITourService
    {
        Task<List<Tours>> GetAllTours();
        Task<Tours> GetTourById(int id);
        Task InsertTours(Tours newTour);
        Task UpdateTour(Tours tourToUpdate);
        Task DeleteTour(Tours tourToDelete);
        Task<ObservableCollection<Tours>> SearchForTours(string tourToFind, ObservableCollection<Tours> allTours, ObservableCollection<Tourlog> allTourlogs);
    }

    public class TourService : ITourService
    {
        private readonly TourDbContext _context;

        public TourService(TourDbContext context)
        {
            _context = context;
        }

        public TourService() { }

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
                    return foundTour;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error finding Tour by Id: {ex.Message}");
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
                const int fuzzyRatio = 50;

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
    }
}
