using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TourPlannerClasses.DB;


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
        
    }
}
