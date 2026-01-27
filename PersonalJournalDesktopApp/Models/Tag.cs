using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalJournalDesktopApp.Models
{
    public class Tag
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsPreDefined { get; set; }
        public string Color { get; set; } = "#ae866c"; // Default accent color

        public static List<Tag> GetDefaultTags()
        {
            var tags = new List<string>
            {
                "Work", "Career", "Studies", "Family", "Friends", "Relationships",
                "Health", "Fitness", "Personal Growth", "Self-care", "Hobbies",
                "Travel", "Nature", "Finance", "Spirituality", "Birthday",
                "Holiday", "Vacation", "Celebration", "Exercise", "Reading",
                "Writing", "Cooking", "Meditation", "Yoga", "Music", "Shopping",
                "Parenting", "Projects", "Planning", "Reflection"
            };

            return tags.Select((tag, index) => new Tag
            {
                Id = index + 1,
                Name = tag,
                IsPreDefined = true,
                Color = "#ae866c"
            }).ToList();
        }
    }
}