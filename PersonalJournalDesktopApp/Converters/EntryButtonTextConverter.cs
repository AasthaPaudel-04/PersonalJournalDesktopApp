using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace PersonalJournalDesktopApp.Converters;

public class EntryButtonTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool hasEntry)
        {
            return hasEntry ? "✏️ Edit Entry" : "➕ Create New Entry";
        }
        return "Create New Entry";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
