using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalJournalDesktopApp.Models
{
    public enum MoodCategory
    {
        Positive,
        Neutral,
        Negative
    }
    public class Mood
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public MoodCategory Category { get; set; }
        public string Emoji { get; set; } = string.Empty;

        public static List<Mood> GetDefaultMoods()
        {
            return new List<Mood>
            {
            // Positive Moods
            new Mood { Id = 1, Name = "Happy", Category = MoodCategory.Positive, Emoji = "😊" },
            new Mood { Id = 2, Name = "Excited", Category = MoodCategory.Positive, Emoji = "🤩" },
            new Mood { Id = 3, Name = "Relaxed", Category = MoodCategory.Positive, Emoji = "😌" },
            new Mood { Id = 4, Name = "Grateful", Category = MoodCategory.Positive, Emoji = "🙏" },
            new Mood { Id = 5, Name = "Confident", Category = MoodCategory.Positive, Emoji = "💪" },
            
            // Neutral Moods
            new Mood { Id = 6, Name = "Calm", Category = MoodCategory.Neutral, Emoji = "😐" },
            new Mood { Id = 7, Name = "Thoughtful", Category = MoodCategory.Neutral, Emoji = "🤔" },
            new Mood { Id = 8, Name = "Curious", Category = MoodCategory.Neutral, Emoji = "🧐" },
            new Mood { Id = 9, Name = "Nostalgic", Category = MoodCategory.Neutral, Emoji = "🌅" },
            new Mood { Id = 10, Name = "Bored", Category = MoodCategory.Neutral, Emoji = "😑" },
            
            // Negative Moods
            new Mood { Id = 11, Name = "Sad", Category = MoodCategory.Negative, Emoji = "😢" },
            new Mood { Id = 12, Name = "Angry", Category = MoodCategory.Negative, Emoji = "😠" },
            new Mood { Id = 13, Name = "Stressed", Category = MoodCategory.Negative, Emoji = "😰" },
            new Mood { Id = 14, Name = "Lonely", Category = MoodCategory.Negative, Emoji = "😔" },
            new Mood { Id = 15, Name = "Anxious", Category = MoodCategory.Negative, Emoji = "😨" }
            };
        }
    }
}
