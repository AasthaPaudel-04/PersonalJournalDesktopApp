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
    public partial class EntryDetailViewModel: ObservableObject
    {
        private readonly JournalService _journalService;
        private readonly MoodService _moodService;

        // JOURNAL ENTRY MANAGEMENT - Current entry
        [ObservableProperty]
        private JournalEntry entry = new();

        [ObservableProperty]
        private DateTime selectedDate;

        // Entry fields
        [ObservableProperty]
        private string title = string.Empty;

        [ObservableProperty]
        private string content = string.Empty;

        // MOOD TRACKING - All available moods
        [ObservableProperty]
        private ObservableCollection<Mood> allMoods = new();

        // MOOD TRACKING - Selected moods
        [ObservableProperty]
        private Mood? selectedPrimaryMood;

        [ObservableProperty]
        private Mood? selectedSecondaryMood1;

        [ObservableProperty]
        private Mood? selectedSecondaryMood2;

        // UI state
        [ObservableProperty]
        private bool isNewEntry = true;

        [ObservableProperty]
        private string pageTitle = "New Entry";

        public EntryDetailViewModel(JournalService journalService, MoodService moodService)
        {
            _journalService = journalService;
            _moodService = moodService;
        }

        // Receive navigation parameters
        

        public async Task InitializeAsync()
        {
            IsNewEntry = Entry.Id == 0;
            PageTitle = IsNewEntry
                ? $"New Entry - {SelectedDate:MMM dd, yyyy}"
                : $"Edit Entry - {SelectedDate:MMM dd, yyyy}";

            Title = Entry.Title;
            Content = Entry.Content;

            await LoadMoodsAsync();
        }

        // MOOD TRACKING - Load all moods and set selected ones
        private async Task LoadMoodsAsync()
        {
            var moods = await _moodService.GetAllMoodsAsync();
            AllMoods = new ObservableCollection<Mood>(moods);

            // Load previously selected moods
            if (Entry.PrimaryMoodId.HasValue)
                SelectedPrimaryMood = AllMoods.FirstOrDefault(m => m.Id == Entry.PrimaryMoodId);

            if (Entry.SecondaryMood1Id.HasValue)
                SelectedSecondaryMood1 = AllMoods.FirstOrDefault(m => m.Id == Entry.SecondaryMood1Id);

            if (Entry.SecondaryMood2Id.HasValue)
                SelectedSecondaryMood2 = AllMoods.FirstOrDefault(m => m.Id == Entry.SecondaryMood2Id);
        }

        // JOURNAL ENTRY MANAGEMENT - Save (Create/Update)
        [RelayCommand]
        private async Task SaveEntry()
        {
            if (string.IsNullOrWhiteSpace(Title))
            {
                await Application.Current.MainPage.DisplayAlert("Validation Error", "Title is required", "OK");
                return;
            }

            Entry.Date = SelectedDate;
            Entry.Title = Title;
            Entry.Content = Content;
            Entry.PrimaryMoodId = SelectedPrimaryMood?.Id;
            Entry.SecondaryMood1Id = SelectedSecondaryMood1?.Id;
            Entry.SecondaryMood2Id = SelectedSecondaryMood2?.Id;

            await _journalService.SaveEntryAsync(Entry);

            await Application.Current.MainPage.DisplayAlert("Success", "Entry saved successfully!", "OK");
            await Application.Current.MainPage.Navigation.PopAsync();
        }

        [RelayCommand]
        private async Task Cancel()
        {
            await Application.Current.MainPage.Navigation.PopAsync();
        }

        [RelayCommand]
        private async Task DeleteEntry()
        {
            if (IsNewEntry)
            {
                await Application.Current.MainPage.Navigation.PopAsync();
                return;
            }

            var confirm = await Application.Current.MainPage.DisplayAlert(
                "Confirm Delete",
                "Are you sure you want to delete this entry?",
                "Yes",
                "No");

            if (confirm)
            {
                await _journalService.DeleteEntryAsync(Entry.Id);
                await Application.Current.MainPage.DisplayAlert("Success", "Entry deleted successfully!", "OK");
                await Application.Current.MainPage.Navigation.PopAsync();
            }
        }
    }
}
