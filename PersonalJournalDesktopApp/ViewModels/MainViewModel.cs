using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PersonalJournalDesktopApp.Models;
using PersonalJournalDesktopApp.Services;
using System.Collections.ObjectModel;

namespace PersonalJournalDesktopApp.ViewModels
{
    public partial class MainViewModel: ObservableObject
    {
        private readonly JournalService _journalService;
        private readonly ThemeService _themeService;

        // CALENDAR NAVIGATION - Selected date
        [ObservableProperty]
        private DateTime selectedDate = DateTime.Today;

        // CALENDAR NAVIGATION - Does entry exist for selected date?
        [ObservableProperty]
        private bool hasEntryForSelectedDate;

        // Display formatted date
        [ObservableProperty]
        private string selectedDateDisplay = string.Empty;

        // Recent entries list
        [ObservableProperty]
        private ObservableCollection<JournalEntry> recentEntries = new();

        // THEME CUSTOMIZATION - Current theme
        [ObservableProperty]
        private Models.AppTheme currentTheme;

        public MainViewModel(JournalService journalService, ThemeService themeService)
        {
            _journalService = journalService;
            _themeService = themeService;
            currentTheme = new Models.AppTheme { Mode = ThemeMode.Light };
        }

        // Initialize on page load
        public async Task InitializeAsync()
        {
            await LoadThemeAsync();
            await LoadSelectedDateInfoAsync();
            await LoadRecentEntriesAsync();
        }

        // THEME CUSTOMIZATION - Load saved theme
        private async Task LoadThemeAsync()
        {
            CurrentTheme = await _themeService.GetCurrentThemeAsync();
            _themeService.ApplyTheme(CurrentTheme);
        }

        // THEME CUSTOMIZATION - Toggle light/dark mode
        [RelayCommand]
        private async Task ToggleTheme()
        {
            await _themeService.ToggleThemeAsync();
            await LoadThemeAsync();
        }

        // CALENDAR NAVIGATION - React to date change
        partial void OnSelectedDateChanged(DateTime value)
        {
            _ = LoadSelectedDateInfoAsync();
        }

        // CALENDAR NAVIGATION - Check if entry exists
        private async Task LoadSelectedDateInfoAsync()
        {
            HasEntryForSelectedDate = await _journalService.HasEntryForDateAsync(SelectedDate);
            SelectedDateDisplay = SelectedDate.ToString("dddd, MMMM dd, yyyy");
        }

        // Load recent entries for display
        private async Task LoadRecentEntriesAsync()
        {
            var entries = await _journalService.GetAllEntriesAsync();
            RecentEntries = new ObservableCollection<JournalEntry>(entries.Take(10));
        }

        // JOURNAL ENTRY MANAGEMENT - Navigate to create/edit
        [RelayCommand]
        private async Task CreateOrEditEntry()
        {
            var entry = await _journalService.GetEntryByDateAsync(SelectedDate);

            var entryDetailPage = MauiProgram.Services.GetRequiredService<Views.EntryDetailPage>();
            var viewModel = entryDetailPage.BindingContext as EntryDetailViewModel;

            if (viewModel != null)
            {
                viewModel.SelectedDate = SelectedDate;
                viewModel.Entry = entry ?? new JournalEntry { Date = SelectedDate };
                await viewModel.InitializeAsync(); // Add this method to EntryDetailViewModel
            }

            await Application.Current.MainPage.Navigation.PushAsync(entryDetailPage);
        }

        // View existing entry from list
        [RelayCommand]
        private async Task ViewEntry(JournalEntry entry)
        {
            var entryDetailPage = MauiProgram.Services.GetRequiredService<Views.EntryDetailPage>();
            var viewModel = entryDetailPage.BindingContext as EntryDetailViewModel;

            if (viewModel != null)
            {
                viewModel.SelectedDate = entry.Date;
                viewModel.Entry = entry;
                await viewModel.InitializeAsync();
            }

            await Application.Current.MainPage.Navigation.PushAsync(entryDetailPage);
        }

        // Refresh data
        [RelayCommand]
        private async Task Refresh()
        {
            await LoadSelectedDateInfoAsync();
            await LoadRecentEntriesAsync();
        }
    }
}
