namespace WeatherAppWpf.Models
{
    public class DailyForecastModel
    {
        public string DayName { get; set; } = string.Empty;
        public double MaxTemp { get; set; }
        public double MinTemp { get; set; }
        public int WeatherCode { get; set; }
    }
}
