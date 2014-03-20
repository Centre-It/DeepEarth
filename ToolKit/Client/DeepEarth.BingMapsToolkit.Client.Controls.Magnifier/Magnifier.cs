// Deep Earth is a community project available under the Microsoft Public License (Ms-PL)
// Code is provided as is and with no warrenty – Use at your own risk
// View the project and the latest code at http://DeepEarth.codeplex.com/

using System;
using System.Windows;
using System.Windows.Controls;
using DeepEarth.BingMapsToolkit.Client.Common;
using Microsoft.Maps.MapControl;
using Microsoft.Maps.MapControl.Core;
using System.Windows.Shapes;

namespace DeepEarth.BingMapsToolkit.Client.Controls
{
    [TemplatePart(Name = PART_map, Type = typeof(Map))]
    [TemplatePart(Name = PART_Rect, Type = typeof(Rectangle))]
    [TemplatePart(Name= PART_Circle, Type = typeof(Ellipse))]
    public class Magnifier : Control, IMapControl<MapCore>
    {
        private const string PART_map = "PART_map";
        private const string PART_Rect = "PART_Rect";
        private const string PART_Circle = "PART_Circle";

        private Map magnifierMap;
        private MapCore map;

        private string mapName;
        private int mouseDown = 0;
        private int sliderMouseDown = 0;
        private Point pointMouseDown;
        private Rectangle rectangle;
        private Ellipse circle;
        private Point center;
        

        /// <summary>
        /// Component acting as a magnifier on the maps canvas.
        /// </summary>
        public Magnifier()
        {
            IsEnabled = false;
            DefaultStyleKey = typeof(Magnifier);
            Loaded += Magnifier_Loaded;
        }

        double magnifyLevel = 0.0;
        /// <summary>
        /// The zoom-level of the magnify glass.
        /// </summary>
        public double MagnifyLevel
        {
            get
            {
                return magnifyLevel;
            }
            set
            {
                magnifyLevel = value;
                if (magnifierMap != null)
                    UpdateMagnifier(magnifierMap.Center);
            }
        }

        #region IDisposable Members

        void IDisposable.Dispose()
        {
            MapInstance = null;
            Loaded -= Magnifier_Loaded;
            this.MouseLeftButtonDown -= Magnifier_MouseLeftButtonDown;
            this.MouseLeftButtonUp -= Magnifier_MouseLeftButtonUp;
            this.MouseMove -= Magnifier_MouseMove;

            rectangle.MouseLeftButtonDown -= rectangle_MouseLeftButtonDown;
            rectangle.MouseMove -= rectangle_MouseMove;
            rectangle.MouseLeftButtonUp -= rectangle_MouseLeftButtonUp;
            map.MouseMove -= map_MouseMove;
            map.MouseLeftButtonUp -= map_MouseLeftButtonUp;
            
        }

        #endregion

        #region IMapControl<MapCore> Members

        public MapCore MapInstance
        {
            get { return map; }
            set
            {
                if (map != null)
                {
                    MapInstance.ViewChangeOnFrame -= MapInstance_ViewChangeOnFrame;
                }

                map = value;
                if (map != null)
                {
                    MapInstance.ViewChangeOnFrame += MapInstance_ViewChangeOnFrame;
                }
            }
        }

        public string MapName
        {
            get { return mapName; }
            set
            {
                mapName = value;
                SetMapInstance(value);
            }
        }

        #endregion

        private void Magnifier_Loaded(object sender, RoutedEventArgs e)
        {
            SetMapInstance(mapName);
            pointMouseDown = new Point();
        }

        private void MapInstance_ViewChangeOnFrame(object sender, MapEventArgs e)
        {
            if (magnifierMap != null && MapInstance != null)
            {
                MapCore core = (MapCore)GetTemplateChild(PART_map);

                Point p = new Point { X = Margin.Left + 150, Y = Margin.Top + 150 };
                Location location = MapInstance.ViewportPointToLocation(p);
                UpdateMagnifier(location);

            }
        }

        private void UpdateMagnifier(Location location)
        {
            if (MagnifyLevel < 1.5)
                MagnifyLevel = 1.5;
            double zoom = MapInstance.ZoomLevel + MagnifyLevel;
            zoom = zoom > 21 ? 21 : zoom;
            magnifierMap.SetView(location, zoom);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            magnifierMap = (Map)GetTemplateChild(PART_map);

            IsEnabled = true;

            if (MapInstance != null)
            {
                magnifierMap.CredentialsProvider = MapInstance.CredentialsProvider;
            }

            //turn off interactivity
            magnifierMap.MouseClick += (o, e) => { e.Handled = true; };
            magnifierMap.MouseDoubleClick += (o, e) => { e.Handled = true; };
            magnifierMap.MouseWheel += (o, e) => { e.Handled = true; };
            magnifierMap.MousePan += (o, e) => { e.Handled = true; };
            magnifierMap.KeyDown += (o, e) => { e.Handled = true; };
            magnifierMap.KeyHeld += (o, e) => { e.Handled = true; };
            magnifierMap.MouseDragBox += (o, e) => { e.Handled = true; };

            this.MouseLeftButtonDown += Magnifier_MouseLeftButtonDown;
            this.MouseLeftButtonUp += Magnifier_MouseLeftButtonUp;
            this.MouseMove += Magnifier_MouseMove;
            this.MouseEnter += Magnifier_MouseEnter;

            rectangle = (Rectangle)GetTemplateChild(PART_Rect);
            rectangle.MouseLeftButtonDown += rectangle_MouseLeftButtonDown;
            rectangle.MouseMove += rectangle_MouseMove;
            rectangle.MouseLeftButtonUp += rectangle_MouseLeftButtonUp;
            
            circle = (Ellipse)GetTemplateChild(PART_Circle);
            center = new Point { X = circle.Width / 2, Y = circle.Height / 2 };

            if (map != null)
            {
                magnifierMap.Center = map.Center;
                map.MouseMove += map_MouseMove;
                map.MouseLeftButtonUp += map_MouseLeftButtonUp;
            }
        }

        void SetSliderToZero()
        {
            if (sliderMouseDown == 1)
                sliderMouseDown--;
        }

        void map_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            rectangle_MouseLeftButtonUp(sender, e);
        }

        void rectangle_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SetSliderToZero();
        }

        void map_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            rectangle_MouseMove(sender, e);
        }

        void rectangle_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (sliderMouseDown == 1)
            {
                rectangle.VerticalAlignment = VerticalAlignment.Top;
                rectangle.HorizontalAlignment = HorizontalAlignment.Left;
                Point p = e.GetPosition(circle);

                double angle = Math.Atan2(p.Y - center.Y, p.X - center.X);
                double y = Math.Sin(angle);
                double x = Math.Cos(angle);

                double left = 150 * (1+x); 
                double top = 150 * (1 + y);

                if (left < 150)
                    left = 150;
                if (top < 150)
                    top = 150;

                if (top > 150 && left > 150)
                    rectangle.Margin = new Thickness { Left = left -12, Top = top -12 };

                RotateSliderGrip(angle);
                double magnify = Math.Abs((angle * 180 / Math.PI) - 90) / 10;
                MagnifyLevel = magnify;
            }
        }

        private void RotateSliderGrip(double angle)
        {
            System.Windows.Media.RotateTransform rotate = new System.Windows.Media.RotateTransform();
            double rotation = angle * 180 / Math.PI - 90;

            if (rotation > 0)
                rotation = 0;

            if (rotation < -90)
                rotation = -90;

            rotate.Angle = rotation;
            rectangle.RenderTransform = rotate;
        }

        void rectangle_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            sliderMouseDown++;
        }

        void Magnifier_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SetMouseDownToZero();

            SetSliderToZero();
        }

        private void SetMouseDownToZero()
        {
            if (mouseDown > 0)
                mouseDown = 0; 
        }

        void Magnifier_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            SetMouseDownToZero();
        }

        void Magnifier_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (mouseDown == 1 && sliderMouseDown == 0)
            {
                VerticalAlignment = VerticalAlignment.Top;
                HorizontalAlignment = HorizontalAlignment.Left;
                double mapX = e.GetPosition(MapInstance).X;
                double mapY = e.GetPosition(MapInstance).Y;

                if (mapX > 0 && mapY > 0 && mapX < MapInstance.ViewportSize.Width && mapY < MapInstance.ViewportSize.Height)
                {
                    Margin = new Thickness()
                    {
                        Left = mapX - pointMouseDown.X,
                        Top = mapY - pointMouseDown.Y
                    };

                    UpdateMagnifier(MapInstance.ViewportPointToLocation(e.GetPosition(MapInstance)));
                }
            }
            else if (mouseDown == 1 && sliderMouseDown == 1)
            {
                rectangle_MouseMove(sender, e);
            }
        }

        void Magnifier_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Get position on the control from the main map
            mouseDown++;
            pointMouseDown.X = e.GetPosition(this).X;
            pointMouseDown.Y = e.GetPosition(this).Y;
        }

        private void SetMapInstance(string mapname)
        {
            MapInstance = Utilities.FindVisualChildByName<MapCore>(Application.Current.RootVisual, mapname);
        }
    }
}