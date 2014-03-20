using System;

namespace DeepEarth.BingMapsToolkit.Client.MapGeometry
{
    public class BalloonEventArgs : EventArgs
    {
        public string LayerID { get; set; }
        public string ItemID { get; set; }
    }
}
