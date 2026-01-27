using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PersonalJournalDesktopApp.Data;
using PersonalJournalDesktopApp.Models;

namespace PersonalJournalDesktopApp.Services
{
    public class SearchService
    {
        private readonly DatabaseService _database;

        public SearchService(DatabaseService database)
        {
            _database = database;
        }

        public async Task<List<JournalEntry>> SearchAsync(
            string? searchText = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            List<int>? moodIds = null,
            List<int>? tagIds = null,
            int? categoryId = null)
        {
            var results = await _database.SearchEntriesAsync(
                searchText, startDate, endDate, moodIds, tagIds, categoryId);

            // Load navigation properties
            foreach (var entry in results)
            {
                if (entry.PrimaryMoodId.HasValue)
                    entry.PrimaryMood = await _database.GetMoodByIdAsync(entry.PrimaryMoodId.Value);

                if (entry.SecondaryMood1Id.HasValue)
                    entry.SecondaryMood1 = await _database.GetMoodByIdAsync(entry.SecondaryMood1Id.Value);

                if (entry.SecondaryMood2Id.HasValue)
                    entry.SecondaryMood2 = await _database.GetMoodByIdAsync(entry.SecondaryMood2Id.Value);

                if (entry.CategoryId.HasValue)
                    entry.Category = await _database.GetCategoryByIdAsync(entry.CategoryId.Value);

                entry.Tags = await _database.GetTagsForEntryAsync(entry.Id);
            }

            return results;
        }
    }
}