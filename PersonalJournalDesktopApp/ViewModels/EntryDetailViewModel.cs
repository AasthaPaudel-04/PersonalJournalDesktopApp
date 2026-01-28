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
    public partial class EntryDetailViewModel : ObservableObject
    {
        private readonly JournalService _journalService;
        private readonly MoodService _moodService;
        private readonly TagService _tagService;
        private readonly CategoryService _categoryService;

        public event EventHandler? SaveRequested;

        [ObservableProperty]
        private string title = string.Empty;

        [ObservableProperty]
        private string content = string.Empty;

        [ObservableProperty]
        private DateTime selectedDate;

        [ObservableProperty]
        private Mood? selectedPrimaryMood;

        [ObservableProperty]
        private Mood? selectedSecondaryMood1;

        [ObservableProperty]
        private Mood? selectedSecondaryMood2;

        [ObservableProperty]
        private Category? selectedCategory;

        [ObservableProperty]
        private ObservableCollection<Mood> allMoods = new();

        [ObservableProperty]
        private ObservableCollection<Category> allCategories = new();

        [ObservableProperty]
        private ObservableCollection<Tag> allTags = new();

        [ObservableProperty]
        private ObservableCollection<Tag> selectedTags = new();

        [ObservableProperty]
        private string newTagName = string.Empty;

        [ObservableProperty]
        private bool isNewEntry;

        [ObservableProperty]
        private string pageTitle = "New Entry";

        private int _entryId;

        public EntryDetailViewModel(
            JournalService journalService,
            MoodService moodService,
            TagService tagService,
            CategoryService categoryService)
        {
            _journalService = journalService;
            _moodService = moodService;
            _tagService = tagService;
            _categoryService = categoryService;
        }

        public async Task InitializeAsync(DateTime date)
        {
            SelectedDate = date;

            // Load moods FIRST
            var moods = await _moodService.GetAllMoodsAsync();
            AllMoods = new ObservableCollection<Mood>(moods);

            // Load categories FIRST
            var categories = await _categoryService.GetAllCategoriesAsync();
            AllCategories = new ObservableCollection<Category>(categories);

            // Load tags
            var tags = await _tagService.GetAllTagsAsync();
            AllTags = new ObservableCollection<Tag>(tags);

            // Check if entry exists for this date
            var entry = await _journalService.GetEntryByDateAsync(date);

            if (entry != null)
            {
                _entryId = entry.Id;
                Title = entry.Title;
                Content = entry.Content;
                IsNewEntry = false;
                PageTitle = $"Edit Entry - {date:MMM dd, yyyy}";

                // FIXED: Use the SAME objects from AllMoods collection
                // This ensures Picker recognizes them
                if (entry.PrimaryMoodId.HasValue)
                {
                    SelectedPrimaryMood = AllMoods.FirstOrDefault(m => m.Id == entry.PrimaryMoodId.Value);
                    System.Diagnostics.Debug.WriteLine($"Loading Primary Mood: {SelectedPrimaryMood?.Name ?? "NULL"}");
                }

                if (entry.SecondaryMood1Id.HasValue)
                {
                    SelectedSecondaryMood1 = AllMoods.FirstOrDefault(m => m.Id == entry.SecondaryMood1Id.Value);
                    System.Diagnostics.Debug.WriteLine($"Loading Secondary Mood 1: {SelectedSecondaryMood1?.Name ?? "NULL"}");
                }

                if (entry.SecondaryMood2Id.HasValue)
                {
                    SelectedSecondaryMood2 = AllMoods.FirstOrDefault(m => m.Id == entry.SecondaryMood2Id.Value);
                    System.Diagnostics.Debug.WriteLine($"Loading Secondary Mood 2: {SelectedSecondaryMood2?.Name ?? "NULL"}");
                }

                // FIXED: Use the SAME objects from AllCategories collection
                if (entry.CategoryId.HasValue)
                {
                    SelectedCategory = AllCategories.FirstOrDefault(c => c.Id == entry.CategoryId.Value);
                    System.Diagnostics.Debug.WriteLine($"Loading Category: {SelectedCategory?.Name ?? "NULL"}");
                }

                // Load entry tags
                SelectedTags = new ObservableCollection<Tag>(entry.Tags);

                System.Diagnostics.Debug.WriteLine($"=== LOADED ENTRY ===");
                System.Diagnostics.Debug.WriteLine($"Entry ID: {entry.Id}");
                System.Diagnostics.Debug.WriteLine($"PrimaryMoodId in DB: {entry.PrimaryMoodId}");
                System.Diagnostics.Debug.WriteLine($"CategoryId in DB: {entry.CategoryId}");
                System.Diagnostics.Debug.WriteLine($"Selected Primary Mood: {SelectedPrimaryMood?.Id} - {SelectedPrimaryMood?.Name}");
                System.Diagnostics.Debug.WriteLine($"Selected Category: {SelectedCategory?.Id} - {SelectedCategory?.Name}");
                System.Diagnostics.Debug.WriteLine("====================");
            }
            else
            {
                _entryId = 0;
                IsNewEntry = true;
                PageTitle = $"New Entry - {date:MMM dd, yyyy}";
            }
        }

        [RelayCommand]
        private async Task SaveEntryAsync()
        {
            if (string.IsNullOrWhiteSpace(Title))
            {
                await Application.Current!.MainPage!.DisplayAlert("Error", "Please enter a title", "OK");
                return;
            }

            // Raise event so page can get WebView content first
            SaveRequested?.Invoke(this, EventArgs.Empty);

            // Small delay to ensure content is retrieved
            await Task.Delay(100);

            // Debug output to verify what we're about to save
            System.Diagnostics.Debug.WriteLine($"=== SAVING ENTRY ===");
            System.Diagnostics.Debug.WriteLine($"Title: {Title}");
            System.Diagnostics.Debug.WriteLine($"Content Length: {Content?.Length ?? 0}");
            System.Diagnostics.Debug.WriteLine($"Primary Mood Selected: {SelectedPrimaryMood?.Name ?? "NULL"} (ID: {SelectedPrimaryMood?.Id})");
            System.Diagnostics.Debug.WriteLine($"Secondary Mood 1: {SelectedSecondaryMood1?.Name ?? "NULL"} (ID: {SelectedSecondaryMood1?.Id})");
            System.Diagnostics.Debug.WriteLine($"Secondary Mood 2: {SelectedSecondaryMood2?.Name ?? "NULL"} (ID: {SelectedSecondaryMood2?.Id})");
            System.Diagnostics.Debug.WriteLine($"Category Selected: {SelectedCategory?.Name ?? "NULL"} (ID: {SelectedCategory?.Id})");
            System.Diagnostics.Debug.WriteLine($"Tags Count: {SelectedTags.Count}");

            var entry = new JournalEntry
            {
                Id = _entryId,
                Date = SelectedDate,
                Title = Title,
                Content = Content ?? string.Empty,
                PrimaryMoodId = SelectedPrimaryMood?.Id,
                SecondaryMood1Id = SelectedSecondaryMood1?.Id,
                SecondaryMood2Id = SelectedSecondaryMood2?.Id,
                CategoryId = SelectedCategory?.Id
            };

            System.Diagnostics.Debug.WriteLine($"Entry object - PrimaryMoodId: {entry.PrimaryMoodId}, CategoryId: {entry.CategoryId}");

            var savedEntryId = await _journalService.SaveEntryAsync(entry);

            System.Diagnostics.Debug.WriteLine($"Entry saved with ID: {savedEntryId}");

            // Save tags
            var tagIds = SelectedTags.Select(t => t.Id).ToList();
            await _tagService.SaveEntryTagsAsync(savedEntryId, tagIds);

            System.Diagnostics.Debug.WriteLine($"Tags saved: {string.Join(", ", SelectedTags.Select(t => t.Name))}");
            System.Diagnostics.Debug.WriteLine("===================");

            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        private async Task DeleteEntryAsync()
        {
            var confirm = await Application.Current!.MainPage!.DisplayAlert(
                "Confirm Delete",
                "Are you sure you want to delete this entry?",
                "Delete",
                "Cancel");

            if (confirm && _entryId > 0)
            {
                await _journalService.DeleteEntryAsync(_entryId);
                await Shell.Current.GoToAsync("..");
            }
        }

        [RelayCommand]
        private async Task CancelAsync()
        {
            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        private void ToggleTag(Tag tag)
        {
            if (SelectedTags.Contains(tag))
            {
                SelectedTags.Remove(tag);
            }
            else
            {
                SelectedTags.Add(tag);
            }
        }

        [RelayCommand]
        private async Task AddNewTagAsync()
        {
            if (string.IsNullOrWhiteSpace(NewTagName))
                return;

            // Check if tag already exists
            var existingTag = AllTags.FirstOrDefault(t =>
                t.Name.Equals(NewTagName, StringComparison.OrdinalIgnoreCase));

            if (existingTag != null)
            {
                // Add existing tag to selected
                if (!SelectedTags.Contains(existingTag))
                    SelectedTags.Add(existingTag);
            }
            else
            {
                // Create new tag
                var tagId = await _tagService.CreateTagAsync(NewTagName);
                var newTag = new Tag
                {
                    Id = tagId,
                    Name = NewTagName,
                    IsPreDefined = false,
                    Color = "#ae866c"
                };

                AllTags.Add(newTag);
                SelectedTags.Add(newTag);
            }

            NewTagName = string.Empty;
        }
    }
}