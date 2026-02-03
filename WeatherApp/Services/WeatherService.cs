using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace WeatherApp.Services
{
    public class WeatherService
    {
        private readonly HttpClient _httpClient;

        public WeatherService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<CurrentWeather?> GetWeatherAsync(double lat, double lon)
        {
            var url = $"https://api.open-meteo.com/v1/forecast?latitude={lat}&longitude={lon}&current=temperature_2m,relative_humidity_2m,surface_pressure,wind_speed_10m,wind_direction_10m";

            try
            {
                var response = await _httpClient.GetFromJsonAsync<OpenMeteoResponse>(url);
                return response?.Current;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching weather: {ex.Message}");
                return null;
            }
        }
    }
}
