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

namespace DeepEarth.BingMapsToolkit.Client.Controls
{
    public class GeoVectorData
    {
        public byte[] Geo { get; set; }

        public string ID { get; set; }

        public string Additional { get; set; }
    }
}
