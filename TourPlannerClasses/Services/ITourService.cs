using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TourPlannerClasses.DB;
using TourPlannerClasses.Models;


namespace TourPlanner.BusinessLogic.Services
{
    public interface ITourService
    {
        Task<List<Tours>> GetAllTours();
        Task<Tours> GetTourById(int id);
        Task InsertTours(Tours newTour);
        Task UpdateTour(Tours tourToUpdate);
        Task DeleteTour(Tours tourToDelete);
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
                var foundTour = await _context.Tours.FindAsync(id);
                if (foundTour != null)
                    return foundTour;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error finding Tour by Id: {ex.Message}");
            }
            return null;
        }

        public async Task InsertTours(Tours newTour)
        {
            try
            {
                newTour.Id = 0;
                await _context.Tours.AddAsync(newTour);
                await _context.SaveChangesAsync();
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

                int changes = await _context.SaveChangesAsync();
                Console.WriteLine($"Rows affected: {changes}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating tour: {ex.Message}");
            }
        }

        public async Task DeleteTour(Tours tourToDelete)
        {
            _context.Remove(tourToDelete);
            await _context.SaveChangesAsync();
        }
    }
}
