using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TourPlannerClasses.DB;
using TourPlannerClasses.Models;

namespace TourPlanner.BusinessLogic.Services
{
    public interface ITourLogService
    {
        Task<List<Tourlog>> GetTourlogsAsync();
        Task InsertTourLog(Tourlog newLog);
        Task DeleteTourLog(Tourlog tourlogToRemove);
        Task<Tourlog> GetTourlogById(int id);
        Task EditTourLog(Tourlog tourlogToEdit);
    }


    public class TourLogService : ITourLogService
    {
        private readonly TourDbContext _context;

        public TourLogService(TourDbContext context)
        {
            _context = context;
        }

        public async Task<List<Tourlog>> GetTourlogsAsync()
        {
            return _context.TourLogs.ToList();
        }

        public virtual async Task InsertTourLog(Tourlog newLog)
        {
            try
            {
                newLog.TourLogId = 0;
                newLog.Date = DateTime.SpecifyKind(newLog.Date, DateTimeKind.Utc);
                _context.TourLogs.Add(newLog);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while inserting new Tourlog: {ex.Message}");
            }
        }

        public async Task DeleteTourLog(Tourlog tourlogToRemove)
        {
            try
            {
                _context.TourLogs.Remove(tourlogToRemove);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occured while trying to remove tourlog: {ex.Message}");
            }
        }

        public async Task<Tourlog> GetTourlogById(int id)
        {
            try
            {
                var foundTourlog = _context.TourLogs.Find(id);

                if (foundTourlog != null)
                return foundTourlog;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while trying to find Tourlog via Id: {ex.Message}");
            }
            return null;
        }

        public async Task EditTourLog(Tourlog tourlogToEdit)
        {
            try
            {
                var existingTourlog = _context.TourLogs.Find(tourlogToEdit.TourLogId);
                    
                if (existingTourlog != null)
                {
                    _context.Entry(existingTourlog).CurrentValues.SetValues(tourlogToEdit);
                    _context.Entry(existingTourlog).State = EntityState.Modified;

                    int changes = _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occured while updating tourlog: {ex.Message}");
            }
        }
    }
}
