using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PersonalJournalDesktopApp.Data;
using PersonalJournalDesktopApp.Models;

namespace PersonalJournalDesktopApp.Services
{
    public class CategoryService
    {
        private readonly DatabaseService _database;

        public CategoryService(DatabaseService database)
        {
            _database = database;
        }

        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            return await _database.GetAllCategoriesAsync();
        }

        public async Task<Category?> GetCategoryByIdAsync(int id)
        {
            return await _database.GetCategoryByIdAsync(id);
        }
    }
}