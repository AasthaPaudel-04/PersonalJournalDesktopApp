using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PersonalJournalDesktopApp.Data;
using PersonalJournalDesktopApp.Models;

namespace PersonalJournalDesktopApp.Services
{
    public class JournalService
    {
        private readonly DatabaseService _database;

        public JournalService(DatabaseService database)
        {
            _database = database;
        }

        // Get today's entry
        public async Task<JournalEntry?> GetTodayEntryAsync()
        {
            return await GetEntryByDateAsync(DateTime.Today);
        }

        // Get entry by specific date - FIXED: Now loads EVERYTHING
        public async Task<JournalEntry?> GetEntryByDateAsync(DateTime date)
        {
            var entry = await _database.GetEntryByDateAsync(date);

            if (entry != null)
            {
                // Load mood details for display
                if (entry.PrimaryMoodId.HasValue)
                    entry.PrimaryMood = await _database.GetMoodByIdAsync(entry.PrimaryMoodId.Value);

                if (entry.SecondaryMood1Id.HasValue)
                    entry.SecondaryMood1 = await _database.GetMoodByIdAsync(entry.SecondaryMood1Id.Value);

                if (entry.SecondaryMood2Id.HasValue)
                    entry.SecondaryMood2 = await _database.GetMoodByIdAsync(entry.SecondaryMood2Id.Value);

                // FIXED: Load category
                if (entry.CategoryId.HasValue)
                    entry.Category = await _database.GetCategoryByIdAsync(entry.CategoryId.Value);

                // FIXED: Load tags
                entry.Tags = await _database.GetTagsForEntryAsync(entry.Id);
            }

            return entry;
        }

        // Get all entries (for recent entries list)
        public async Task<List<JournalEntry>> GetAllEntriesAsync()
        {
            var entries = await _database.GetAllEntriesAsync();

            // FIXED: Load related data for each entry
            foreach (var entry in entries)
            {
                // Load moods
                if (entry.PrimaryMoodId.HasValue)
                    entry.PrimaryMood = await _database.GetMoodByIdAsync(entry.PrimaryMoodId.Value);

                if (entry.SecondaryMood1Id.HasValue)
                    entry.SecondaryMood1 = await _database.GetMoodByIdAsync(entry.SecondaryMood1Id.Value);

                if (entry.SecondaryMood2Id.HasValue)
                    entry.SecondaryMood2 = await _database.GetMoodByIdAsync(entry.SecondaryMood2Id.Value);

                // Load category
                if (entry.CategoryId.HasValue)
                    entry.Category = await _database.GetCategoryByIdAsync(entry.CategoryId.Value);

                // Load tags
                entry.Tags = await _database.GetTagsForEntryAsync(entry.Id);
            }

            return entries;
        }

        // Save (Create or Update) entry
        public async Task<int> SaveEntryAsync(JournalEntry entry)
        {
            return await _database.SaveEntryAsync(entry);
        }

        // Delete entry
        public async Task<bool> DeleteEntryAsync(int id)
        {
            return await _database.DeleteEntryAsync(id);
        }

        // Check if entry exists for a date (Calendar Navigation feature)
        public async Task<bool> HasEntryForDateAsync(DateTime date)
        {
            var entry = await _database.GetEntryByDateAsync(date);
            return entry != null;
        }
    }
}