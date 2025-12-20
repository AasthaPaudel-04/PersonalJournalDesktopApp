using PersonalJournalDesktopApp.Views;
namespace PersonalJournalDesktopApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new NavigationPage(new MainPage(
                MauiProgram.Services.GetRequiredService<ViewModels.MainViewModel>()));
        }
    }
}
