using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TourPlanner.BusinessLogic.Services;
using TourPlannerClasses.DB;
using Xunit;

namespace UnitTests
{
    public class ApiHandlerTests
    {
        private readonly ConfigReader _configReader;

        public ApiHandlerTests()
        {
            // Optionally, you can mock ConfigReader
            var mockConfig = new Mock<ConfigReader>();
            mockConfig.Setup(c => c.GetApiKeys())
                      .Returns(new List<string> { "FAKE_OPENROUTE_APIKEY" });

            _configReader = mockConfig.Object;
        }

        [Fact]
        public async Task GetCoordinates_ShouldReturnCorrectCoordinates()
        {
            // Arrange
            var fakeGeoJson = new
            {
                features = new[]
                {
                    new
                    {
                        geometry = new
                        {
                            coordinates = new[] { 16.3738, 48.2082 }
                        }
                    }
                }
            };

            string jsonResponse = JsonConvert.SerializeObject(fakeGeoJson);

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
                });

            var httpClient = new HttpClient(handlerMock.Object);

            var apiHandler = new ApiHandler(_configReader, httpClient);

            // Act
            var result = await apiHandler.GetCoordinates("Vienna", "Graz");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(4, result.Count);
            Assert.Equal(16.3738f, result[0]);
            Assert.Equal(48.2082f, result[1]);
        }

        [Fact]
        public async Task GetRouteDirections_ShouldReturnRouteInfo()
        {
            // Arrange
            var fakeRouteJson = new
            {
                features = new[]
                {
                    new
                    {
                        properties = new
                        {
                            summary = new
                            {
                                distance = 12345.67,
                                duration = 543.21
                            }
                        },
                        geometry = new
                        {
                            coordinates = new[]
                            {
                                new [] {16.3738, 48.2082},
                                new [] {15.437, 47.0707}
                            }
                        }
                    }
                }
            };

            string jsonResponse = JsonConvert.SerializeObject(fakeRouteJson);

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
                });

            var httpClient = new HttpClient(handlerMock.Object);

            var apiHandler = new ApiHandler(_configReader, httpClient);

            // Act
            var result = await apiHandler.GetRouteDirections(16.3738f, 48.2082f, 15.437f, 47.0707f);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(12345.67, result.DistanceMeters);
            Assert.Equal(543.21, result.DurationSeconds);
            Assert.NotNull(result.Geometry);
            Assert.Equal(2, result.Geometry.Count);
            Assert.Equal(16.3738, result.Geometry[0][0]);
        }

        [Fact]
        public void HandleJsonResponseCoordinates_ShouldReturnCorrectValues()
        {
            // Arrange
            var fakeGeoJson = JObject.FromObject(new
            {
                features = new[]
                {
                    new
                    {
                        geometry = new
                        {
                            coordinates = new[] { 16.3738, 48.2082 }
                        }
                    }
                }
            });

            var apiHandler = new ApiHandler(_configReader, new HttpClient());

            // Act
            var coords = apiHandler.HandleJsonResponseCoordinates(fakeGeoJson);

            // Assert
            Assert.NotNull(coords);
            Assert.Equal(16.3738f, coords[0]);
            Assert.Equal(48.2082f, coords[1]);
        }

        [Fact]
        public async Task GetCoordinates_ShouldReturnCorrectCoords()
        {
            //Arrange
            var configReader = new ConfigReader();
            var httpClient = new HttpClient();

            var handler = new ApiHandler(configReader, httpClient);

            //Act
            var result = await handler.GetCoordinates("Wien", "Graz");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(4, result.Count);

            // approximate
            Assert.InRange(result[0], 15, 17);   // longitude Vienna
            Assert.InRange(result[1], 47, 49);   // latitude Vienna
            Assert.InRange(result[2], 14, 16);   // longitude Graz
            Assert.InRange(result[3], 46, 48);   // latitude Graz
        }

        [Fact]
        public async Task GetRouteDirections_ShouldReturnDistanceAndGeometry()
        {
            //Arrange
            var configReader = new ConfigReader();
            var httpClient = new HttpClient();

            var handler = new ApiHandler(configReader, httpClient);

            // Coordinates roughly Vienna center and Graz center
            float viennaLon = 16.3738f;
            float viennaLat = 48.2082f;
            float grazLon = 15.4395f;
            float grazLat = 47.0707f;

            //Act
            var route = await handler.GetRouteDirections(
                viennaLon, viennaLat,
                grazLon, grazLat,
                "driving-car");

            //Assert
            Assert.NotNull(route);
            Assert.True(route.DistanceMeters > 0, "Route distance must be greater than zero");
            Assert.True(route.DurationSeconds > 0, "Route duration must be greater than zero");
            Assert.NotNull(route.Geometry);
            Assert.True(route.Geometry.Count > 0, "Geometry must include atleast one line segment");
        }

        [Fact]
        public async Task GetRouteDirections_ShouldReturnCyclingRoute()
        {
            // Arrange
            var configReader = new ConfigReader();
            var apiKeys = configReader.GetApiKeys();
            var httpClient = new HttpClient();

            var handler = new ApiHandler(configReader, httpClient);

            float startLon = 16.3738f;
            float startLat = 48.2082f;
            float endLon = 16.3333f;
            float endLat = 48.2200f;

            // Act
            var route = await handler.GetRouteDirections(
                startLon,
                startLat,
                endLon,
                endLat,
                "cycling-regular"
            );

            // Assert
            Assert.NotNull(route);
            Assert.True(route.DistanceMeters > 0);
            Assert.True(route.DurationSeconds > 0);
            Assert.NotNull(route.Geometry);
        }
    }
}
