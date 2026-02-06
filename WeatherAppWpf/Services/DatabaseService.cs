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

        public async Task ClearLogsAsync()
        {
            // Truncate logs
            var logs = _context.WeatherLogs.ToList();
            _context.WeatherLogs.RemoveRange(logs);
            await _context.SaveChangesAsync();
        }

        public async Task<List<FavoriteCity>> GetFavoritesAsync()
        {
            return await _context.Favorites.ToListAsync();
        }

        public async Task AddFavoriteAsync(FavoriteCity city)
        {
            var exists = await _context.Favorites
                .AnyAsync(f => f.Name == city.Name && f.Country == city.Country);

            if (!exists)
            {
                await _context.Favorites.AddAsync(city);
                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoveFavoriteAsync(FavoriteCity city)
        {
            if (_context.Entry(city).State == EntityState.Detached)
            {
                var existing = await _context.Favorites.FindAsync(city.Id);
                if (existing != null)
                {
                    _context.Favorites.Remove(existing);
                }
            }
            else
            {
                _context.Favorites.Remove(city);
            }
            await _context.SaveChangesAsync();
        }
    }
}
