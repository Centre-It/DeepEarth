using System.Windows.Controls;
using System.Windows.Shapes;
using DeepEarth.Client.MapControl.Geometry;

#pragma warning disable 1591

namespace DeepEarth.Client.MapControl.Controls.GeometryTools
{
    public class DrawLineString : LineString
    {
        public string _LabelText { get; set; }
        public bool _LabelVisible { get; set; }

        public DrawLineString()
        {
            DefaultStyleKey = typeof (DrawLineString);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }
    }
}

#pragma warning restore 1591