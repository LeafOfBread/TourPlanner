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
        public Task GetRouteDirections(string start, string end);
        public Task<List<float>> GetCoordinates(string startLocation, string endLocation);
        public float[] HandleJsonResponseCoordinates(object data);
    }

    public class ApiHandler : IApiHandler           //unfinished, no idea if this actually works, took this code from the openrouteservices documentation. api key should work in theory
    {
        private readonly string OpenRouteApiKey;
        private readonly string MapBoxApiKey;
        private readonly List<string> ApiKeys;
        private readonly ConfigReader _configReader;
        private readonly HttpClient _httpClient;

        public ApiHandler(ConfigReader configReader, HttpClient httpClient)
        {
            _configReader = configReader;
            ApiKeys = _configReader.GetApiKeys();
            OpenRouteApiKey = ApiKeys[0];
            MapBoxApiKey = ApiKeys[1];
            _httpClient = httpClient;
        }

        public async Task GetRouteDirections(string start, string end)
        {
            var baseAddress = new Uri("https://api.openrouteservice.org/");
            
            using (var httpClient = new HttpClient { BaseAddress = baseAddress })
            {
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("accept", "application/json, application/geo+json, application/gpx+xml, img/png; charset=utf-8");

                using (var response = await httpClient.GetAsync("directions"))
                {
                    string responseData = await response.Content.ReadAsStringAsync();
                    var data = JsonConvert.DeserializeObject(responseData);
                }
            }
        }

        public async Task<List<float>> GetCoordinates(string startLocation, string endLocation)
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

            return new float[2]; // default empty
        }
    }
}
