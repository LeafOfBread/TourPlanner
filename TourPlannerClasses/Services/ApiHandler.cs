using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TourPlannerClasses.Models;
using System;
using System.Net.Http;
using Newtonsoft.Json;

namespace TourPlanner.BusinessLogic.Services
{
    public interface IApiHandler
    {
        public Task<Tours> GetRouteInformation(string start, string end);
    }

    public class ApiHandler : IApiHandler           //unfinished, no idea if this actually works, took this code from the openrouteservices documentation. api key should work in theory
    {
        public string Url;
        private readonly string ApiKey = "5b3ce3597851110001cf62485cf43c3fbd16446584cc2d3271c39cd0";

        public async Task<Tours> GetRouteInformation(string start, string end)
        {
            var baseAddress = new Uri($"https://api.openrouteservice.org/v2/directions/driving-car?api_key={ApiKey}&start={start}&end={end}");

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
            return null;
        }
    }
}
