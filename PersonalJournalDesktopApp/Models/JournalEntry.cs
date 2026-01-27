using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

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
        public int? CategoryId { get; set; }

        // Navigation properties
        public Mood? PrimaryMood { get; set; }
        public Mood? SecondaryMood1 { get; set; }
        public Mood? SecondaryMood2 { get; set; }
        public Category? Category { get; set; }
        public List<Tag> Tags { get; set; } = new();

        // Computed property for word count
        public int WordCount => string.IsNullOrWhiteSpace(Content)
            ? 0
            : Content.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length;
    }
}