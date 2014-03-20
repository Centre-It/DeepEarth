using System;
using Microsoft.Maps.MapControl;

namespace DeepEarth.BingMapsToolkit.Client.Controls
{
    public class XYZLocationRectTileSource : LocationRectTileSource
    {
        public override Uri GetUri(int x, int y, int zoomLevel)
        {
            if (base.GetUri(x, y, zoomLevel) != null)
            {
                return new Uri(string.Format(UriFormat, x, y, zoomLevel));
            }
            return null;
        }
    }
}
