using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Data;
using DeepEarth.BingMapsToolkit.Client.Common;
using System.Globalization;
using DeepEarth.BingMapsToolkit.Client.Common.Entities;

namespace DeepEarth.BingMapsToolkit.Client.Common.Converters
{
	public class IconSourceConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			StyleSpecification style = value as StyleSpecification;
			if (style == null)
				return null;

			return style.IconURL;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return null;
		}
	}
}

