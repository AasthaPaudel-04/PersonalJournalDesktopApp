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
    public partial class SearchViewModel : ObservableObject
    {
        private readonly SearchService _searchService;
        private readonly MoodService _moodService;
        private readonly TagService _tagService;
        private readonly CategoryService _categoryService;
        private readonly JournalService _journalService;

        [ObservableProperty]
        private string searchText = string.Empty;

        [ObservableProperty]
        private DateTime? startDate;

        [ObservableProperty]
        private DateTime? endDate;

        [ObservableProperty]
        private ObservableCollection<JournalEntry> searchResults = new();

        [ObservableProperty]
        private ObservableCollection<Mood> allMoods = new();

        [ObservableProperty]
        private ObservableCollection<Mood> selectedMoods = new();

        [ObservableProperty]
        private ObservableCollection<Tag> allTags = new();

        [ObservableProperty]
        private ObservableCollection<Tag> selectedTags = new();

        [ObservableProperty]
        private ObservableCollection<Category> allCategories = new();

        [ObservableProperty]
        private Category? selectedCategory;

        [ObservableProperty]
        private bool isSearching = false;

        [ObservableProperty]
        private int resultCount = 0;

        public SearchViewModel(
            SearchService searchService,
            MoodService moodService,
            TagService tagService,
            CategoryService categoryService,
            JournalService journalService)
        {
            _searchService = searchService;
            _moodService = moodService;
            _tagService = tagService;
            _categoryService = categoryService;
            _journalService = journalService;
        }

        public async Task InitializeAsync()
        {
            // Load moods
            var moods = await _moodService.GetAllMoodsAsync();
            AllMoods = new ObservableCollection<Mood>(moods);

            // Load tags
            var tags = await _tagService.GetAllTagsAsync();
            AllTags = new ObservableCollection<Tag>(tags);

            // Load categories
            var categories = await _categoryService.GetAllCategoriesAsync();
            AllCategories = new ObservableCollection<Category>(categories);

            // Show all entries initially
            await SearchAsync();
        }

        [RelayCommand]
        private async Task SearchAsync()
        {
            IsSearching = true;

            try
            {
                var moodIds = SelectedMoods.Select(m => m.Id).ToList();
                var tagIds = SelectedTags.Select(t => t.Id).ToList();

                var results = await _searchService.SearchAsync(
                    string.IsNullOrWhiteSpace(SearchText) ? null : SearchText,
                    StartDate,
                    EndDate,
                    moodIds.Any() ? moodIds : null,
                    tagIds.Any() ? tagIds : null,
                    SelectedCategory?.Id
                );

                SearchResults = new ObservableCollection<JournalEntry>(results);
                ResultCount = results.Count;
            }
            finally
            {
                IsSearching = false;
            }
        }

        [RelayCommand]
        private async Task ClearFiltersAsync()
        {
            SearchText = string.Empty;
            StartDate = null;
            EndDate = null;
            SelectedMoods.Clear();
            SelectedTags.Clear();
            SelectedCategory = null;

            await SearchAsync();
        }

        [RelayCommand]
        private void ToggleMood(Mood mood)
        {
            if (SelectedMoods.Contains(mood))
                SelectedMoods.Remove(mood);
            else
                SelectedMoods.Add(mood);
        }

        [RelayCommand]
        private void ToggleTag(Tag tag)
        {
            if (SelectedTags.Contains(tag))
                SelectedTags.Remove(tag);
            else
                SelectedTags.Add(tag);
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
    }
}