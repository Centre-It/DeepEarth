using System;
using DeepEarth.BingMapsToolkit.Client.Common.Entities;
using GeoAPI.Geometries;

namespace DeepEarth.BingMapsToolkit.Client.MapGeometry
{
    public class GeometryChangedEventArgs : EventArgs
    {
        public IGeometry OldGeometry { get; set; }
        public IGeometry NewGeometry { get; set; }
        public string LayerID { get; set; }
        public string StyleID { get; set; }
        public StyleSpecification StyleSpecification { get; set; }
        public bool Cancel { get; set; }
    }
}
