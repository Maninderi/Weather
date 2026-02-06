using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WeatherAppWpf.Models
{
    public class OpenMeteoResponse
    {
        [JsonPropertyName("current")]
        public CurrentWeather? Current { get; set; }

        [JsonPropertyName("daily")]
        public DailyWeather? Daily { get; set; }
    }

    public class CurrentWeather
    {
        [JsonPropertyName("temperature_2m")]
        public double Temperature { get; set; }

        [JsonPropertyName("relative_humidity_2m")]
        public int RelativeHumidity { get; set; }

        [JsonPropertyName("surface_pressure")]
        public double SurfacePressure { get; set; }

        [JsonPropertyName("wind_speed_10m")]
        public double WindSpeed { get; set; }

        [JsonPropertyName("wind_direction_10m")]
        public int WindDirection { get; set; }

        [JsonPropertyName("weather_code")]
        public int WeatherCode { get; set; }
    }

    public class DailyWeather
    {
        [JsonPropertyName("time")]
        public List<string>? Time { get; set; }

        [JsonPropertyName("weather_code")]
        public List<int>? WeatherCode { get; set; }

        [JsonPropertyName("temperature_2m_max")]
        public List<double>? Temperature2mMax { get; set; }

        [JsonPropertyName("temperature_2m_min")]
        public List<double>? Temperature2mMin { get; set; }
    }
}
