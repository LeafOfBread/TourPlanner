using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using TourPlannerClasses.DB;
using TourPlannerClasses.Models;
using TourPlannerClasses.Services;
using TourPlannerClasses.Tour;
using Xunit;

namespace ServiceTests
{
    public class TourServiceTests
    {
        private readonly TourDbContext _context;
        private readonly TourService _tourService;
        private readonly TourLogService _tourlogService;

        public TourServiceTests()
        {
            // create in-memory database
            var options = new DbContextOptionsBuilder<TourDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _context = new TourDbContext(options);
            _tourService = new TourService(_context);
            _tourlogService = new TourLogService(_context);

            // make sure database is clean before running tests
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();
        }

        [Fact]
        public async Task GetAllTours_ShouldReturnAllTours()
        {
            // Arrange
            var mockTours = new List<Tours>
        {
            new Tours(1, "Tour 1", "Description 1", "Start 1", "End 1", new System.TimeSpan(1, 0, 0), 10.5, TransportType.Boat),
            new Tours(2, "Tour 2", "Description 2", "Start 2", "End 2", new System.TimeSpan(2, 0, 0), 20.0, TransportType.Bus)
        };

            // Add mock tours to the in-memory database
            await _context.Tours.AddRangeAsync(mockTours);
            await _context.SaveChangesAsync();

            // Act
            var result = await _tourService.GetAllTours();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("Tour 1", result[0].Name);
            Assert.Equal("Tour 2", result[1].Name);
        }

        [Fact]
        public async Task GetTourById_ShouldReturnTour()
        {
            //Arrange
            var mockTours = new List<Tours>
        {
            new Tours(1, "Tour 1", "Description 1", "Start 1", "End 1", new System.TimeSpan(1, 0, 0), 10.5, TransportType.Boat),
            new Tours(2, "Tour 2", "Description 2", "Start 2", "End 2", new System.TimeSpan(2, 0, 0), 20.0, TransportType.Bus)
        };

            await _context.Tours.AddRangeAsync(mockTours);
            await _context.SaveChangesAsync();

            //Act
            int tourId = 2;
            var result = await _tourService.GetTourById(tourId);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(result, mockTours[1]);
        }

        [Fact]
        public async Task InsertTour_ShouldCorrectlyInsert()
        {
            //Arrange
            var tourToInsert = new Tours(1, "Tour 1", "Description 1", "Start 1", "End 1", new System.TimeSpan(1, 0, 0), 10.5, TransportType.Boat);
            //Act
            await _tourService.InsertTours(tourToInsert);

            //Assert
            var result = await _context.Tours.FindAsync(tourToInsert.Id);
            Assert.NotNull(result);
            Assert.Equal(result, tourToInsert);
        }

        [Fact]
        public async Task DeleteTour_ShouldCorrectlyDelete()
        {
            //Arrange
            var mockTour = new Tours(1, "Tour 1", "Description 1", "Start 1", "End 1", new System.TimeSpan(1, 0, 0), 10.5, TransportType.Boat);
            await _context.Tours.AddAsync(mockTour);
            await _context.SaveChangesAsync();

            //Act
            var tourToRemove = await _context.Tours.FindAsync(mockTour.Id);
            await _tourService.DeleteTour(tourToRemove);

            //Assert
            var result = await _context.Tours.FindAsync(mockTour.Id);
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateTour_ShouldCorrectlyUpdate()
        {
            //Arrange
            var mockTour = new Tours(1, "Tour 1", "Description 1", "Start 1", "End 1", new System.TimeSpan(1, 0, 0), 10.5, TransportType.Boat);
            await _context.AddAsync(mockTour);
            await _context.SaveChangesAsync();

            //Act
            var mockTourWithEdits = new Tours(1, "Updated Tour", "New Description", "Another Start", "Different End", new System.TimeSpan(1, 1, 0), 55.89, TransportType.Plane);
            await _tourService.UpdateTour(mockTourWithEdits);

            //Assert
            var result = await _context.Tours.FindAsync(mockTourWithEdits.Id);
            Assert.Equal(mockTourWithEdits.Name, result.Name);
            Assert.Equal(mockTourWithEdits.Description, result.Description);
            Assert.Equal(mockTourWithEdits.From, result.From);
            Assert.Equal(mockTourWithEdits.To, result.To);
            Assert.InRange(mockTourWithEdits.Distance - result.Distance, -0.0001, 0.0001);
            Assert.Equal(mockTourWithEdits.Duration, result.Duration);
            Assert.Equal(mockTourWithEdits.Transport, result.Transport);
        }
    }
}
