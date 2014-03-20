using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DeepEarth.BingMapsToolkit.Client.Common.Converters
{
    /// <summary>
    /// A type converter for visibility and boolean values.
    /// </summary>
    public class VisibilityConverter : IValueConverter
    {
        public object Convert(object value,Type targetType,object parameter,CultureInfo culture)
        {
            bool visibility = false;
            if (value is bool)
            {
                visibility = (bool) value;
            }
            else
            {
                if (value != null)
                {
                    visibility = true;
                }
            }
            return visibility ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value,Type targetType,object parameter,CultureInfo culture)
        {
            var visibility = (Visibility)value;
            return (visibility == Visibility.Visible);
        }
    }
}