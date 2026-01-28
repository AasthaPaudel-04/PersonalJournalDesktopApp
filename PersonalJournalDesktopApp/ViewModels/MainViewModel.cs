using System;
using System.Collections.ObjectModel;
using System.Linq;
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
        private const int PageSize = 5;

        [ObservableProperty]
        private DateTime selectedDate = DateTime.Today;

        [ObservableProperty]
        private string selectedDateDisplay = string.Empty;

        [ObservableProperty]
        private bool hasEntryForSelectedDate;

        [ObservableProperty]
        private ObservableCollection<JournalEntry> recentEntries = new();

        // Pagination properties
        [ObservableProperty]
        private int currentPage = 1;

        [ObservableProperty]
        private int totalPages = 1;

        [ObservableProperty]
        private bool hasPreviousPage;

        [ObservableProperty]
        private bool hasNextPage;

        [ObservableProperty]
        private string pageInfo = "Page 1 of 1";

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
            CurrentPage = 1;
            await LoadRecentEntriesAsync();
            await CheckEntryExistsAsync();
        }

        // PAGINATION: Load entries for current page
        private async Task LoadRecentEntriesAsync()
        {
            var allEntries = await _journalService.GetAllEntriesAsync();

            // Calculate total pages
            TotalPages = allEntries.Count == 0 ? 1 : (int)Math.Ceiling(allEntries.Count / (double)PageSize);

            // Ensure current page is valid
            if (CurrentPage > TotalPages)
                CurrentPage = TotalPages;
            if (CurrentPage < 1)
                CurrentPage = 1;

            // Get entries for current page
            var skip = (CurrentPage - 1) * PageSize;
            var pagedEntries = allEntries.Skip(skip).Take(PageSize).ToList();

            RecentEntries = new ObservableCollection<JournalEntry>(pagedEntries);

            // Update pagination state
            UpdatePaginationState();
        }

        private void UpdatePaginationState()
        {
            HasPreviousPage = CurrentPage > 1;
            HasNextPage = CurrentPage < TotalPages;
            PageInfo = $"Page {CurrentPage} of {TotalPages}";
        }

        [RelayCommand]
        private async Task NextPageAsync()
        {
            if (CurrentPage < TotalPages)
            {
                CurrentPage++;
                await LoadRecentEntriesAsync();
            }
        }

        [RelayCommand]
        private async Task PreviousPageAsync()
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
                await LoadRecentEntriesAsync();
            }
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