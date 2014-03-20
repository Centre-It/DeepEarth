// Deep Earth is a community project available under the Microsoft Public License (Ms-PL)
// Code is provided as is and with no warrenty – Use at your own risk
// View the project and the latest code at http://DeepEarth.codeplex.com/

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DeepEarth.BingMapsToolkit.Client.Common.Entities;
using Microsoft.Maps.MapControl.Core;

namespace DeepEarth.BingMapsToolkit.Client.MapGeometry.Drawing
{
    [TemplatePart(Name = PART_ScaleTransform, Type = typeof(ScaleTransform))]
    [TemplatePart(Name = PART_Image, Type = typeof(Image))]
    public class DrawPointCustom : DrawPoint
    {

        private const string PART_Image = "PART_Image";
        private const string PART_ScaleTransform = "PART_ScaleTransform";
        private Image imageControl;
        private double scale;
        private ScaleTransform scaleTransform;

        public DrawPointCustom(MapCore mapInstance)
            : base(mapInstance)
        {
            DefaultStyleKey = typeof(DrawPointCustom);
        }
        private StyleSpecification geometryStyle;
        public StyleSpecification GeometryStyle
        {
            get { return geometryStyle; }
            set
            {
                geometryStyle = value;
                applyStyle();
            }
        }

        public double Scale
        {
            get { return scale; }
            set
            {
                scale = value;
                applyScale();
            }
        }

        public override void OnApplyTemplate()
        {
            scaleTransform = (ScaleTransform)GetTemplateChild("PART_ScaleTransform");
            imageControl = (Image)GetTemplateChild("PART_Image");

            base.OnApplyTemplate();

            applyStyle();
        }

        private void applyStyle()
        {
            applyScale();

            if (GeometryStyle != null && !string.IsNullOrEmpty(GeometryStyle.IconURL) && imageControl != null)
            {
                imageControl.Source = new BitmapImage(new Uri(GeometryStyle.IconURL, UriKind.Absolute));
            }
        }

        private void applyScale()
        {
            if (scaleTransform != null && GeometryStyle != null)
            {
                var scaleFactor = GeometryStyle.IconScale.GetValueOrDefault();
                if (scaleFactor == 0)
                {
                    scaleFactor = 1;
                }
                scaleTransform.ScaleX = Scale * scaleFactor;
                scaleTransform.ScaleY = Scale * scaleFactor;
            }
        }
    }
}