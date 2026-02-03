using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WeatherAppWpf.Data;
using WeatherAppWpf.Models;

namespace WeatherAppWpf.Services
{
    public class DatabaseService
    {
        private readonly WeatherContext _context;

        public DatabaseService()
        {
            _context = new WeatherContext();
            _context.Database.EnsureCreated();
        }

        public async Task AddLogsAsync(IEnumerable<WeatherLog> logs)
        {
            await _context.WeatherLogs.AddRangeAsync(logs);
            await _context.SaveChangesAsync();
        }

        public async Task<List<WeatherLog>> GetRecentLogsAsync(int count = 10)
        {
            return await _context.WeatherLogs
                .OrderByDescending(l => l.Timestamp)
                .Take(count)
                .ToListAsync();
        }
    }
}
