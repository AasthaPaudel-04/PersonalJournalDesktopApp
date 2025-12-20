using PersonalJournalDesktopApp.ViewModels;

namespace PersonalJournalDesktopApp.Views;

public partial class EntryDetailPage : ContentPage
{
    public EntryDetailPage(EntryDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}