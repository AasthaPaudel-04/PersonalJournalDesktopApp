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

    public class JournalTheme
    {
        public ThemeMode Mode { get; set; }

        // Modern white/black color scheme
        public Color PrimaryBackground => Mode == ThemeMode.Light
            ? Color.FromRgb(255, 255, 255)  // Pure white
            : Color.FromRgb(18, 18, 18);     // Near black (#121212)

        public Color SecondaryBackground => Mode == ThemeMode.Light
            ? Color.FromRgb(248, 249, 250)  // Very light gray (#f8f9fa)
            : Color.FromRgb(30, 30, 30);     // Dark gray (#1e1e1e)

        public Color AccentColor => Mode == ThemeMode.Light
            ? Color.FromRgb(59, 130, 246)   // Blue (#3b82f6)
            : Color.FromRgb(128, 0, 128);   // Purple (#60a5fa)

        public Color PrimaryText => Mode == ThemeMode.Light
            ? Color.FromRgb(17, 24, 39)     // Almost black (#111827)
            : Color.FromRgb(243, 244, 246);  // Almost white (#f3f4f6)

        public Color SecondaryText => Mode == ThemeMode.Light
            ? Color.FromRgb(107, 114, 128)  // Medium gray (#6b7280)
            : Color.FromRgb(107, 114, 128); // Light gray (#9ca3af)

        public Color CardBackground => Mode == ThemeMode.Light
            ? Color.FromRgb(255, 255, 255)  // White cards
            : Color.FromRgb(128, 128, 128);     // Dark cards (#1f2937)

        public Color BorderColor => Mode == ThemeMode.Light
            ? Color.FromRgb(229, 231, 235)  // Light border (#e5e7eb)
            : Color.FromRgb(55, 65, 81);     // Dark border (#374151)
    }
}