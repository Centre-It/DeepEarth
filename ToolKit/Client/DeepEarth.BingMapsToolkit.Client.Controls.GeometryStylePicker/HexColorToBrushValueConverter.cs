using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using DeepEarth.BingMapsToolkit.Client.Common;


namespace DeepEarth.BingMapsToolkit.Client.Controls
{
    public class HexColorToBrushValueConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return new SolidColorBrush(Utilities.ColorFromHexString((string)value));
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return Utilities.ColorToHexString(((SolidColorBrush)value).Color);
            }

        }
}
