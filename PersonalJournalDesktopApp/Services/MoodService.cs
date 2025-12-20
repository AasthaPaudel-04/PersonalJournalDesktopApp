using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PersonalJournalDesktopApp.Data;
using PersonalJournalDesktopApp.Models;

namespace PersonalJournalDesktopApp.Services
{
    public class MoodService
    {
        private readonly DatabaseService _database;

        public MoodService(DatabaseService database)
        {
            _database = database;
        }

        // Get all moods for picker
        public async Task<List<Mood>> GetAllMoodsAsync()
        {
            return await _database.GetAllMoodsAsync();
        }

        // Get moods filtered by category
        public async Task<List<Mood>> GetMoodsByCategoryAsync(MoodCategory category)
        {
            var allMoods = await GetAllMoodsAsync();
            return allMoods.Where(m => m.Category == category).ToList();
        }

        // Get specific mood by ID
        public async Task<Mood?> GetMoodByIdAsync(int id)
        {
            return await _database.GetMoodByIdAsync(id);
        }
    }
}
