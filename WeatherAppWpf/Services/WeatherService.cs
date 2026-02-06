using System;
using System.Linq;
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

        public async Task<OpenMeteoResponse?> GetWeatherAsync(double lat, double lon)
        {
            var url = FormattableString.Invariant($"https://api.open-meteo.com/v1/forecast?latitude={lat}&longitude={lon}&current=temperature_2m,relative_humidity_2m,surface_pressure,wind_speed_10m,wind_direction_10m,weather_code&daily=weather_code,temperature_2m_max,temperature_2m_min&timezone=auto");

            return await _httpClient.GetFromJsonAsync<OpenMeteoResponse>(url);
        }

        public async Task<GeocodingResult?> GetCityCoordinatesAsync(string cityName)
        {
            var url = FormattableString.Invariant($"https://geocoding-api.open-meteo.com/v1/search?name={cityName}&count=1&language=en&format=json");

            var response = await _httpClient.GetFromJsonAsync<GeocodingResponse>(url);
            return response?.Results?.FirstOrDefault();
        }
    }
}
