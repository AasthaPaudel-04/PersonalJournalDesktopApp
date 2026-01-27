using PersonalJournalDesktopApp.ViewModels;

namespace PersonalJournalDesktopApp.Views
{
    [QueryProperty(nameof(Date), "Date")]
    public partial class EntryDetailPage : ContentPage
    {
        private readonly EntryDetailViewModel _viewModel;
        private DateTime _date;

        public DateTime Date
        {
            get => _date;
            set
            {
                _date = value;
                if (_viewModel != null)
                {
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        await _viewModel.InitializeAsync(value);
                    });
                }
            }
        }

        public EntryDetailPage(EntryDetailViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }
    }
}