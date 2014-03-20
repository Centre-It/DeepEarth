﻿using System;
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
using DeepEarth.BingMapsToolkit.Client.Common.Entities;
using System.Globalization;

namespace DeepEarth.BingMapsToolkit.Client.Common.Converters
{
	public class PolyLineColorConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			StyleSpecification style = value as StyleSpecification;
			if (style == null)
				return null;
			if (style.ShowLine == false)
				return null;

			string val = style.PolygonLineColour;

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