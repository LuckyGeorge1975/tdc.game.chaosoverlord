using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace ChaosOverlords.App.Converters;

public class ValueToRedGreenConverter : IValueConverter
{
    public static readonly ValueToRedGreenConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int i)
            return i > 0 ? Brushes.Green : i < 0 ? Brushes.Red : Brushes.Gray;
        return Brushes.Yellow;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}