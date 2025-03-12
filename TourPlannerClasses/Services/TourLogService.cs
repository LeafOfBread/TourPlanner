using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TourPlannerClasses.DB;
using TourPlannerClasses.Models;

namespace TourPlannerClasses.Services
{
    public class TourLogService
    {
        private readonly TourDbContext _context;

        public TourLogService(TourDbContext context)
        {
            _context = context;
        }

        public async Task<List<Tourlog>> GetTourlogsAsync()
        {
            return await _context.TourLogs.ToListAsync();
        }
    }
}
