using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading;
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
        private CancellationTokenSource? _searchCts;

        // Current Coords
        private double _latitude = 55.7558;
        private double _longitude = 37.6173;
        private string _currentCityNameRaw = "Москва";
        private string _currentCountryRaw = "Россия";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsWeatherVisible))]
        [NotifyPropertyChangedFor(nameof(IsOverviewVisible))]
        [NotifyPropertyChangedFor(nameof(IsSettingsVisible))]
        private int currentView = 0; // 0: Weather, 1: Overview, 2: Settings

        public bool IsWeatherVisible => CurrentView == 0;
        public bool IsOverviewVisible => CurrentView == 1;
        public bool IsSettingsVisible => CurrentView == 2;

        [ObservableProperty]
        private string searchText = string.Empty;

        [ObservableProperty]
        private ObservableCollection<GeocodingResult> suggestions = new();

        [ObservableProperty]
        private GeocodingResult? selectedSuggestion;

        [ObservableProperty]
        private string cityName = "Москва, Россия";

        [ObservableProperty]
        private string weatherDescription = "Ясно / Солнечно";

        [ObservableProperty]
        private string temperature = "--";

        [ObservableProperty]
        private string feelsLike = "--";

        [ObservableProperty]
        private string windSpeed = "--";

        [ObservableProperty]
        private string windDirection = "--";

        [ObservableProperty]
        private string uVIndex = "4";

        [ObservableProperty]
        private string humidity = "--";

        [ObservableProperty]
        private string pressure = "--";

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        public ObservableCollection<WeatherLog> Logs { get; } = new();
        public ObservableCollection<DailyForecastModel> Forecasts { get; } = new();
        public ObservableCollection<FavoriteCity> Favorites { get; } = new();

        public MainViewModel()
        {
            _weatherService = new WeatherService();
            _databaseService = new DatabaseService();

            // Load initial data
            LoadDataCommand.Execute(null);
            LoadFavoritesCommand.Execute(null);
        }

        async partial void OnSearchTextChanged(string value)
        {
            _searchCts?.Cancel();
            _searchCts = new CancellationTokenSource();
            var token = _searchCts.Token;

            try
            {
                await Task.Delay(500, token);
                if (token.IsCancellationRequested) return;

                if (string.IsNullOrWhiteSpace(value))
                {
                    Suggestions.Clear();
                    return;
                }

                var results = await _weatherService.GetCitySuggestionsAsync(value);
                if (!token.IsCancellationRequested)
                {
                    Suggestions.Clear();
                    foreach (var r in results) Suggestions.Add(r);
                }
            }
            catch (Exception)
            {
                // Ignore errors during suggestion fetch
            }
        }

        partial void OnSelectedSuggestionChanged(GeocodingResult? value)
        {
            if (value != null)
            {
                _latitude = value.Latitude;
                _longitude = value.Longitude;
                _currentCityNameRaw = value.Name;
                _currentCountryRaw = value.Country ?? "";
                CityName = value.Country != null ? $"{value.Name}, {value.Country}" : value.Name;

                // Don't clear search text immediately, maybe?
                // Actually, let's clear suggestions to close the popup.
                Suggestions.Clear();

                LoadDataCommand.Execute(null);
            }
        }

        [RelayCommand]
        private async Task NavigateAsync(string view)
        {
            switch (view)
            {
                case "W": CurrentView = 0; break;
                case "O":
                    CurrentView = 1;
                    await LoadFavoritesAsync();
                    break;
                case "S": CurrentView = 2; break;
            }
        }

        [RelayCommand]
        private async Task SearchCityAsync()
        {
            // Fallback for manual Enter key if needed, though suggestion selection is primary
            if (string.IsNullOrWhiteSpace(SearchText)) return;

            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                var result = await _weatherService.GetCityCoordinatesAsync(SearchText);
                if (result != null)
                {
                    _latitude = result.Latitude;
                    _longitude = result.Longitude;
                    _currentCityNameRaw = result.Name;
                    _currentCountryRaw = result.Country ?? "";
                    CityName = result.Country != null ? $"{result.Name}, {result.Country}" : result.Name;
                    SearchText = string.Empty;
                    Suggestions.Clear();
                    await LoadDataAsync();
                }
                else
                {
                    ErrorMessage = "Город не найден.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка поиска: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task LoadDataAsync()
        {
            if (IsLoading) return;
            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                var response = await _weatherService.GetWeatherAsync(_latitude, _longitude);

                if (response?.Current != null)
                {
                    var current = response.Current;
                    Temperature = $"{current.Temperature}°";
                    FeelsLike = $"{current.Temperature}°";
                    WindSpeed = $"{current.WindSpeed}";
                    WindDirection = $"{current.WindDirection}";
                    Humidity = $"{current.RelativeHumidity}";
                    Pressure = $"{current.SurfacePressure}";
                    WeatherDescription = GetWeatherDescription(current.WeatherCode);

                    Forecasts.Clear();
                    if (response.Daily != null && response.Daily.Time != null)
                    {
                        for (int i = 0; i < response.Daily.Time.Count; i++)
                        {
                            if (i >= 7) break;

                            DateTime date;
                            if (DateTime.TryParse(response.Daily.Time[i], out date))
                            {
                                var dayName = date.ToString("ddd", new CultureInfo("ru-RU")).ToUpper();
                                Forecasts.Add(new DailyForecastModel
                                {
                                    DayName = dayName,
                                    MaxTemp = response.Daily.Temperature2mMax?[i] ?? 0,
                                    MinTemp = response.Daily.Temperature2mMin?[i] ?? 0,
                                    WeatherCode = response.Daily.WeatherCode?[i] ?? 0
                                });
                            }
                        }
                    }

                     var logTemp = new WeatherLog
                    {
                        Timestamp = DateTime.Now,
                        SensorId = "SEN-992",
                        Parameter = "Температура",
                        Value = $"{current.Temperature}°C",
                        Status = "СТАБИЛЬНО"
                    };

                    var logPress = new WeatherLog
                    {
                        Timestamp = DateTime.Now,
                        SensorId = "SEN-811",
                        Parameter = "Давление",
                        Value = $"{current.SurfacePressure} hPa",
                        Status = "СТАБИЛЬНО"
                    };

                    var logWind = new WeatherLog
                    {
                        Timestamp = DateTime.Now,
                        SensorId = "SEN-044",
                        Parameter = "Порывы ветра",
                        Value = $"{current.WindSpeed} km/h",
                        Status = current.WindSpeed > 20 ? "НЕСТАБИЛЬНО" : "СТАБИЛЬНО"
                    };

                    await _databaseService.AddLogsAsync(new[] { logTemp, logPress, logWind });
                }

                var recentLogs = await _databaseService.GetRecentLogsAsync();
                Logs.Clear();
                foreach (var log in recentLogs)
                {
                    Logs.Add(log);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task AddFavoriteAsync()
        {
            var city = new FavoriteCity
            {
                Name = _currentCityNameRaw,
                Country = _currentCountryRaw,
                Latitude = _latitude,
                Longitude = _longitude
            };

            await _databaseService.AddFavoriteAsync(city);
            await LoadFavoritesAsync();
        }

        [RelayCommand]
        private async Task DeleteFavoriteAsync(FavoriteCity city)
        {
            if (city == null) return;
            await _databaseService.RemoveFavoriteAsync(city);
            await LoadFavoritesAsync();
        }

        [RelayCommand]
        private async Task LoadFavoritesAsync()
        {
            var favs = await _databaseService.GetFavoritesAsync();
            Favorites.Clear();
            foreach (var f in favs) Favorites.Add(f);
        }

        [RelayCommand]
        private async Task SelectFavoriteAsync(FavoriteCity city)
        {
            if (city == null) return;
            _latitude = city.Latitude;
            _longitude = city.Longitude;
            _currentCityNameRaw = city.Name;
            _currentCountryRaw = city.Country;
            CityName = string.IsNullOrEmpty(city.Country) ? city.Name : $"{city.Name}, {city.Country}";

            CurrentView = 0; // Go to Weather
            await LoadDataAsync();
        }

        [RelayCommand]
        private async Task ClearLogsAsync()
        {
            await _databaseService.ClearLogsAsync();
            Logs.Clear();
        }

        private string GetWeatherDescription(int code)
        {
            return code switch
            {
                0 => "Ясно / Солнечно",
                1 => "Преимущественно ясно",
                2 => "Переменная облачность",
                3 => "Пасмурно",
                45 => "Туман",
                48 => "Изморозь",
                51 => "Легкая морось",
                53 => "Умеренная морось",
                55 => "Сильная морось",
                61 => "Слабый дождь",
                63 => "Умеренный дождь",
                65 => "Сильный дождь",
                71 => "Слабый снег",
                73 => "Умеренный снег",
                75 => "Сильный снег",
                77 => "Снежные зерна",
                80 => "Ливень (слабый)",
                81 => "Ливень (умеренный)",
                82 => "Ливень (сильный)",
                95 => "Гроза",
                96 => "Гроза с градом",
                99 => "Сильная гроза с градом",
                _ => "Неизвестно"
            };
        }
    }
}
