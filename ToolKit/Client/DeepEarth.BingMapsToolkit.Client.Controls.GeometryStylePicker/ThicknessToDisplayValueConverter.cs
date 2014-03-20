using System;
using System.Globalization;
using System.Windows.Data;


namespace DeepEarth.BingMapsToolkit.Client.Controls
{
        public class ThicknessToDisplayValueConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value == null) return "0.00";
                double thickness;
                return Double.TryParse(value.ToString(), out thickness) 
                    ? Math.Round(thickness, 2).ToString("0.00") 
                    : "0.00";
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value == null) return 0.00;
                double thickness;
                if (Double.TryParse(value.ToString(), out thickness))
                {

                    if (thickness > 10) return 10;
                    if (thickness < 0) return 0;

                    return Math.Round(thickness, 2);
                }
                return 0;
            }

        }
}
