using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PersonalJournalDesktopApp.Data;
using PersonalJournalDesktopApp.Models;

namespace PersonalJournalDesktopApp.Services
{
    public class TagService
    {
        private readonly DatabaseService _database;

        public TagService(DatabaseService database)
        {
            _database = database;
        }

        public async Task<List<Tag>> GetAllTagsAsync()
        {
            return await _database.GetAllTagsAsync();
        }

        public async Task<List<Tag>> GetTagsForEntryAsync(int entryId)
        {
            return await _database.GetTagsForEntryAsync(entryId);
        }

        public async Task<int> CreateTagAsync(string tagName)
        {
            var tag = new Tag
            {
                Name = tagName,
                IsPreDefined = false,
                Color = "#ae866c"
            };

            return await _database.CreateTagAsync(tag);
        }

        public async Task SaveEntryTagsAsync(int entryId, List<int> tagIds)
        {
            await _database.SaveEntryTagsAsync(entryId, tagIds);
        }
    }
}