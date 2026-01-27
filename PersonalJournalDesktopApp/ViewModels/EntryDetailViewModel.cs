using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
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

            // Load moods
            var moods = await _moodService.GetAllMoodsAsync();
            AllMoods = new ObservableCollection<Mood>(moods);

            // Load categories
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

                // Set moods
                if (entry.PrimaryMoodId.HasValue)
                    SelectedPrimaryMood = AllMoods.FirstOrDefault(m => m.Id == entry.PrimaryMoodId.Value);

                if (entry.SecondaryMood1Id.HasValue)
                    SelectedSecondaryMood1 = AllMoods.FirstOrDefault(m => m.Id == entry.SecondaryMood1Id.Value);

                if (entry.SecondaryMood2Id.HasValue)
                    SelectedSecondaryMood2 = AllMoods.FirstOrDefault(m => m.Id == entry.SecondaryMood2Id.Value);

                // Set category
                if (entry.CategoryId.HasValue)
                    SelectedCategory = AllCategories.FirstOrDefault(c => c.Id == entry.CategoryId.Value);

                // Load entry tags
                var entryTags = await _tagService.GetTagsForEntryAsync(entry.Id);
                SelectedTags = new ObservableCollection<Tag>(entryTags);
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

            var entry = new JournalEntry
            {
                Id = _entryId,
                Date = SelectedDate,
                Title = Title,
                Content = Content,
                PrimaryMoodId = SelectedPrimaryMood?.Id,
                SecondaryMood1Id = SelectedSecondaryMood1?.Id,
                SecondaryMood2Id = SelectedSecondaryMood2?.Id,
                CategoryId = SelectedCategory?.Id
            };

            var savedEntryId = await _journalService.SaveEntryAsync(entry);

            // Save tags
            var tagIds = SelectedTags.Select(t => t.Id).ToList();
            await _tagService.SaveEntryTagsAsync(savedEntryId, tagIds);

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