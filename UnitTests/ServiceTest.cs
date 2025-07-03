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
using System.Text.Json;

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
            _tourlogService = new TourLogService(_context);
            _tourService = new TourService(_context, _tourlogService, _apiHandler);

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
    new Tours(1, "Tour 1", "Description 1", "Start 1", 48.2, 16.37, "End 1", 48.21, 16.38, new TimeSpan(1, 0, 0), 10.5, TourPlannerClasses.Models.TransportType.Car),
    new Tours(2, "Tour 2", "Description 2", "Start 2", 48.3, 16.4, "End 2", 48.31, 16.41, new TimeSpan(2, 0, 0), 20.0, TourPlannerClasses.Models.TransportType.Car)
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
    new Tours(1, "Tour 1", "Description 1", "Start 1", 48.2, 16.37, "End 1", 48.21, 16.38, new TimeSpan(1, 0, 0), 10.5, TourPlannerClasses.Models.TransportType.Car),
    new Tours(2, "Tour 2", "Description 2", "Start 2", 48.3, 16.4, "End 2", 48.31, 16.41, new TimeSpan(2, 0, 0), 20.0, TourPlannerClasses.Models.TransportType.Bicycle)
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
            var tourToInsert = new Tours(
            1, "Tour 1", "Description 1",
            "Start 1", 48.2, 16.37,
            "End 1", 48.21, 16.38,
            new TimeSpan(1, 0, 0), 10.5,
            TourPlannerClasses.Models.TransportType.Car
);
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
            var mockTour = new Tours(
                1, "Tour 1", "Description 1",
                "Start 1", 48.2, 16.37,
                "End 1", 48.21, 16.38,
                new TimeSpan(1, 0, 0), 10.5,
                TourPlannerClasses.Models.TransportType.Car
            );
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
            var mockTour = new Tours(
                1, "Tour 1", "Description 1",
                "Start 1", 48.2, 16.37,
                "End 1", 48.21, 16.38,
                new TimeSpan(1, 0, 0), 10.5,
                TourPlannerClasses.Models.TransportType.Walking
            );
            await _context.AddAsync(mockTour);
            await _context.SaveChangesAsync();

            //Act
            var mockTourWithEdits = new Tours(
                1, "Updated Tour", "New Description",
                "Another Start", 48.3, 16.4,
                "Different End", 48.31, 16.41,
                new TimeSpan(1, 1, 0), 55.89,
                TourPlannerClasses.Models.TransportType.Walking
            );
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
                new Tours(1, "SimilarSoundingTour", "Description 1", "Start 1", 43.123, 16.340, "End 1", 41.320, 14.320, new TimeSpan(1, 0, 0), 10.5, TourPlannerClasses.Models.TransportType.Car),
                new Tours(2, "SmilerSoundTour", "Description 2", "Start 2", 43.123, 16.340, "End 2", 41.320, 14.320, new TimeSpan(2, 0, 0), 20.0, TourPlannerClasses.Models.TransportType.Car),
                new Tours(1, "ThisIsNotTheSame", "Description 1", "Start 1", 43.123, 16.340, "End 1", 41.320, 14.320, new TimeSpan(1, 0, 0), 10.5, TourPlannerClasses.Models.TransportType.Car),
                new Tours(2, "Smiler", "Description 2", "Start 2", 43.123, 16.340, "End 2", 41.320, 14.320, new TimeSpan(2, 0, 0), 20.0, TourPlannerClasses.Models.TransportType.Car),
                new Tours(1, "Simulation", "Description 1", "Start 1", 43.123, 16.340, "End 1", 41.320, 14.320, new TimeSpan(1, 0, 0), 10.5, TourPlannerClasses.Models.TransportType.Car),
                new Tours(2, "Test", "Description 2", "Start 2", 43.123, 16.340, "End 2", 41.320, 14.320, new TimeSpan(2, 0, 0), 20.0, TourPlannerClasses.Models.TransportType.Car),
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
            configReaderMock.Setup(c => c.GetApiKeys()).Returns(new List<string> { "fake-api-key" });

            var handler = new ApiHandler(configReaderMock.Object, client);

            // Act
            var result = await handler.GetCoordinates("Vienna", "Graz");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(4, result.Count); // [lon, lat, lon, lat]
            Assert.Equal(48.2082f, result[0], 3);
            Assert.Equal(16.3738f, result[1], 3);
        }
        [Fact]
        public async Task ExportTourToFileAsync_WritesCorrectJson()
        {
            // Arrange
            var fakeTour = new Tours
            {
                Name = "Test Tour",
                Description = "Description",
                From = "Vienna",
                FromLat = 48.2,
                FromLng = 16.3,
                To = "Graz",
                ToLat = 47.07,
                ToLng = 15.43,
                Duration = TimeSpan.FromHours(2),
                Distance = 120.5,
                Transport = TourPlannerClasses.Models.TransportType.Car,
                Tourlogs = new List<Tourlog>
                {
                    new Tourlog
                    {
                        Date = new DateTime(2025, 01, 01),
                        Comment = "Great ride!",
                        Difficulty = Difficulty.Medium,
                        TotalDistance = 120,
                        TotalTime = TimeSpan.FromHours(2),
                        Rating = 5,
                        Author = "Tester"
                    }
                }
            };

            var mockLogService = new Mock<TourLogService>(null);
            var tourService = new TourService(null, mockLogService.Object, _apiHandler);

            var tempFile = Path.GetTempFileName();

            try
            {
                // Act
                await tourService.ExportTourToFileAsync(fakeTour, tempFile);

                // Assert
                var json = await File.ReadAllTextAsync(tempFile);
                var dto = JsonSerializer.Deserialize<TourExportDto>(json);

                Assert.NotNull(dto);
                Assert.Equal(fakeTour.Name, dto.Name);
                Assert.Equal(fakeTour.From, dto.From);
                Assert.Equal(fakeTour.To, dto.To);
                Assert.Single(dto.Tourlogs);
                Assert.Equal("Tester", dto.Tourlogs[0].Author);
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        [Fact]
        public async Task ImportTourFromFileAsync_ReadsAndMapsCorrectly()
        {
            // Arrange
            var dto = new TourExportDto
            {
                Name = "Imported Tour",
                Description = "Some import",
                From = "Vienna",
                FromLat = 48.2,
                FromLng = 16.3,
                To = "Graz",
                ToLat = 47.07,
                ToLng = 15.43,
                Duration = TimeSpan.FromHours(2),
                Distance = 120.5,
                Transport = TourPlannerClasses.Models.TransportType.Car,
                Tourlogs = new List<TourlogExportDto>
                {
                    new TourlogExportDto
                    {
                        Date = new DateTime(2025, 02, 02),
                        Comment = "Imported log",
                        Difficulty = (int)Difficulty.Medium,
                        TotalDistance = 120,
                        TotalTime = TimeSpan.FromHours(2),
                        Rating = 4,
                        Author = "Importer"
                    }
                }
            };

            var json = JsonSerializer.Serialize(dto, new JsonSerializerOptions { WriteIndented = true });
            var tempFile = Path.GetTempFileName();

            await File.WriteAllTextAsync(tempFile, json);

            var mockLogService = new Mock<TourLogService>(null);
            var tourService = new TourService(null, mockLogService.Object, _apiHandler);

            // Act
            var importedTour = await tourService.ImportTourFromFileAsync(tempFile);

            // Assert
            Assert.NotNull(importedTour);
            Assert.Equal(dto.Name, importedTour.Name);
            Assert.Equal(dto.From, importedTour.From);
            Assert.Equal(dto.To, importedTour.To);
            Assert.Single(importedTour.Tourlogs);
            Assert.Equal("Importer", importedTour.Tourlogs.First().Author);

            mockLogService.Verify(
                s => s.InsertTourLog(It.Is<Tourlog>(tl => tl.Author == "Importer")),
                Times.Once);

            // Clean up
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }

        [Fact]
        public async Task ImportTourFromFileAsync_ReturnsNull_WhenFileDoesNotExist()
        {
            var mockLogService = new Mock<TourLogService>(null);
            var tourService = new TourService(null, mockLogService.Object, _apiHandler);

            var result = await tourService.ImportTourFromFileAsync("nonexistent_file.json");

            Assert.Null(result);
        }
    }
}
