using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalJournalDesktopApp.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Icon { get; set; } = "📁";
        public string Color { get; set; } = "#ae866c";

        public static List<Category> GetDefaultCategories()
        {
            return new List<Category>
            {
                new Category { Id = 1, Name = "Personal", Icon = "👤", Color = "#ae866c" },
                new Category { Id = 2, Name = "Work", Icon = "💼", Color = "#8b7355" },
                new Category { Id = 3, Name = "Health", Icon = "❤️", Color = "#c9a987" },
                new Category { Id = 4, Name = "Travel", Icon = "✈️", Color = "#9d7d60" },
                new Category { Id = 5, Name = "Goals", Icon = "🎯", Color = "#b89778" },
                new Category { Id = 6, Name = "Gratitude", Icon = "🙏", Color = "#a38569" }
            };
        }
    }
}