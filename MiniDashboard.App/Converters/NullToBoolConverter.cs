using System.Globalization;
using System.Windows.Data;

namespace MiniDashboard.App.Converters;

public class NullToBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool invert = parameter?.ToString() == "Invert";
        bool isNotNull = value != null;
        return invert ? !isNotNull : isNotNull;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}



