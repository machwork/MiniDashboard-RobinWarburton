using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MiniDashboard.App.Converters;

/// <summary>
/// Converter that shows empty state only when ItemsCount is 0 AND IsLoading is false
/// </summary>
public class EmptyStateVisibilityConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values == null || values.Length < 2)
            return Visibility.Collapsed;

        // values[0] should be ItemsCount, values[1] should be IsLoading
        var itemsCount = values[0] is int count ? count : 0;
        var isLoading = values[1] is bool loading ? loading : false;

        // Show empty state only when count is 0 AND not loading
        if (itemsCount == 0 && !isLoading)
        {
            return Visibility.Visible;
        }

        return Visibility.Collapsed;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

