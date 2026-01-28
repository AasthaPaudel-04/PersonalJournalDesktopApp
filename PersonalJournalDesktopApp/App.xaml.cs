using PersonalJournalDesktopApp.Services;
using PersonalJournalDesktopApp.Views;

namespace PersonalJournalDesktopApp;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        // Check security and set main page
        CheckSecurityAndNavigate();
    }

    private async void CheckSecurityAndNavigate()
    {
        try
        {
            // Get SecurityService from DI container
            var securityService = MauiProgram.Services?.GetService<SecurityService>();

            if (securityService != null)
            {
                var isSecurityEnabled = await securityService.IsSecurityEnabledAsync();

                if (isSecurityEnabled)
                {
                    // Security is enabled - show login page
                    var loginPage = MauiProgram.Services?.GetService<LoginPage>();
                    if (loginPage != null)
                    {
                        MainPage = new NavigationPage(loginPage);
                        return;
                    }
                }
            }

            // No security or couldn't load - show main app
            MainPage = new AppShell();

            // Apply saved theme
            var themeService = MauiProgram.Services?.GetService<ThemeService>();
            if (themeService != null)
            {
                var theme = await themeService.GetCurrentThemeAsync();
                themeService.ApplyTheme(theme);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error checking security: {ex.Message}");
            // Fallback to main app
            MainPage = new AppShell();
        }
    }
}