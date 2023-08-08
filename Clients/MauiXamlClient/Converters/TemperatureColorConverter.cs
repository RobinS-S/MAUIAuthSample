using System.Globalization;

namespace MauiXamlClient.Converters;

public class TemperatureColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int temperature)
        {
            if (temperature < 20)
                return Color.FromRgb(0, 0, 250);
            if (temperature >= 20 && temperature < 30)
                return Color.FromRgb(0, 250, 0);
            return Color.FromRgb(250, 0, 0);
        }

        return Color.FromRgb(0, 0, 0);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}