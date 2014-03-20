using System.Windows;
using DeepEarth.Client.MapControl.Geometry;

#pragma warning disable 1591

namespace DeepEarth.Client.MapControl.Controls.GeometryTools
{
    public class DrawPoint : PointBase
    {
        //#region Delegates

        //public delegate void DrawPointSelectedHandler(Point location, Size drawPointSize, bool isSelected, PointBase drawPoint);

        //#endregion

        public string LabelText { get; set; }
        public string LabelVisible { get; set; }

        public DrawPoint()
        {
            DefaultStyleKey = typeof (DrawPoint); 
        }
  
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }
    }
}

#pragma warning restore 1591