using PersonalJournalDesktopApp.ViewModels;

namespace PersonalJournalDesktopApp.Views
{
    public partial class SecurityPage : ContentPage
    {
        private readonly SecurityViewModel _viewModel;

        public SecurityPage(SecurityViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.InitializeAsync();
        }
    }
}