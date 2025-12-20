using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalJournalDesktopApp.Models
{
    public class JournalEntry
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int? PrimaryMoodId { get; set; }
        public int? SecondaryMood1Id { get; set; }
        public int? SecondaryMood2Id { get; set; }

        // Navigation properties
        public Mood? PrimaryMood { get; set; }
        public Mood? SecondaryMood1 { get; set; }
        public Mood? SecondaryMood2 { get; set; }
    }
}
