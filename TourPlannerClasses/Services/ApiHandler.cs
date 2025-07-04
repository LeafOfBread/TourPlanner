using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TourPlannerClasses.Models;
using TourPlannerClasses.DB;
using System;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text.Json.Nodes;
using Newtonsoft.Json.Linq;

namespace TourPlanner.BusinessLogic.Services
{
    public interface IApiHandler
    {
        public Task<RouteInfo> GetRouteDirections(float startLon, float startLat,
            float endLon, float endLat,
            string profile);
        public Task<List<float>> GetCoordinates(string startLocation, string endLocation);
        public float[] HandleJsonResponseCoordinates(object data);
        public double Haversine(double lat1, double lon1, double lat2, double lon2);
    }

    public class ApiHandler : IApiHandler
    {
        private readonly string OpenRouteApiKey;
        private readonly List<string> ApiKeys;
        private readonly ConfigReader _configReader;
        private readonly HttpClient _httpClient;

        public ApiHandler(ConfigReader configReader, HttpClient httpClient)
        {
            try
            {
                _configReader = configReader;
                ApiKeys = _configReader.GetApiKeys();
                OpenRouteApiKey = ApiKeys[0];
                _httpClient = httpClient;
            }
            catch(Exception ex)
            {
                throw new ApiException("Error initializing ApiHandler.", ex);
            }
        }

        public async Task<RouteInfo> GetRouteDirections(
            float startLon, float startLat,
            float endLon, float endLat,
            string profile)
        {
            var url = $"https://api.openrouteservice.org/v2/directions/{profile}/geojson";

            var requestBody = new
            {
                coordinates = new[] {
            new[] { startLon, startLat },
            new[] { endLon, endLat }
        },
                instructions = false,
                geometry_simplify = false,
                units = "m"
            };

            var json = JsonConvert.SerializeObject(requestBody);

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.TryAddWithoutValidation("accept", "application/json, application/geo+json");
            request.Headers.TryAddWithoutValidation("authorization", OpenRouteApiKey);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            var responseJson = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new ApiException(
                    $"OpenRouteService API error: {response.StatusCode}\n{responseJson}"
                );
            }

            JObject jObject;
            try
            {
                jObject = JObject.Parse(responseJson);
            }
            catch (JsonReaderException ex)
            {
                throw new ApiException("Invalid JSON received from OpenRouteService.", ex);
            }

            var distance = jObject["features"]?[0]?["properties"]?["summary"]?["distance"]?.Value<double>() ?? 0;
            var duration = jObject["features"]?[0]?["properties"]?["summary"]?["duration"]?.Value<double>() ?? 0;

            var coordinates = jObject["features"]?[0]?["geometry"]?["coordinates"]?.ToObject<List<List<double>>>();

            if (coordinates == null)
                throw new ApiException("Directions API response contained no geometry.");

            return new RouteInfo
            {
                DistanceMeters = distance,
                DurationSeconds = duration,
                Geometry = coordinates
            };
        }

        public async Task<List<float>> GetCoordinates(string startLocation, string endLocation)
        {
            try
            {
                List<float> coordinates = new List<float>();

                var urls = new[]
                {
                $"https://api.openrouteservice.org/geocode/search?api_key={OpenRouteApiKey}&text={startLocation}",
                $"https://api.openrouteservice.org/geocode/search?api_key={OpenRouteApiKey}&text={endLocation}"
                };

                foreach (var url in urls)
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, url);
                    request.Headers.TryAddWithoutValidation("accept", "application/json, application/geo+json, application/gpx+xml, img/png; charset=utf-8");

                    var response = await _httpClient.SendAsync(request);
                    string responseData = await response.Content.ReadAsStringAsync();
                    var data = JsonConvert.DeserializeObject(responseData);
                    var coords = HandleJsonResponseCoordinates(data);
                    coordinates.Add(coords[0]);
                    coordinates.Add(coords[1]);
                }
                return coordinates;
            }
            catch(Exception ex)
            {
                throw new ApiException("Error occured while trying to fetch coordinates.", ex);
            }
            
        }

        public float[] HandleJsonResponseCoordinates(object data)
        {
            var json = data as JObject;
            var coordinates = json?["features"]?[0]?["geometry"]?["coordinates"];

            if (coordinates != null)
            {
                float lon = coordinates[0].Value<float>();
                float lat = coordinates[1].Value<float>();
                return new float[] { lon, lat };
            }

            else
                throw new ApiException("No coordinates returned from OpenRouteService geocoding.");
        }

        public double Haversine(double lat1, double lon1, double lat2, double lon2)
        {
            var R = 6371.0; // Earth radius in km
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private double ToRadians(double deg) => deg * (Math.PI / 180);
    }
    public class RouteInfo
    {
        public double DistanceMeters { get; set; }
        public double DurationSeconds { get; set; }
        public List<List<double>> Geometry { get; set; }
    }
}
