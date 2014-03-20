using System.ComponentModel;
using System.Windows.Media;
using DeepEarth.Client.Controls.ColorPicker;

namespace DeepEarth.Client.Controls.GeometryStylePicker
{
    public delegate void LineColorChangedEventHandler(object sender, ColorChangedEventArgs e);
    public delegate void FillColorChangedEventHandler(object sender, ColorChangedEventArgs e);
    public delegate void OpacityChangedEventHandler(object sender, OpacityChangedEventArgs e);
    public delegate void ThicknessChangedEventHandler(object sender, ThicknessChangedEventArgs e);
    public delegate void GeometryStyleChangedEventHandler(object sender, GeometryStyleChangedEventArgs e);

    public class GeometryStyle : INotifyPropertyChanged
    {
        private SolidColorBrush lineColor;
        private SolidColorBrush fillColor;
        private double thickness;
        private double opacity;

        public event PropertyChangedEventHandler PropertyChanged;

        public SolidColorBrush LineColor
        {
            get { return lineColor; }
            set
            {
                if (lineColor == value) return;
                OnLineColorChanged(new ColorChangedEventArgs(lineColor, value));
                var oldStyle = new GeometryStyle
                                   {
                                       FillColor = fillColor,
                                       LineColor = lineColor,
                                       Opacity = opacity,
                                       Thickness = thickness
                                   };
                lineColor = value;
                OnGeometryStyleChanged(new GeometryStyleChangedEventArgs(oldStyle, this));
                OnPropertyChanged("LineColor");
            }
        }

        public SolidColorBrush FillColor
        {
            get { return fillColor; }
            set
            {
                if (fillColor == value) return;
                OnFillColorChanged(new ColorChangedEventArgs(fillColor, value));
                var oldStyle = new GeometryStyle
                {
                    FillColor = fillColor,
                    LineColor = lineColor,
                    Opacity = opacity,
                    Thickness = thickness
                };
                fillColor = value;
                OnGeometryStyleChanged(new GeometryStyleChangedEventArgs(oldStyle, this));
                OnPropertyChanged("FillColor");
            }
        }

        public double Thickness
        {
            get { return thickness; }
            set
            {
                if (thickness == value) return;
                OnThicknessChanged(new ThicknessChangedEventArgs(thickness, value));
                var oldStyle = new GeometryStyle
                {
                    FillColor = fillColor,
                    LineColor = lineColor,
                    Opacity = opacity,
                    Thickness = thickness
                };
                thickness = value;
                OnGeometryStyleChanged(new GeometryStyleChangedEventArgs(oldStyle, this));
                OnPropertyChanged("Thickness");
            }
        }

        public double Opacity
        {
            get { return opacity; }
            set
            {
                if (opacity == value) return;
                OnOpacityChanged(new OpacityChangedEventArgs(opacity, value));
                var oldStyle = new GeometryStyle
                {
                    FillColor = fillColor,
                    LineColor = lineColor,
                    Opacity = opacity,
                    Thickness = thickness
                };
                opacity = value;
                OnGeometryStyleChanged(new GeometryStyleChangedEventArgs(oldStyle, this));
                OnPropertyChanged("Opacity");
            }
        }


        #region events

        #region LineColourChanged
        private event LineColorChangedEventHandler LineColorChangedEvent;

        public event LineColorChangedEventHandler LineColorChanged
        {
            add { LineColorChangedEvent += value; }
            remove { LineColorChangedEvent -= value; }
        }

        protected virtual void OnLineColorChanged(ColorChangedEventArgs e)
        {
            if (LineColorChangedEvent != null)
            {
                LineColorChangedEvent(this, e);
            }
        }
        #endregion

        #region FillColourChanged
        private event FillColorChangedEventHandler FillColorChangedEvent;

        public event FillColorChangedEventHandler FillColorChanged
        {
            add { FillColorChangedEvent += value; }
            remove { FillColorChangedEvent -= value; }
        }

        protected virtual void OnFillColorChanged(ColorChangedEventArgs e)
        {
            if (FillColorChangedEvent != null)
            {
                FillColorChangedEvent(this, e);
            }
        }
        #endregion

        #region ThicknessChanged
        private event ThicknessChangedEventHandler ThicknessChangedEvent;

        public event ThicknessChangedEventHandler ThicknessChanged
        {
            add { ThicknessChangedEvent += value; }
            remove { ThicknessChangedEvent -= value; }
        }

        protected virtual void OnThicknessChanged(ThicknessChangedEventArgs e)
        {
            if (ThicknessChangedEvent != null)
            {
                ThicknessChangedEvent(this, e);
            }
        }
        #endregion

        #region OpacityChanged
        private event OpacityChangedEventHandler OpacityChangedEvent;

        public event OpacityChangedEventHandler OpacityChanged
        {
            add { OpacityChangedEvent += value; }
            remove { OpacityChangedEvent -= value; }
        }

        protected virtual void OnOpacityChanged(OpacityChangedEventArgs e)
        {
            if (OpacityChangedEvent != null)
            {
                OpacityChangedEvent(this, e);
            }
        }
        #endregion

        #region GeometryStyleChanged
        private event GeometryStyleChangedEventHandler GeometryStyleChangedEvent;

        public event GeometryStyleChangedEventHandler GeometryStyleChanged
        {
            add { GeometryStyleChangedEvent += value; }
            remove { GeometryStyleChangedEvent -= value; }
        }

        protected virtual void OnGeometryStyleChanged(GeometryStyleChangedEventArgs e)
        {
            if (GeometryStyleChangedEvent != null)
            {
                GeometryStyleChangedEvent(this, e);
            }
        }
        #endregion

        #endregion


        protected void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
