using PersonalJournalDesktopApp.Data;
using PersonalJournalDesktopApp.Models;

namespace PersonalJournalDesktopApp.Services
{
    public class ThemeService
    {
        private readonly DatabaseService _databaseService;
        private const string ThemeKey = "AppTheme";

        public ThemeService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task<ThemeMode> GetCurrentThemeAsync()
        {
            var themeValue = await _databaseService.GetSettingAsync(ThemeKey);
            if (Enum.TryParse<ThemeMode>(themeValue, out var theme))
            {
                return theme;
            }
            return ThemeMode.Light; // Default to light theme
        }

        public async Task SetThemeAsync(ThemeMode theme)
        {
            await _databaseService.SaveSettingAsync(ThemeKey, theme.ToString());
            ApplyTheme(theme);
        }

        public async Task ToggleThemeAsync()
        {
            var currentTheme = await GetCurrentThemeAsync();
            var newTheme = currentTheme == ThemeMode.Light ? ThemeMode.Dark : ThemeMode.Light;
            await SetThemeAsync(newTheme);
        }

        public void ApplyTheme(ThemeMode theme)
        {
            var appTheme = new JournalTheme { Mode = theme };

            // Update all color resources
            Application.Current.Resources["PrimaryBackground"] = appTheme.PrimaryBackground;
            Application.Current.Resources["SecondaryBackground"] = appTheme.SecondaryBackground;
            Application.Current.Resources["AccentColor"] = appTheme.AccentColor;
            Application.Current.Resources["PrimaryText"] = appTheme.PrimaryText;
            Application.Current.Resources["SecondaryText"] = appTheme.SecondaryText;
            Application.Current.Resources["CardBackground"] = appTheme.CardBackground;
            Application.Current.Resources["BorderColor"] = appTheme.BorderColor;

            // FIXED: Force Shell to update its colors
            if (Application.Current?.MainPage is Shell shell)
            {

                // Update tab bar colors
                Shell.SetBackgroundColor(shell, appTheme.SecondaryBackground);
                Shell.SetForegroundColor(shell, appTheme.PrimaryText);
                Shell.SetTitleColor(shell, appTheme.PrimaryText);
            }
        }
    }
}