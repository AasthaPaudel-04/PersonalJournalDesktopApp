using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PersonalJournalDesktopApp.Data;
using PersonalJournalDesktopApp.Models;

namespace PersonalJournalDesktopApp.Services
{
    public class AnalyticsService
    {
        private readonly DatabaseService _database;
        private readonly JournalService _journalService;

        public AnalyticsService(DatabaseService database, JournalService journalService)
        {
            _database = database;
            _journalService = journalService;
        }

        public async Task<AnalyticsData> GetAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var allEntries = await _journalService.GetAllEntriesAsync();

            // Filter by date range if provided
            if (startDate.HasValue)
                allEntries = allEntries.Where(e => e.Date >= startDate.Value).ToList();

            if (endDate.HasValue)
                allEntries = allEntries.Where(e => e.Date <= endDate.Value).ToList();

            var analytics = new AnalyticsData
            {
                TotalEntries = allEntries.Count
            };

            // Calculate streaks
            CalculateStreaks(allEntries, analytics);

            // Calculate mood analytics
            await CalculateMoodAnalyticsAsync(allEntries, analytics);

            // Calculate tag analytics
            await CalculateTagAnalyticsAsync(allEntries, analytics);

            // Calculate word count analytics
            CalculateWordCountAnalytics(allEntries, analytics);

            return analytics;
        }

        private void CalculateStreaks(List<JournalEntry> entries, AnalyticsData analytics)
        {
            if (!entries.Any())
            {
                analytics.CurrentStreak = 0;
                analytics.LongestStreak = 0;
                analytics.MissedDays = 0;
                return;
            }

            var sortedDates = entries.Select(e => e.Date.Date).OrderByDescending(d => d).Distinct().ToList();

            // Calculate current streak
            var today = DateTime.Today;
            var currentStreak = 0;
            var checkDate = today;

            // Start from yesterday if no entry today, or from today if there is an entry
            if (sortedDates.Contains(today))
            {
                currentStreak = 1;
                checkDate = today.AddDays(-1);
            }
            else if (sortedDates.Contains(today.AddDays(-1)))
            {
                currentStreak = 1;
                checkDate = today.AddDays(-2);
            }

            while (sortedDates.Contains(checkDate))
            {
                currentStreak++;
                checkDate = checkDate.AddDays(-1);
            }

            analytics.CurrentStreak = currentStreak;

            // Calculate longest streak
            var longestStreak = 0;
            var tempStreak = 1;

            for (int i = 0; i < sortedDates.Count - 1; i++)
            {
                if ((sortedDates[i] - sortedDates[i + 1]).Days == 1)
                {
                    tempStreak++;
                }
                else
                {
                    longestStreak = Math.Max(longestStreak, tempStreak);
                    tempStreak = 1;
                }
            }

            longestStreak = Math.Max(longestStreak, tempStreak);
            analytics.LongestStreak = longestStreak;

            // Calculate missed days (days between first entry and today with no entry)
            if (sortedDates.Any())
            {
                var firstEntryDate = sortedDates.Min();
                var totalDays = (today - firstEntryDate).Days + 1;
                analytics.MissedDays = totalDays - sortedDates.Count;
            }
        }

        private async Task CalculateMoodAnalyticsAsync(List<JournalEntry> entries, AnalyticsData analytics)
        {
            var allMoods = await _database.GetAllMoodsAsync();
            var moodCounts = new Dictionary<int, int>();

            foreach (var entry in entries)
            {
                // Load mood details for each entry
                if (entry.PrimaryMoodId.HasValue)
                {
                    entry.PrimaryMood = await _database.GetMoodByIdAsync(entry.PrimaryMoodId.Value);
                    if (entry.PrimaryMood != null)
                    {
                        if (!moodCounts.ContainsKey(entry.PrimaryMood.Id))
                            moodCounts[entry.PrimaryMood.Id] = 0;
                        moodCounts[entry.PrimaryMood.Id]++;
                    }
                }

                if (entry.SecondaryMood1Id.HasValue)
                {
                    entry.SecondaryMood1 = await _database.GetMoodByIdAsync(entry.SecondaryMood1Id.Value);
                    if (entry.SecondaryMood1 != null)
                    {
                        if (!moodCounts.ContainsKey(entry.SecondaryMood1.Id))
                            moodCounts[entry.SecondaryMood1.Id] = 0;
                        moodCounts[entry.SecondaryMood1.Id]++;
                    }
                }

                if (entry.SecondaryMood2Id.HasValue)
                {
                    entry.SecondaryMood2 = await _database.GetMoodByIdAsync(entry.SecondaryMood2Id.Value);
                    if (entry.SecondaryMood2 != null)
                    {
                        if (!moodCounts.ContainsKey(entry.SecondaryMood2.Id))
                            moodCounts[entry.SecondaryMood2.Id] = 0;
                        moodCounts[entry.SecondaryMood2.Id]++;
                    }
                }
            }

            // Mood distribution by category
            analytics.MoodDistribution = new Dictionary<MoodCategory, int>
            {
                { MoodCategory.Positive, 0 },
                { MoodCategory.Neutral, 0 },
                { MoodCategory.Negative, 0 }
            };

            foreach (var moodCount in moodCounts)
            {
                var mood = allMoods.FirstOrDefault(m => m.Id == moodCount.Key);
                if (mood != null)
                {
                    analytics.MoodDistribution[mood.Category] += moodCount.Value;
                }
            }

            var totalMoodCount = analytics.MoodDistribution.Values.Sum();
            if (totalMoodCount > 0)
            {
                analytics.PositivePercentage = (analytics.MoodDistribution[MoodCategory.Positive] * 100.0) / totalMoodCount;
                analytics.NeutralPercentage = (analytics.MoodDistribution[MoodCategory.Neutral] * 100.0) / totalMoodCount;
                analytics.NegativePercentage = (analytics.MoodDistribution[MoodCategory.Negative] * 100.0) / totalMoodCount;
            }

            // Most frequent mood
            if (moodCounts.Any())
            {
                var mostFrequentMoodId = moodCounts.OrderByDescending(m => m.Value).First().Key;
                analytics.MostFrequentMood = allMoods.FirstOrDefault(m => m.Id == mostFrequentMoodId);
            }
        }

        private async Task CalculateTagAnalyticsAsync(List<JournalEntry> entries, AnalyticsData analytics)
        {
            var tagCounts = new Dictionary<string, int>();

            foreach (var entry in entries)
            {
                var tags = await _database.GetTagsForEntryAsync(entry.Id);
                foreach (var tag in tags)
                {
                    if (!tagCounts.ContainsKey(tag.Name))
                        tagCounts[tag.Name] = 0;
                    tagCounts[tag.Name]++;
                }
            }

            var totalTags = tagCounts.Values.Sum();

            analytics.MostUsedTags = tagCounts
                .OrderByDescending(t => t.Value)
                .Take(10)
                .Select(t => new TagStatistic
                {
                    TagName = t.Key,
                    Count = t.Value,
                    Percentage = totalTags > 0 ? (t.Value * 100.0 / totalTags) : 0
                })
                .ToList();

            analytics.TagBreakdown = tagCounts;
        }

        private void CalculateWordCountAnalytics(List<JournalEntry> entries, AnalyticsData analytics)
        {
            if (entries.Any())
            {
                analytics.AverageWordCount = entries.Average(e => e.WordCount);

                analytics.WordCountTrends = entries
                    .OrderBy(e => e.Date)
                    .Select(e => new WordCountTrend
                    {
                        Date = e.Date,
                        WordCount = e.WordCount
                    })
                    .ToList();
            }
        }
    }
}