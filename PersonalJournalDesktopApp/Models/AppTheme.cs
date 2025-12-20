using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalJournalDesktopApp.Models
{
    public enum ThemeMode
    {
        Light,
        Dark
    }

    public class AppTheme
    {
        public ThemeMode Mode { get; set; }

        // Your custom color palette
        public Color PrimaryBackground => Mode == ThemeMode.Light
            ? Color.FromRgb(234, 216, 194)  // #ead8c2
            : Color.FromRgb(84, 63, 63);     // #543f3f

        public Color SecondaryBackground => Mode == ThemeMode.Light
            ? Color.FromRgb(227, 221, 197)  // #e3ddc5
            : Color.FromRgb(118, 85, 85);    // #765555

        public Color AccentColor => Color.FromRgb(174, 134, 108); // #ae866c

        public Color PrimaryText => Mode == ThemeMode.Light
            ? Color.FromRgb(84, 63, 63)     // #543f3f
            : Color.FromRgb(227, 221, 197);  // #e3ddc5

        public Color SecondaryText => Mode == ThemeMode.Light
            ? Color.FromRgb(118, 85, 85)    // #765555
            : Color.FromRgb(174, 134, 108);  // #ae866c
    }
}
