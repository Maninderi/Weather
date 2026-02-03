using System;

namespace WeatherAppWpf.Models
{
    public class WeatherLog
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string SensorId { get; set; } = string.Empty;
        public string Parameter { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
