using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace AErenderLauncher.Classes.Converters;

public class IntStringConverter : IValueConverter {
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (value is string str) {
            return int.TryParse(str, out var result) ? result : 0;
        }
        if (value is int) {
            return $"{value}";
        }
        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (value is int i) {
            return i.ToString();
        }
        if (value is string str) {
            return int.TryParse(str, out var result) ? result : 0;
        }
        return null;
    }
}