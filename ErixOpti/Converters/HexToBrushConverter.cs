using System.Globalization;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace ErixOpti.Converters;

public sealed class HexToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var hex = value as string;
        if (string.IsNullOrWhiteSpace(hex) || hex[0] != '#' || hex.Length is not (7 or 9))
            return new SolidColorBrush(Microsoft.UI.Colors.Gray);

        try
        {
            byte a = 255, r, g, b;
            if (hex.Length == 9)
            {
                a = byte.Parse(hex.AsSpan(1, 2), NumberStyles.HexNumber);
                r = byte.Parse(hex.AsSpan(3, 2), NumberStyles.HexNumber);
                g = byte.Parse(hex.AsSpan(5, 2), NumberStyles.HexNumber);
                b = byte.Parse(hex.AsSpan(7, 2), NumberStyles.HexNumber);
            }
            else
            {
                r = byte.Parse(hex.AsSpan(1, 2), NumberStyles.HexNumber);
                g = byte.Parse(hex.AsSpan(3, 2), NumberStyles.HexNumber);
                b = byte.Parse(hex.AsSpan(5, 2), NumberStyles.HexNumber);
            }

            return new SolidColorBrush(Windows.UI.Color.FromArgb(a, r, g, b));
        }
        catch
        {
            return new SolidColorBrush(Microsoft.UI.Colors.Gray);
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotSupportedException();
}
