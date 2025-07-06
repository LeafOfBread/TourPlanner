using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
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
            try
            {
                return await _context.TourLogs.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new NotFoundException("Failed to retrieve tour logs. " + ex.Message);
            }
        }

        public virtual async Task InsertTourLog(Tourlog newLog)
        {
            try
            {
                newLog.TourLogId = 0;
                newLog.Date = DateTime.SpecifyKind(newLog.Date, DateTimeKind.Utc);
                _context.TourLogs.Add(newLog);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new InsertionException("Error while inserting new Tourlog: " + ex.Message);
            }
        }

        public async Task DeleteTourLog(Tourlog tourlogToRemove)
        {
            try
            {
                _context.TourLogs.Remove(tourlogToRemove);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new InsertionException("Error occurred while trying to remove Tourlog: " + ex.Message);
            }
        }

        public async Task<Tourlog> GetTourlogById(int id)
        {
            try
            {
                var foundTourlog = await _context.TourLogs.FindAsync(id);

                if (foundTourlog == null)
                {
                    throw new SingleNotFoundException(nameof(Tourlog), $"Tourlog with ID {id} was not found.");
                }

                return foundTourlog;
            }
            catch (SingleNotFoundException)
            {
                throw; // rethrow to caller
            }
            catch (Exception ex)
            {
                throw new NotFoundException($"Error while trying to find Tourlog via Id {id}: {ex.Message}");
            }
        }

        public async Task EditTourLog(Tourlog tourlogToEdit)
        {
            try
            {
                var existingTourlog = await _context.TourLogs.FindAsync(tourlogToEdit.TourLogId);

                if (existingTourlog == null)
                {
                    throw new SingleNotFoundException(nameof(Tourlog), $"Tourlog with ID {tourlogToEdit.TourLogId} was not found.");
                }

                _context.Entry(existingTourlog).CurrentValues.SetValues(tourlogToEdit);
                _context.Entry(existingTourlog).State = EntityState.Modified;

                int changes = await _context.SaveChangesAsync();
            }
            catch (SingleNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InsertionException($"Error occurred while updating Tourlog: {ex.Message}");
            }
        }
    }
}
