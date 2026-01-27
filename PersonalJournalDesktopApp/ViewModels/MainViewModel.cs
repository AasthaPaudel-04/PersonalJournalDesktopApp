using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PersonalJournalDesktopApp.Models;
using PersonalJournalDesktopApp.Services;

namespace PersonalJournalDesktopApp.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly JournalService _journalService;
        private readonly ThemeService _themeService;

        [ObservableProperty]
        private DateTime selectedDate = DateTime.Today;

        [ObservableProperty]
        private string selectedDateDisplay = string.Empty;

        [ObservableProperty]
        private bool hasEntryForSelectedDate;

        [ObservableProperty]
        private ObservableCollection<JournalEntry> recentEntries = new();

        public MainViewModel(JournalService journalService, ThemeService themeService)
        {
            _journalService = journalService;
            _themeService = themeService;
            UpdateSelectedDateDisplay();
        }

        public async Task InitializeAsync()
        {
            await LoadRecentEntriesAsync();
            await CheckEntryExistsAsync();
        }

        partial void OnSelectedDateChanged(DateTime value)
        {
            UpdateSelectedDateDisplay();
            _ = CheckEntryExistsAsync();
        }

        private void UpdateSelectedDateDisplay()
        {
            SelectedDateDisplay = SelectedDate.ToString("dddd, MMMM dd, yyyy");
        }

        private async Task CheckEntryExistsAsync()
        {
            HasEntryForSelectedDate = await _journalService.HasEntryForDateAsync(SelectedDate);
        }

        [RelayCommand]
        private async Task CreateOrEditEntryAsync()
        {
            var navigationParameter = new Dictionary<string, object>
            {
                { "Date", SelectedDate }
            };

            await Shell.Current.GoToAsync("EntryDetailPage", navigationParameter);
        }

        [RelayCommand]
        private async Task ViewEntryAsync(JournalEntry entry)
        {
            var navigationParameter = new Dictionary<string, object>
            {
                { "Date", entry.Date }
            };

            await Shell.Current.GoToAsync("EntryDetailPage", navigationParameter);
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            await LoadRecentEntriesAsync();
            await CheckEntryExistsAsync();
        }

        private async Task LoadRecentEntriesAsync()
        {
            var entries = await _journalService.GetAllEntriesAsync();
            RecentEntries = new ObservableCollection<JournalEntry>(entries);
        }

        [RelayCommand]
        private async Task ToggleThemeAsync()
        {
            await _themeService.ToggleThemeAsync();
            var theme = await _themeService.GetCurrentThemeAsync();
            _themeService.ApplyTheme(theme);
        }
    }
}