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

namespace TourPlanner.BusinessLogic.Services
{
    public interface IApiHandler
    {
        public Task GetRouteInformation(string start, string end);
    }

    public class ApiHandler : IApiHandler           //unfinished, no idea if this actually works, took this code from the openrouteservices documentation. api key should work in theory
    {
        private readonly string OpenRouteApiKey;
        private readonly string MapBoxApiKey;
        private readonly List<string> ApiKeys;
        private readonly ConfigReader _configReader;

        ApiHandler(ConfigReader configReader)
        {
            _configReader = configReader;
            ApiKeys = _configReader.GetApiKeys();
            OpenRouteApiKey = ApiKeys[0];
            MapBoxApiKey = ApiKeys[1];
        }

        public async Task GetRouteInformation(string start, string end)
        {
            var baseAddress = new Uri($"https://api.openrouteservice.org/v2/directions/driving-car?api_key={OpenRouteApiKey}&start={start}&end={end}");
            
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
    }
}
