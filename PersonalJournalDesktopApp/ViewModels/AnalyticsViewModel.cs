using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PersonalJournalDesktopApp.Models;
using PersonalJournalDesktopApp.Services;

namespace PersonalJournalDesktopApp.ViewModels
{
    public partial class AnalyticsViewModel : ObservableObject
    {
        private readonly AnalyticsService _analyticsService;
        private readonly ExportService _exportService;
        private readonly JournalService _journalService;

        [ObservableProperty]
        private AnalyticsData? analyticsData;

        [ObservableProperty]
        private DateTime? filterStartDate;

        [ObservableProperty]
        private DateTime? filterEndDate;

        [ObservableProperty]
        private bool isLoading = false;

        // Quick stats
        [ObservableProperty]
        private int currentStreak;

        [ObservableProperty]
        private int longestStreak;

        [ObservableProperty]
        private int totalEntries;

        [ObservableProperty]
        private int missedDays;

        // Mood stats
        [ObservableProperty]
        private string? mostFrequentMoodDisplay;

        [ObservableProperty]
        private double positivePercentage;

        [ObservableProperty]
        private double neutralPercentage;

        [ObservableProperty]
        private double negativePercentage;

        // Tag stats
        [ObservableProperty]
        private ObservableCollection<TagStatistic> topTags = new();

        [ObservableProperty]
        private double averageWordCount;

        public AnalyticsViewModel(
            AnalyticsService analyticsService,
            ExportService exportService,
            JournalService journalService)
        {
            _analyticsService = analyticsService;
            _exportService = exportService;
            _journalService = journalService;
        }

        public async Task InitializeAsync()
        {
            await LoadAnalyticsAsync();
        }

        [RelayCommand]
        private async Task LoadAnalyticsAsync()
        {
            IsLoading = true;

            try
            {
                AnalyticsData = await _analyticsService.GetAnalyticsAsync(FilterStartDate, FilterEndDate);

                // Update properties
                CurrentStreak = AnalyticsData.CurrentStreak;
                LongestStreak = AnalyticsData.LongestStreak;
                TotalEntries = AnalyticsData.TotalEntries;
                MissedDays = AnalyticsData.MissedDays;

                PositivePercentage = AnalyticsData.PositivePercentage;
                NeutralPercentage = AnalyticsData.NeutralPercentage;
                NegativePercentage = AnalyticsData.NegativePercentage;

                MostFrequentMoodDisplay = AnalyticsData.MostFrequentMood != null
                    ? $"{AnalyticsData.MostFrequentMood.Emoji} {AnalyticsData.MostFrequentMood.Name}"
                    : "No mood data";

                TopTags = new ObservableCollection<TagStatistic>(AnalyticsData.MostUsedTags);

                AverageWordCount = AnalyticsData.AverageWordCount;
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task ExportToPdfAsync()
        {
            try
            {
                var entries = await _journalService.GetAllEntriesAsync();

                // Filter by date range if set
                if (FilterStartDate.HasValue)
                    entries = entries.Where(e => e.Date >= FilterStartDate.Value).ToList();

                if (FilterEndDate.HasValue)
                    entries = entries.Where(e => e.Date <= FilterEndDate.Value).ToList();

                // Load full entry details
                var fullEntries = new List<JournalEntry>();
                foreach (var entry in entries)
                {
                    var fullEntry = await _journalService.GetEntryByDateAsync(entry.Date);
                    if (fullEntry != null)
                        fullEntries.Add(fullEntry);
                }

                var fileName = $"Journal_Export_{DateTime.Now:yyyyMMdd_HHmmss}";
                var filePath = await _exportService.ExportToPdfAsync(fullEntries, fileName);

                await Application.Current!.MainPage!.DisplayAlert(
                    "Export Successful",
                    $"Journal exported to:\n{filePath}\n\nYou can print this HTML file to PDF.",
                    "OK");

                // Open the file
                await Launcher.OpenAsync(new OpenFileRequest
                {
                    File = new ReadOnlyFile(filePath)
                });
            }
            catch (Exception ex)
            {
                await Application.Current!.MainPage!.DisplayAlert(
                    "Export Failed",
                    $"Error: {ex.Message}",
                    "OK");
            }
        }

        [RelayCommand]
        private async Task ClearDateFilterAsync()
        {
            FilterStartDate = null;
            FilterEndDate = null;
            await LoadAnalyticsAsync();
        }

        [RelayCommand]
        private async Task SetLast30DaysAsync()
        {
            FilterStartDate = DateTime.Today.AddDays(-30);
            FilterEndDate = DateTime.Today;
            await LoadAnalyticsAsync();
        }

        [RelayCommand]
        private async Task SetLast90DaysAsync()
        {
            FilterStartDate = DateTime.Today.AddDays(-90);
            FilterEndDate = DateTime.Today;
            await LoadAnalyticsAsync();
        }

        [RelayCommand]
        private async Task SetThisYearAsync()
        {
            FilterStartDate = new DateTime(DateTime.Now.Year, 1, 1);
            FilterEndDate = DateTime.Today;
            await LoadAnalyticsAsync();
        }
    }
}