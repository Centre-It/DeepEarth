using System;
using System.Globalization;
using System.Windows.Data;


namespace DeepEarth.BingMapsToolkit.Client.Controls
{
        public class OpacityToDisplayValueConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value == null) return "0";
                double opacity;
                return Double.TryParse(value.ToString(), out opacity)
                    ? Math.Round(opacity, 0).ToString() 
                    : "0";
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value == null) return 0;
                int opacity;
                if (Int32.TryParse(value.ToString(), out opacity))
                {

                    if (opacity > 100) return 100;
                    if (opacity < 0) return 0;

                    return opacity;
                }
                return 0;
            }

        }
}
