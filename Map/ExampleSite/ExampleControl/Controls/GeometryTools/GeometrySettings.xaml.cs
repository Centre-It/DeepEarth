using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows.Shapes;
using DeepEarth.Client.MapControl;
using DeepEarth.Client.MapControl.Controls.GeometryTools;

namespace ExampleControl.Controls.GeometryTools
{
    public partial class GeometrySettings
    {
        [DefaultValue(Map.DragBehavior.Pan)]
        private Map.DragBehavior _DragBehaviour { get; set; }

        private Rectangle _ColourToSet;

        private Map _Map { get; set; }
        private Color _LineColor { get; set; }
        private Color _FillColor { get; set; }


        public GeometrySettings()
            : this(Map.DefaultInstance)
        {
        }

        public GeometrySettings(Map map)
        {
            InitializeComponent();

            _Map = map;

            _FreeDrawToggleButton.Unchecked += ToggleButton_Unchecked;
            _StringLineToggleButton.Unchecked += ToggleButton_Unchecked;
            _PolygonToggleButton.Unchecked += ToggleButton_Unchecked;
            _PointPinToggleButton.Unchecked += ToggleButton_Unchecked;
            _EraserToggleButton.Unchecked += ToggleButton_Unchecked;

            _FreeDrawToggleButton.Checked += ToggleButton_Checked;
            _StringLineToggleButton.Checked += ToggleButton_Checked;
            _PolygonToggleButton.Checked += ToggleButton_Checked;
            _PointPinToggleButton.Checked += ToggleButton_Checked;
            _EraserToggleButton.Checked += ToggleButton_Checked;

            _Opacity.ValueChanged += Opacity_ValueChanged;
            _OpacityValue.LostFocus += OpacityValue_LostFocus;
            _OpacityValue.KeyDown += OpacityValue_KeyDown;

            _LineThickness.ValueChanged += LineThickness_ValueChanged;
            _ThicknessValue.LostFocus += ThicknessValue_LostFocus;
            _ThicknessValue.KeyDown += ThicknessValue_KeyDown;

            _LineColour.Click += ShowColourPicker_Click;
            _FillColour.Click += ShowColourPicker_Click;

            _ColourPicker.UpdateColourButton.Click += HideColourPicker_Click;
            _ColourPicker.ColorChanged += ColourPicker_ColorChanged;

            _Map.Events.MapLoaded += Events_MapLoaded;
        }

        private void Events_MapLoaded(Map map, DeepEarth.Client.MapControl.Events.MapEventArgs args)
        {
            _OpacityValue.Text = (_Map.GeometryTools.DrawOpacity * 100).ToString();
            _ThicknessValue.Text = Math.Round(_Map.GeometryTools.DrawLineThickness, 2).ToString("0.00");
            _LineColorDisplay.Fill = new SolidColorBrush(_Map.GeometryTools.DrawLineColor);
            _FillColourDisplay.Fill = new SolidColorBrush(_Map.GeometryTools.DrawFillColor);

            SetThickness();
            SetOpacity();
        }


        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            var tb = sender as ToggleButton;
            if(tb != null)
            {
                foreach(ToggleButton b in GeometryButtons.Children)
                {
                    if(_Map.DragMode != Map.DragBehavior.Draw) _DragBehaviour = _Map.DragMode;
                    if(tb != b) b.IsChecked = false;
                }

                _Map.DragMode = Map.DragBehavior.Draw;

                switch(tb.Name)
                {
                    case "_FreeDrawToggleButton":
                        _Map.GeometryTools.DrawMode = DrawControl.GeometryMode.FreeDraw;
                        break;

                    case "_StringLineToggleButton":
                        _Map.GeometryTools.DrawMode = DrawControl.GeometryMode.StringLine;
                        break;

                    case "_PolygonToggleButton":
                        _Map.GeometryTools.DrawMode = DrawControl.GeometryMode.Polygon;
                        break;

                    case "_PointPinToggleButton":
                        _Map.GeometryTools.DrawMode = DrawControl.GeometryMode.PointPin;
                        break;

                    case "_EraserToggleButton":
                        _Map.GeometryTools.DrawMode = DrawControl.GeometryMode.Eraser;
                        break;
                }
            }
        }

        private void ToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            bool _IsDrawMode = false;
            foreach(ToggleButton b in GeometryButtons.Children)
            {
                if(b.IsChecked == true)
                {
                    _IsDrawMode = true;
                }
            }

            if(!_IsDrawMode) _Map.DragMode = _DragBehaviour;
        }


        private void ShowColourPicker_Click(object sender, RoutedEventArgs e)
        {
            var b = sender as Button;
            if(b != null)
            {
                switch(b.Name)
                {
                    case "_LineColour":
                        _ColourToSet = _LineColorDisplay;
                        _ColourPicker.SelectedColor.Color = _LineColor;
                        //_ColourPicker.SelectedColor.Color = _LineColor;
                        //_ColourPicker.SelectedColor.Color.B = _LineColor.B;
                        //_ColourPicker.SelectedColor.Color.G = _LineColor.G;
                        //_ColourPicker.SelectedColor.Color.R = _LineColor.R;
                        break;
                    case "_FillColour":
                        _ColourToSet = _FillColourDisplay;
                        _ColourPicker.SelectedColor.Color = _FillColor;
                        break;
                }
            }
            //_ColourPicker.UpdateSelectedColor();
            VisualStateManager.GoToState(this, "ShowColourPicker", true);
        }

        private void HideColourPicker_Click(object sender, RoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "HideColourPicker", true);
        }

        private void ColourPicker_ColorChanged(object sender, ColourPicker.ColorChangedEventArgs e)
        {
            _ColourToSet.Fill = e.newColor;

            switch(_ColourToSet.Name)
            {
                case "_LineColorDisplay":
                    _LineColor = e.newColor.Color;
                    _Map.GeometryTools.DrawLineColor = e.newColor.Color;
                    break;
                case "_FillColourDisplay":
                    _FillColor = e.newColor.Color;
                    _Map.GeometryTools.DrawFillColor = e.newColor.Color;
                    break;
            }
            //UpdateSettings();
        }


        private void LineThickness_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _ThicknessValue.Text = Math.Round(_LineThickness.Value, 2).ToString("0.00");
            _Map.GeometryTools.DrawLineThickness = _LineThickness.Value;
        }

        private void Opacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _OpacityValue.Text = Math.Round(_Opacity.Value).ToString();
            _Map.GeometryTools.DrawOpacity = _Opacity.Value / 100;
        }


        private void ThicknessValue_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                SetThickness();
            }
        }

        private void ThicknessValue_LostFocus(object sender, RoutedEventArgs e)
        {
            SetThickness();
        }

        private void OpacityValue_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                SetOpacity();
            }
        }

        private void OpacityValue_LostFocus(object sender, RoutedEventArgs e)
        {
            SetOpacity();
        }


        private void SetThickness()
        {
            var thickness = Math.Round(double.Parse(_ThicknessValue.Text), 2);

            //if(thickness > 10 || thickness < 0) HtmlPage.Window.Alert("Line Thinkness value between 0 and 10");
            if(thickness > 10) thickness = 10;
            if(thickness < 0) thickness = 0;

            _ThicknessValue.Text = thickness.ToString("0.00");
            _LineThickness.Value = thickness;
            _Map.GeometryTools.DrawLineThickness = _LineThickness.Value;
        }

        private void SetOpacity()
        {
            var opacity = (int)Math.Round(double.Parse(_OpacityValue.Text), 0);

            //if(opacity > 100 || opacity < 0) HtmlPage.Window.Alert("Opacity value between 0 and 100");
            if(opacity > 100) opacity = 100;
            if(opacity < 0) opacity = 0;

            _OpacityValue.Text = opacity.ToString();
            _Opacity.Value = opacity;
            _Map.GeometryTools.DrawOpacity = _Opacity.Value / 100;
        }
    }
}