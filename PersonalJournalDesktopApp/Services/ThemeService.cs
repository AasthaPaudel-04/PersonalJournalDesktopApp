using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PersonalJournalDesktopApp.Data;
using PersonalJournalDesktopApp.Models;

namespace PersonalJournalDesktopApp.Services
{
    public class ThemeService
    {
        private readonly DatabaseService _database;
        private const string THEME_KEY = "AppTheme";

        public ThemeService(DatabaseService database)
        {
            _database = database;
        }

        // Get current theme from settings
        public async Task<Models.AppTheme> GetCurrentThemeAsync()
        {
            var themeSetting = await _database.GetSettingAsync(THEME_KEY);
            var mode = themeSetting == "Dark" ? ThemeMode.Dark : ThemeMode.Light;
            return new Models.AppTheme { Mode = mode };
        }

        // Save theme preference
        public async Task SaveThemeAsync(ThemeMode mode)
        {
            await _database.SaveSettingAsync(THEME_KEY, mode.ToString());
        }

        // Toggle between Light and Dark
        public async Task ToggleThemeAsync()
        {
            var currentTheme = await GetCurrentThemeAsync();
            var newMode = currentTheme.Mode == ThemeMode.Light ? ThemeMode.Dark : ThemeMode.Light;
            await SaveThemeAsync(newMode);
        }

        // Apply theme to app resources
        public void ApplyTheme(PersonalJournalDesktopApp.Models.AppTheme theme)
        {
            var resources = Application.Current!.Resources;

            // Update dynamic colors
            resources["PrimaryBackground"] = theme.PrimaryBackground;
            resources["SecondaryBackground"] = theme.SecondaryBackground;
            resources["AccentColor"] = theme.AccentColor;
            resources["PrimaryText"] = theme.PrimaryText;
            resources["SecondaryText"] = theme.SecondaryText;
        }
    }
}
