// Deep Earth is a community project available under the Microsoft Public License (Ms-PL)
// Code is provided as is and with no warrenty - Use at your own risk
// View the project and the latest code at http://DeepEarth.codeplex.com/

using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Browser;
using System.Windows.Media;

namespace DeepEarth.Client.Controls.DEMap
{
    public static class Utilities
    {

        public static T FindVisualChildByName<T>(DependencyObject parent, string name) where T : DependencyObject
        {
            if (parent != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                    var controlName = child.GetValue(FrameworkElement.NameProperty) as string;
                    if (controlName == name)
                    {
                        return child as T;
                    }
                    var result = FindVisualChildByName<T>(child, name);
                    if (result != null)
                        return result;
                }
            }
            return null;
        }

        public static T GetParentInstance<T>(FrameworkElement element) where T : FrameworkElement
        {
            var parent = element;
            while (parent != null && parent is T == false)
            {
                parent = (FrameworkElement)VisualTreeHelper.GetParent(parent);
            }
            if (parent != null)
            {
                return parent as T;
            }
            return null;
        }

        public static bool IsDesignTime()
        {
            try
            {
                return DesignerProperties.GetIsInDesignMode(Application.Current.RootVisual);
            }catch
            {
                return false;
            }
        }

        public static double GetScreenDPI()
        {
            if (HtmlPage.IsEnabled)
            {
                var screen = HtmlPage.Window.GetProperty("screen") as ScriptObject;
                if (screen != null)
                {
                    object o = screen.GetProperty("deviceYDPI");
                    if (o != null)
                    {
                        return (double) o;
                    }
                }
            }
            return 96;
        }

        public static Color ColorFromHexString(string HexColor)
        {
            try
            {
                //The input at this point could be HexColor = "FF00FF1F" 
                byte Alpha = byte.Parse(HexColor.Substring(0, 2), NumberStyles.HexNumber);
                byte Red = byte.Parse(HexColor.Substring(2, 2), NumberStyles.HexNumber);
                byte Green = byte.Parse(HexColor.Substring(4, 2), NumberStyles.HexNumber);
                byte Blue = byte.Parse(HexColor.Substring(6, 2), NumberStyles.HexNumber);
                return Color.FromArgb(Alpha, Red, Green, Blue);
            }
            catch (Exception)
            {
                return Colors.Black;
            }
        }

        public static string ColorToHexString(Color color)
        {
            return color.ToString().Replace("#", "");
        }

        public static string InvertColorFromHexString(string HexColor)
        {
            var c = ColorFromHexString(HexColor);
            //invert RGB, keep Alpha
            c.R = (byte)~c.R;
            c.G = (byte)~c.G;
            c.B = (byte)~c.B;
            return ColorToHexString(c);
        }

        public static string DDMMSSFromDecimal(double decimaldegrees)
        {
            double d = Math.Abs(decimaldegrees);
            d += 1.3888888888888888888888888888889e-4;  // add ½ second for rounding  
            var deg = Math.Floor(d);
            var min = Math.Floor((d - deg) * 60);
            var sec = Math.Floor((d - deg - min / 60) * 3600);  
            
            // add leading zeros if required  
            return deg.ToString("000") + '\u00B0' + min.ToString("00") + '\u2032' + sec.ToString("00") + '\u2033';
        }


        public static double GetScaleAtZoomLevel(double latitude, double zoomLevel)
        {
            const double EarthRadius = 6378137;
            const double INCH_TO_METER = 0.0254D;

            double mapWidth = Math.Pow(2, zoomLevel + 8);
            double scale = (Math.Cos(latitude * Math.PI / 180) * 2 * Math.PI * EarthRadius / mapWidth) *
                           (GetScreenDPI() / INCH_TO_METER);
            return scale;
        }

    }
}