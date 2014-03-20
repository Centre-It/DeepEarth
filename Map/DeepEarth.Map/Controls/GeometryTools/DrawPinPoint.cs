using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using DeepEarth.Client.MapControl.Geometry;

#pragma warning disable 1591

namespace DeepEarth.Client.MapControl.Controls.GeometryTools
{
    public class DrawPinPoint : PointBase
    {
        public string LabelText { get; set; }
        public string LabelVisible { get; set; }
        private Image img { get; set; }

        public DrawPinPoint()
        {
            DefaultStyleKey = typeof(DrawPinPoint);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            //img = (Image)GetTemplateChild("PART_PinImage");
            //img.Source = new BitmapImage(new Uri("PinPoints/burn.png", UriKind.Relative));
            //img.Height = 10;
            //img.Width = 10;

            //Layer.UpdateChildLocation(this);
        }
    }
}

#pragma warning restore 1591