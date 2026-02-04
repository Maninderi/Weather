using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WeatherAppWpf.Models;
using WeatherAppWpf.Services;

namespace WeatherAppWpf.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly WeatherService _weatherService;
        private readonly DatabaseService _databaseService;

        [ObservableProperty]
        private string temperature = "--";

        [ObservableProperty]
        private string feelsLike = "--"; // API doesn't provide apparent temp in this call, reuse temp or fetch extra

        [ObservableProperty]
        private string windSpeed = "--";

        [ObservableProperty]
        private string windDirection = "--";

        [ObservableProperty]
        private string uVIndex = "4"; // Placeholder as requested design has static 4

        [ObservableProperty]
        private string humidity = "--";

        [ObservableProperty]
        private string pressure = "--";

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        public ObservableCollection<WeatherLog> Logs { get; } = new();

        public MainViewModel()
        {
            _weatherService = new WeatherService();
            _databaseService = new DatabaseService();

            // Load initial data
            LoadDataCommand.Execute(null);
        }

        [RelayCommand]
        private async Task LoadDataAsync()
        {
            if (IsLoading) return;
            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                // Fetch Weather (Moscow)
                var weather = await _weatherService.GetWeatherAsync(55.7558, 37.6173);

                if (weather != null)
                {
                    Temperature = $"{weather.Temperature}°";
                    FeelsLike = $"{weather.Temperature}°"; // Placeholder
                    WindSpeed = $"{weather.WindSpeed}";
                    WindDirection = $"{weather.WindDirection}°";
                    Humidity = $"{weather.RelativeHumidity}";
                    Pressure = $"{weather.SurfacePressure}";

                    // Save Logs
                     var logTemp = new WeatherLog
                    {
                        Timestamp = DateTime.Now,
                        SensorId = "SEN-992",
                        Parameter = "Ambient Temp",
                        Value = $"{weather.Temperature}°C",
                        Status = "STABLE"
                    };

                    var logPress = new WeatherLog
                    {
                        Timestamp = DateTime.Now,
                        SensorId = "SEN-811",
                        Parameter = "Barometer",
                        Value = $"{weather.SurfacePressure} hPa",
                        Status = "STABLE"
                    };

                    var logWind = new WeatherLog
                    {
                        Timestamp = DateTime.Now,
                        SensorId = "SEN-044",
                        Parameter = "Wind Gust",
                        Value = $"{weather.WindSpeed} km/h",
                        Status = weather.WindSpeed > 20 ? "FLUCTUATING" : "STABLE"
                    };

                    await _databaseService.AddLogsAsync(new[] { logTemp, logPress, logWind });
                }

                // Refresh Logs
                var recentLogs = await _databaseService.GetRecentLogsAsync();
                Logs.Clear();
                foreach (var log in recentLogs)
                {
                    Logs.Add(log);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
