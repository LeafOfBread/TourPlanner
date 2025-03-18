using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TourPlannerClasses.DB;
using TourPlannerClasses.Models;


namespace TourPlannerClasses.Tour
{
    public class TourService
    {
        private readonly TourDbContext _context;

        public TourService(TourDbContext context)
        {
            _context = context;
        }

        public async Task<List<Tours>> GetAllTours()
        {
            return await _context.Tours.ToListAsync();
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

        public async Task DeleteTour(Tours tourToDelete)
        {
            _context.Remove(tourToDelete);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateTour(Tours tourToUpdate)
        {
            try
            {
                _context.Update(tourToUpdate);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inserting tour: {ex.Message}");
            }
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
    }
}
