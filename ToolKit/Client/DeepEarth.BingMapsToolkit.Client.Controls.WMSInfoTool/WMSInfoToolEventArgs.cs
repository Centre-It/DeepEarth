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
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DeepEarth.BingMapsToolkit.Client.Controls
{
    public class WMSInfoToolEventArgs : EventArgs
    {
        public double Minx { get; set; }
		public double Maxx { get; set; }
		public double Miny { get; set; }
		public double Maxy { get; set; }
		public double X { get; set; }
		public double Y { get; set; }
		public double Width { get; set; }
        public double Height { get; set; }
        public Action<IEnumerable<WMSFeature>> OnWMSInfoToolRequestComplete { get; set; }
    }
}
