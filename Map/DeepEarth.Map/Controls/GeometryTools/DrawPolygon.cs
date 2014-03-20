using DeepEarth.Client.MapControl.Geometry;

#pragma warning disable 1591

namespace DeepEarth.Client.MapControl.Controls.GeometryTools
{
    public class DrawPolygon : Polygon
    {
        public DrawPolygon()
        {
            DefaultStyleKey = typeof (DrawPolygon);
        }

        public string LabelText { get; set; }
        public string LabelVisible { get; set; }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }
    }
}

#pragma warning restore 1591