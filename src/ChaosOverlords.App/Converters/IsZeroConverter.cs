using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace ChaosOverlords.App.Converters;

public sealed class IsZeroConverter : IValueConverter
{
    public static readonly IsZeroConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int i)
            return i == 0;

        // If null or unexpected, default to not visible false
        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
