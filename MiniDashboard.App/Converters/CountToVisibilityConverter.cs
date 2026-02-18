using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MiniDashboard.App.Converters;

public class CountToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool invert = parameter?.ToString() == "Invert";
        
        if (value is int count)
        {
            bool hasItems = count > 0;
            return invert ? (hasItems ? Visibility.Collapsed : Visibility.Visible) 
                         : (hasItems ? Visibility.Visible : Visibility.Collapsed);
        }
        
        // If value is not an int, try to get count from collection
        if (value is System.Collections.ICollection collection)
        {
            bool hasItems = collection.Count > 0;
            return invert ? (hasItems ? Visibility.Collapsed : Visibility.Visible) 
                         : (hasItems ? Visibility.Visible : Visibility.Collapsed);
        }
        
        return invert ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

