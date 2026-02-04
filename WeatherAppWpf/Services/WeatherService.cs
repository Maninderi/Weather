using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using WeatherAppWpf.Models;

namespace WeatherAppWpf.Services
{
    public class WeatherService
    {
        private readonly HttpClient _httpClient;

        public WeatherService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "WeatherAppWpf/1.0");
        }

        public async Task<CurrentWeather?> GetWeatherAsync(double lat, double lon)
        {
            var url = $"https://api.open-meteo.com/v1/forecast?latitude={lat}&longitude={lon}&current=temperature_2m,relative_humidity_2m,surface_pressure,wind_speed_10m,wind_direction_10m";

            var response = await _httpClient.GetFromJsonAsync<OpenMeteoResponse>(url);
            return response?.Current;
        }
    }
}
