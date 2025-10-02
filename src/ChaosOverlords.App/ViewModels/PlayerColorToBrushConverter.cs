using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace ChaosOverlords.App.ViewModels;

public sealed class PlayerColorToBrushConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count < 3)
        {
            return Brushes.Transparent;
        }

        var isControlled = values[0] as bool? ?? false;
        var ownerColor = values[1] as string;
        var fallback = values[2] as IBrush ?? Brushes.Transparent;

        if (!isControlled || string.IsNullOrWhiteSpace(ownerColor))
        {
            return fallback;
        }

        var color = ownerColor.Trim();
        // Simple palette mapping; semi-transparent tint.
        return color switch
        {
            "Red" => new SolidColorBrush(Color.Parse("#20FF0000")),
            "Blue" => new SolidColorBrush(Color.Parse("#200000FF")),
            "Green" => new SolidColorBrush(Color.Parse("#2000FF00")),
            "Yellow" => new SolidColorBrush(Color.Parse("#20FFFF00")),
            "Purple" => new SolidColorBrush(Color.Parse("#20800080")),
            "Orange" => new SolidColorBrush(Color.Parse("#20FFA500")),
            "Cyan" => new SolidColorBrush(Color.Parse("#2000FFFF")),
            "Magenta" => new SolidColorBrush(Color.Parse("#20FF00FF")),
            _ => fallback
        };
    }
}
