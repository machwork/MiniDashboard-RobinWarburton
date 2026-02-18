using System.Globalization;
using System.Windows.Data;

namespace MiniDashboard.App.Converters;

/// <summary>
/// Converter for pagination button enablement
/// </summary>
public class PageToBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not int currentPage || parameter == null)
            return false;

        var param = parameter.ToString();
        
        // This converter needs context from the binding, so we'll use a simpler approach
        // We'll handle this differently in the ViewModel
        return true;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

