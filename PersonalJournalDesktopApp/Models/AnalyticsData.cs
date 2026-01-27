using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalJournalDesktopApp.Models
{
    public class AnalyticsData
    {
        // Streak data
        public int CurrentStreak { get; set; }
        public int LongestStreak { get; set; }
        public int TotalEntries { get; set; }
        public int MissedDays { get; set; }

        // Mood analytics
        public Dictionary<MoodCategory, int> MoodDistribution { get; set; } = new();
        public Mood? MostFrequentMood { get; set; }
        public double PositivePercentage { get; set; }
        public double NeutralPercentage { get; set; }
        public double NegativePercentage { get; set; }

        // Tag analytics
        public List<TagStatistic> MostUsedTags { get; set; } = new();
        public Dictionary<string, int> TagBreakdown { get; set; } = new();

        // Word count
        public double AverageWordCount { get; set; }
        public List<WordCountTrend> WordCountTrends { get; set; } = new();
    }

    public class TagStatistic
    {
        public string TagName { get; set; } = string.Empty;
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    public class WordCountTrend
    {
        public DateTime Date { get; set; }
        public int WordCount { get; set; }
    }
}