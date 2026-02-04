using Microsoft.EntityFrameworkCore;
using WeatherAppWpf.Models;
using System.IO;

namespace WeatherAppWpf.Data
{
    public class WeatherContext : DbContext
    {
        public DbSet<WeatherLog> WeatherLogs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source={Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "weather.db")}");
        }
    }
}
