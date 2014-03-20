using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace DeepEarth.BingMapsToolkit.Client.Common.Converters
{
	/// <summary>
	/// A type converter for color.
	/// </summary>
	public class ColorConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
			string val = value.ToString();

			if (string.IsNullOrEmpty(val))
				{ return null; }

			val = val.Replace("#", "");

			byte a = System.Convert.ToByte("ff", 16);
			byte pos = 0;
			if (val.Length == 8)
			{
			    a = System.Convert.ToByte(val.Substring(pos, 2), 16);
			    pos = 2;
			}
			byte r = System.Convert.ToByte(val.Substring(pos, 2), 16);
			pos += 2;
			byte g = System.Convert.ToByte(val.Substring(pos, 2), 16);
			pos += 2;
			byte b = System.Convert.ToByte(val.Substring(pos, 2), 16);
			Color col = Color.FromArgb(a, r, g, b);
			return new SolidColorBrush(col);
        }

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			SolidColorBrush val = value as SolidColorBrush;
			return "#" + val.Color.A.ToString() + val.Color.R.ToString() + val.Color.G.ToString() + val.Color.B.ToString();
		}
	}
}