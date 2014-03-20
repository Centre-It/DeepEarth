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
using DeepEarth.BingMapsToolkit.Common.Entities;

namespace DeepEarth.BingMapsToolkit.Client.Controls
{
    public class VectorDataRequestEventArgs : EventArgs
    {
        public string LayerID { get; set; }

        public Action<IEnumerable<GeoVectorData>> OnVectorDataRequestEnd;
    }
}
