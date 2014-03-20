//  Deep Earth is a community project available under the Microsoft Public License (Ms-PL)
//  Code is provided as is and with no warrenty – Use at your own risk
//  View the project and the latest code at http://DeepEarth.codeplex.com/

using System;
using System.Windows;
using System.Windows.Browser;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using DeepEarth.Client.MapControl.Events;
using DeepEarth.Client.MapControl.Layers;
using MouseWheelEventArgs=DeepEarth.Client.MapControl.Events.MouseWheelEventArgs;

namespace DeepEarth.Client.MapControl.Controls
{
    public class ZoomSliderControl : Slider, ILayer
    {
        private readonly Slider _Slider;
        private bool _HasFocus;
        private bool _IsZooming;
        private Map _Map;
        private Thumb _Thumb;
        private bool _UpdateSlider;

        public string ID { get; set; }

        public bool IsVisible { get; set; }

        public virtual Map MapInstance
        {
            get
            {
                if(_Map == null)
                {
                    _Map = Map.GetMapInstance(this);
                }
                return _Map;
            }
            set
            {
                if(ReferenceEquals(_Map, value))
                {
                    return;
                }
                _Map = value;
            }
        }

        public bool SnapToZoomLevel { get; set; }


        public ZoomSliderControl()
            : this(Map.DefaultInstance)
        {
        }

        public ZoomSliderControl(Map map)
        {
            _Map = map;
            DefaultStyleKey = typeof(ZoomSliderControl);

            if(HtmlPage.IsEnabled)
            {
                _Thumb = new Thumb();
                _Slider = this;

                // Slider Events
                _Slider.GotFocus += Slider_GotFocus;
                _Slider.LostFocus += Slider_LostFocus;
                _Slider.ValueChanged += ZoomControl_ValueChanged;

                // Map Events
                _Map.Events.MapMouseEnter += Events_MapMouseEnter;
                _Map.Events.MapMouseLeave += Events_MapMouseLeave;
                _Map.Events.MapZoomStarted += Events_MapZoomStarted;
                _Map.Events.MapZoomEnded += Events_MapZoomEnded;
                _Map.Events.MapZoomChanged += Events_MapZoomChanged;
                _Map.Events.MapMouseWheel += Events_MapMouseWheel;
                _Map.Events.MapDoubleClick += Events_MapDoubleClick;
                _Map.Events.MapTileSourceChanged += Events_MapTileSourceChanged;

                SetTileSourceZoomLevels();
                _UpdateSlider = true;
            }
        }


        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if(Orientation == Orientation.Horizontal)
            {
                _Thumb = (Thumb)GetTemplateChild("HorizontalThumb");
            }
            else
            {
                _Thumb = (Thumb)GetTemplateChild("VerticalThumb");
            }
        }

        private void SetTileSourceZoomLevels()
        {
            Maximum = MapInstance.BaseLayer.Source.MaxZoomLevel;
            Minimum = MapInstance.BaseLayer.Source.MinZoomLevel;
        }


        private void Slider_LostFocus(object sender, RoutedEventArgs e)
        {
            _HasFocus = false;
            if(_IsZooming)
            {
                _UpdateSlider = false;
            }
        }

        private void Slider_GotFocus(object sender, RoutedEventArgs e)
        {
            _HasFocus = true;
        }


        private void Events_MapMouseEnter(Map map, MouseEventArgs args)
        {
            VisualStateManager.GoToState(this, "MouseOver", true);
        }

        private void Events_MapMouseLeave(Map map, MouseEventArgs args)
        {
            VisualStateManager.GoToState(this, "Normal", true);
        }

        private void Events_MapMouseWheel(Map map, MouseWheelEventArgs args)
        {
            _UpdateSlider = true;
        }

        private void Events_MapDoubleClick(Map map, MouseButtonEventArgs args)
        {
            _UpdateSlider = true;
        }

        private void Events_MapZoomEnded(Map map, MapEventArgs args)
        {
            _IsZooming = false;
            _UpdateSlider = true;
        }

        private void Events_MapZoomStarted(Map map, MapEventArgs args)
        {
            _IsZooming = true;
        }

        private void Events_MapTileSourceChanged(Map map, MapEventArgs args)
        {
            SetTileSourceZoomLevels();
        }

        private void Events_MapZoomChanged(Map map, double zoomLevel)
        {
            if((_Thumb.IsDragging == false && _IsZooming == false) || (_HasFocus == false && _UpdateSlider))
            {
                Value = zoomLevel;
            }
        }


        private void ZoomControl_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(_Thumb.IsDragging || _HasFocus)
            {
                if(SnapToZoomLevel)
                {
                    _Slider.Value = Math.Round(_Slider.Value);
                }

                if(_Map.BaseLayer.Source.IsValidZoomLevel(_Slider.Value))
                {
                    Rect bounds = _Map.LogicalBounds;

                    Point pixelBoxOrigin = _Map.CoordHelper.LogicalToPixel(new Point(bounds.Left, bounds.Top));
                    Point pixelBoxExtent = _Map.CoordHelper.LogicalToPixel(new Point(bounds.Right, bounds.Bottom));
                    var pixelBox = new Rect(pixelBoxOrigin, pixelBoxExtent);

                    var pixelBoxCenter = new Point
                                         {
                                             X = (pixelBox.X + pixelBox.Width / 2),
                                             Y = (pixelBox.Y + pixelBox.Height / 2)
                                         };

                    Point logicalPoint = _Map.CoordHelper.PixelToLogical(pixelBoxCenter);
                    Size viewSize = _Map.CoordHelper.ZoomLevelToLogicalView(_Slider.Value);

                    double factorX = pixelBoxCenter.X / _Map.MapViewPixelSize.Width;
                    double factorY = pixelBoxCenter.Y / _Map.MapViewPixelSize.Height;

                    var targetLogicalOrigin = new Point
                                              {
                                                  X = (logicalPoint.X - (viewSize.Width * factorX)),
                                                  Y = (logicalPoint.Y - (viewSize.Height * factorY))
                                              };

                    _Map.BaseLayer.MSI.ViewportOrigin = targetLogicalOrigin;
                    _Map.BaseLayer.MSI.ViewportWidth = viewSize.Width;
                }
            }
        }
    }
}