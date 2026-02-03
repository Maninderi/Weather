using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WeatherApp.Data;
using WeatherApp.Models;
using WeatherApp.Services;

namespace WeatherApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly WeatherService _weatherService;
        private readonly WeatherContext _context;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(WeatherService weatherService, WeatherContext context, ILogger<IndexModel> logger)
        {
            _weatherService = weatherService;
            _context = context;
            _logger = logger;
        }

        public CurrentWeather? Weather { get; set; }
        public List<WeatherLog> Logs { get; set; } = new();

        public async Task OnGetAsync()
        {
            // Fetch Weather (Moscow)
            Weather = await _weatherService.GetWeatherAsync(55.7558, 37.6173);

            if (Weather != null)
            {
                // Log Temperature
                var logTemp = new WeatherLog
                {
                    Timestamp = DateTime.Now,
                    SensorId = "SEN-992",
                    Parameter = "Ambient Temp",
                    Value = $"{Weather.Temperature}°C",
                    Status = "STABLE"
                };

                // Log Pressure
                var logPress = new WeatherLog
                {
                    Timestamp = DateTime.Now,
                    SensorId = "SEN-811",
                    Parameter = "Barometer",
                    Value = $"{Weather.SurfacePressure} hPa",
                    Status = "STABLE"
                };

                 // Log Wind
                var logWind = new WeatherLog
                {
                    Timestamp = DateTime.Now,
                    SensorId = "SEN-044",
                    Parameter = "Wind Gust",
                    Value = $"{Weather.WindSpeed} km/h",
                    Status = Weather.WindSpeed > 20 ? "FLUCTUATING" : "STABLE"
                };

                _context.WeatherLogs.AddRange(logTemp, logPress, logWind);
                await _context.SaveChangesAsync();
            }

            // Retrieve Logs (Last 10)
            Logs = await _context.WeatherLogs
                .OrderByDescending(l => l.Timestamp)
                .Take(10)
                .ToListAsync();
        }
    }
}
