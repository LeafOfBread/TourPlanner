using Microsoft.EntityFrameworkCore;
using Moq.Protected;
using Moq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TourPlanner.BusinessLogic.Services;
using TourPlannerClasses.DB;
using TourPlannerClasses.Models;
using Xunit;

namespace UnitTests
{
    public class TourServiceTests
    {
        private readonly TourDbContext _context;
        private readonly TourService _tourService;
        private readonly TourLogService _tourlogService;
        private readonly ApiHandler _apiHandler;
        private readonly ConfigReader _configReader;

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
            new Tours(1, "Tour 1", "Description 1", "Start 1", "End 1", new TimeSpan(1, 0, 0), 10.5, TourPlannerClasses.Models.TransportType.Boat),
            new Tours(2, "Tour 2", "Description 2", "Start 2", "End 2", new TimeSpan(2, 0, 0), 20.0, TourPlannerClasses.Models.TransportType.Bus)
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
            new Tours(1, "Tour 1", "Description 1", "Start 1", "End 1", new TimeSpan(1, 0, 0), 10.5, TourPlannerClasses.Models.TransportType.Boat),
            new Tours(2, "Tour 2", "Description 2", "Start 2", "End 2", new TimeSpan(2, 0, 0), 20.0, TourPlannerClasses.Models.TransportType.Bus)
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
            var tourToInsert = new Tours(1, "Tour 1", "Description 1", "Start 1", "End 1", new TimeSpan(1, 0, 0), 10.5, TourPlannerClasses.Models.TransportType.Boat);
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
            var mockTour = new Tours(1, "Tour 1", "Description 1", "Start 1", "End 1", new TimeSpan(1, 0, 0), 10.5, TourPlannerClasses.Models.TransportType.Boat);
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
            var mockTour = new Tours(1, "Tour 1", "Description 1", "Start 1", "End 1", new TimeSpan(1, 0, 0), 10.5, TourPlannerClasses.Models.TransportType.Boat);
            await _context.AddAsync(mockTour);
            await _context.SaveChangesAsync();

            //Act
            var mockTourWithEdits = new Tours(1, "Updated Tour", "New Description", "Another Start", "Different End", new TimeSpan(1, 1, 0), 55.89, TourPlannerClasses.Models.TransportType.Plane);
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

        [Fact]
        public async Task SearchForTours_ShouldReturnSimilarTours()
        {
            //Arrange
            ObservableCollection<Tours> listOfTours = new ObservableCollection<Tours>()
            {
                new Tours(1, "SimilarSoundingTour", "Description 1", "Start 1", "End 1", new TimeSpan(1, 0, 0), 10.5, TourPlannerClasses.Models.TransportType.Boat),
                new Tours(2, "SmilerSoundTour", "Description 2", "Start 2", "End 2", new TimeSpan(2, 0, 0), 20.0, TourPlannerClasses.Models.TransportType.Bus),
                new Tours(1, "ThisIsNotTheSame", "Description 1", "Start 1", "End 1", new TimeSpan(1, 0, 0), 10.5, TourPlannerClasses.Models.TransportType.Boat),
                new Tours(2, "Smiler", "Description 2", "Start 2", "End 2", new TimeSpan(2, 0, 0), 20.0, TourPlannerClasses.Models.TransportType.Bus),
                new Tours(1, "Simulation", "Description 1", "Start 1", "End 1", new TimeSpan(1, 0, 0), 10.5, TourPlannerClasses.Models.TransportType.Boat),
                new Tours(2, "Test", "Description 2", "Start 2", "End 2", new TimeSpan(2, 0, 0), 20.0, TourPlannerClasses.Models.TransportType.Bus),
            };

            ObservableCollection<Tourlog> emptyTourlogs = new ObservableCollection<Tourlog>() { };

            string tourNameToLookFor = "SimilarTour";
            var expectedTourNames = new List<string> { "SimilarSoundingTour", "SmilerSoundTour", "Smiler", "Simulation" };

            //Act
            var result = await _tourService.SearchForTours(tourNameToLookFor, listOfTours, emptyTourlogs);


            //Assert
            Assert.NotNull(result);
            Assert.IsType<ObservableCollection<Tours>>(result);
            Assert.Equal(expectedTourNames.Count, result.Count);

            var resultNames = result.Select(t => t.Name).ToList();
            foreach (var expectedName in expectedTourNames)
            {
                Assert.Contains(expectedName, resultNames);
            }
        }

        [Fact]
        public async Task GetCoordinates_ReturnsCorrectCoordinates_WhenApiReturnsValidData()
        {
            // Arrange
            var fakeResponseJson = "{\"features\":[{\"geometry\":{\"coordinates\":[48.2082, 16.3738]}}]}";

            var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            httpMessageHandlerMock
                .Protected()
                .SetupSequence<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(fakeResponseJson, Encoding.UTF8, "application/json")
                }) // startLocation
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(fakeResponseJson, Encoding.UTF8, "application/json")
                }); // endLocation

            var client = new HttpClient(httpMessageHandlerMock.Object);

            var configReaderMock = new Mock<ConfigReader>();
            configReaderMock.Setup(c => c.GetApiKeys()).Returns(new List<string> { "fake-api-key", "fake-mapbox-key" });

            var handler = new ApiHandler(configReaderMock.Object, client);

            // Act
            var result = await handler.GetCoordinates("Vienna", "Graz");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(4, result.Count); // [lon, lat, lon, lat]
            Assert.Equal(48.2082f, result[0], 3);
            Assert.Equal(16.3738f, result[1], 3);
        }


    }
}
