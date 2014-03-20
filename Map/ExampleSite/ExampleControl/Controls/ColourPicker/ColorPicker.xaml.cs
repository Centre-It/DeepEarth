using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ExampleControl.Controls.ColourPicker
{
    public delegate void ColorChangedEventHandler(object sender, ColorChangedEventArgs e);

    public partial class ColorPicker
    {
        private SolidColorBrush baseHueProperty;
        private double colorCanvasXProperty;
        private double colorCanvasYProperty;
        private bool isCompactProperty;
        private bool isMouseDownOnCanvas;
        private bool isMouseDownOnRainbow;

        private SolidColorBrush selectedColorProperty;

        public ColorPicker()
        {
            InitializeComponent();

            colorCanvasXProperty = 1;
            ColorCanvasY = 1;
            isMouseDownOnCanvas = false;
            UpdateLayout();
            if(RainbowBorder.ActualWidth > 1)
            {
                RainbowHandle.Width = RainbowBorder.ActualWidth - 1;
            }
        }

        #region ColorChanged Event code

        private event ColorChangedEventHandler ColorChangedEvent;

        public event ColorChangedEventHandler ColorChanged
        {
            add { ColorChangedEvent += value; }
            remove { ColorChangedEvent -= value; }
        }

        protected virtual void OnColorChanged(ColorChangedEventArgs e)
        {
            if(ColorChangedEvent != null)
            {
                ColorChangedEvent(this, e);
            }
        }

        #endregion

        #region Dependency Properties

        private double ColorCanvasX
        {
            get { return colorCanvasXProperty; }
            set
            {
                colorCanvasXProperty = value;
                UpdateSelectedColor();
            }
        }

        private double ColorCanvasY
        {
            get { return colorCanvasYProperty; }
            set
            {
                colorCanvasYProperty = value;
                UpdateSelectedColor();
            }
        }

        private SolidColorBrush BaseHue
        {
            get { return baseHueProperty; }
            set
            {
                baseHueProperty = value;
                UpdateSelectedColor();
                if(BackgroundCanvas != null)
                {
                    BackgroundCanvas.Background = baseHueProperty;
                }
            }
        }

        public bool IsCompact
        {
            get { return isCompactProperty; }
            set
            {
                isCompactProperty = value;
                if(LayoutRoot != null)
                {
                    CompactLayoutChange(value);
                }
            }
        }

        public SolidColorBrush SelectedColor
        {
            get { return selectedColorProperty; }
            set
            {
                selectedColorProperty = value;

                if(resultCanvas != null)
                {
                    resultCanvas.Background = selectedColorProperty;
                }
            }
        }

        private void CompactLayoutChange(bool compactChange)
        {
            var oldColorPosition = new Point(Canvas.GetLeft(FinalColor), Canvas.GetTop(FinalColor));
            var oldHuePosition = new Point(0.0, Canvas.GetTop(RainbowHandle));
            var oldColorCanvasSize = new Size(ColorCanvas.ActualWidth, ColorCanvas.ActualHeight);
            var oldHueCanvasSize = new Size(HueCanvas.ActualWidth, HueCanvas.ActualHeight);

            if(compactChange)
            {
                LargePanel.Visibility = Visibility.Collapsed;
                CompactPanel.Visibility = Visibility.Visible;
                rightColumn.MinWidth = 0;
                RootControl.MinHeight = 0;
                RootControl.MinWidth = 0;
                RootControl.Width = 130;
                RootControl.Height = 200;
            }
            else
            {
                LargePanel.Visibility = Visibility.Visible;
                CompactPanel.Visibility = Visibility.Collapsed;
                rightColumn.MinWidth = 90;
                RootControl.Height = double.NaN;
                RootControl.Width = double.NaN;
                RootControl.MinHeight = 190;
                RootControl.MinWidth = 240;
            }
            UpdateLayout();

            RealignElement(oldColorPosition, oldColorCanvasSize, new Size(ColorCanvas.ActualWidth, ColorCanvas.ActualHeight), FinalColor);
            RealignElement(oldHuePosition, oldHueCanvasSize, new Size(HueCanvas.ActualWidth, HueCanvas.ActualHeight), RainbowHandle);

            if(RainbowBorder.ActualWidth > 1)
            {
                RainbowHandle.Width = RainbowBorder.ActualWidth - 1;
            }
        }

        private void RealignElement(Point oldPosition, Size oldCanvasSize, Size newCanvasSize, UIElement elementToRealign)
        {
            //OK... so we find the old size and the old position, turn them into a
            // percentage an apply the new position based on the new size
            if((oldCanvasSize.Width != 0) && (oldCanvasSize.Height != 0))
            {
                double relativeX = oldPosition.X / oldCanvasSize.Width;
                double relativeY = oldPosition.Y / oldCanvasSize.Height;

                Canvas.SetLeft(elementToRealign, (newCanvasSize.Width * relativeX));
                Canvas.SetTop(elementToRealign, (newCanvasSize.Height * relativeY));
            }
        }

        #endregion

        #region Update Color methods

        public void UpdateSelectedColor()
        {
            if(baseHueProperty == null)
            {
                UpdateColorCanvas(150, 0);
            }

            if(baseHueProperty != null)
            {
                Color baseColor = ((baseHueProperty)).Color;

                var newColor = new Color();
                if(colorCanvasXProperty > 1.0)
                {
                    colorCanvasXProperty = 1.0;
                }

                if(colorCanvasYProperty > 1.0)
                {
                    colorCanvasYProperty = 1.0;
                }

                newColor.R = Convert.ToByte((int)(colorCanvasYProperty * (baseColor.R + ((255 - baseColor.R) * colorCanvasXProperty))));
                newColor.G = Convert.ToByte((int)(colorCanvasYProperty * (baseColor.G + ((255 - baseColor.G) * colorCanvasXProperty))));
                newColor.B = Convert.ToByte((int)(colorCanvasYProperty * (baseColor.B + ((255 - baseColor.B) * colorCanvasXProperty))));

                newColor.A = 255;

                var updatedColor = new SolidColorBrush(newColor);

                OnColorChanged(new ColorChangedEventArgs(SelectedColor, updatedColor));

                SelectedColor = new SolidColorBrush(newColor);

                if(CopyColorText != null)
                {
                    RedText.Text = newColor.R.ToString();
                    GreenText.Text = newColor.G.ToString();
                    BlueText.Text = newColor.B.ToString();
                    CopyColorText.Text = CompactRGBText.Text = "" + newColor.R + "," + newColor.G + "," + newColor.B;
                    CopyHexText.Text = CompactHexText.Text = newColor.ToString();
                }
            }
        }

        //private void UpdateColorCanvas(object sender, RoutedEventArgs e)
        private void UpdateColorCanvas(double max, double position)
        {
            var targetColor = new Color();

            if(position > max)
            {
                position = max;
            }

            targetColor.R = GetRedValue(max, position);
            targetColor.G = GetGreenValue(max, position);
            targetColor.B = GetBlueValue(max, position);
            targetColor.A = 255;

            BaseHue = new SolidColorBrush(targetColor);
        }

        #endregion

        #region Color Calculations

        private byte GetRedValue(double range, double value)
        {
            double percentage = value / range;
            int redValue = 0;

            if(percentage <= .3333)
            {
                if(percentage <= .16666)
                {
                    redValue = 255;
                }
                else
                {
                    redValue = Convert.ToInt32(255 * (Math.Abs(.3333 - percentage) / .16666));
                }
            }
            else if(percentage >= .66666)
            {
                if(percentage >= .83333)
                {
                    redValue = 255;
                }
                else
                {
                    redValue = Convert.ToInt32(255 * (Math.Abs(percentage - .6666) / .16666));
                }
            }

            return Convert.ToByte(redValue);
        }

        private byte GetBlueValue(double range, double value)
        {
            double percentage = value / range;
            int blueValue = 0;

            if(percentage >= .66666)
            {
                blueValue = 0;
            }
            else if(percentage <= .16666)
            {
                blueValue = Convert.ToInt32(255 * (Math.Abs(percentage) / .16666));
            }
            else if(percentage <= .5)
            {
                blueValue = 255;
            }
            else if(percentage >= .5)
            {
                blueValue = Convert.ToInt32(255 * (Math.Abs(.66666 - percentage) / .16666));
            }

            return Convert.ToByte(blueValue);
        }

        private byte GetGreenValue(double range, double value)
        {
            double percentage = value / range;
            int greenValue = 0;

            if(percentage <= .3333)
            {
                greenValue = 0;
            }
            else if(percentage >= .8333)
            {
                greenValue = Convert.ToInt32(255 * (Math.Abs(1 - percentage) / .16666));
            }
            else if(percentage >= .5)
            {
                greenValue = 255;
            }
            else if(percentage <= .5)
            {
                greenValue = Convert.ToInt32(255 * (Math.Abs(.3333 - percentage) / .16666));
            }

            return Convert.ToByte(greenValue);
        }

        #endregion

        #region Color Canvas Mouse Event Handlers

        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isMouseDownOnCanvas = false;
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //Let the dragging issue begin
            isMouseDownOnCanvas = true;
            var backgroundCanvas = sender as Canvas;

            //move the little circular canvas thingy
            Point newGridPoint = e.GetPosition(backgroundCanvas);
            Canvas.SetTop(FinalColor, (newGridPoint.Y - 6));
            Canvas.SetLeft(FinalColor, (newGridPoint.X - 6));

            //Set the new Brush
            if(backgroundCanvas != null)
            {
                colorCanvasXProperty = (Math.Abs(backgroundCanvas.ActualWidth - newGridPoint.X)) / backgroundCanvas.ActualWidth;
                ColorCanvasY = (Math.Abs(newGridPoint.Y - backgroundCanvas.ActualHeight)) / backgroundCanvas.ActualHeight;
            }
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if(isMouseDownOnCanvas)
            {
                var backgroundCanvas = sender as Canvas;
                Point newGridPoint = e.GetPosition(backgroundCanvas);
                Canvas.SetTop(FinalColor, (newGridPoint.Y - 6));
                Canvas.SetLeft(FinalColor, (newGridPoint.X - 6));

                //Set the new Brush
                if(backgroundCanvas != null)
                {
                    colorCanvasXProperty = (Math.Abs(backgroundCanvas.ActualWidth - newGridPoint.X)) / backgroundCanvas.ActualWidth;
                    ColorCanvasY = (Math.Abs(newGridPoint.Y - backgroundCanvas.ActualHeight)) / backgroundCanvas.ActualHeight;
                }
            }
        }

        private void Canvas_MouseLeave(object sender, MouseEventArgs e)
        {
            //find out which side the mouse left
            if(isMouseDownOnCanvas)
            {
                if((colorCanvasXProperty - 1) > -.1)
                {
                    colorCanvasXProperty = 1;
                }
                else if((colorCanvasXProperty - .1) < 0)
                {
                    colorCanvasXProperty = 0;
                }

                if((colorCanvasYProperty - 1) > -.1)
                {
                    colorCanvasYProperty = 1;
                }
                else if((colorCanvasYProperty - .1) < 0)
                {
                    colorCanvasYProperty = 0;
                }
                ColorCanvasY = colorCanvasYProperty;
            }
        }

        private void ColorCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RealignElement(new Point(Canvas.GetLeft(FinalColor), Canvas.GetTop(FinalColor)), e.PreviousSize, e.NewSize, FinalColor);
        }

        #endregion

        #region Rainbow Border Event Handlers

        private void RainbowBorder_TurnOn(object sender, MouseButtonEventArgs e)
        {
            isMouseDownOnRainbow = true;
            var thisRainbowBorder = (FrameworkElement)sender;
            Point mousePosInRainbow = e.GetPosition(thisRainbowBorder);
            UpdateColorCanvas(thisRainbowBorder.ActualHeight, (thisRainbowBorder.ActualHeight - mousePosInRainbow.Y));
            Canvas.SetTop(RainbowHandle, (mousePosInRainbow.Y - (RainbowHandle.ActualHeight / 2)));
        }

        private void RainbowBorder_TurnOff(object sender, MouseButtonEventArgs e)
        {
            isMouseDownOnRainbow = false;
        }

        private void RainbowBorder_UpdateHue(object sender, MouseEventArgs e)
        {
            if(isMouseDownOnRainbow)
            {
                var thisRainbowBorder = (FrameworkElement)sender;
                Point mousePosInRainbow = e.GetPosition(thisRainbowBorder);
                if((mousePosInRainbow.Y < RainbowBorder.ActualHeight) && (mousePosInRainbow.Y > 0))
                {
                    UpdateColorCanvas(thisRainbowBorder.ActualHeight, (thisRainbowBorder.ActualHeight - mousePosInRainbow.Y));
                    Canvas.SetTop(RainbowHandle, (mousePosInRainbow.Y - (RainbowHandle.ActualHeight / 2)));
                }
            }
        }

        private void HueCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RealignElement(new Point(0.0, Canvas.GetTop(RainbowHandle)), e.PreviousSize, e.NewSize, RainbowHandle);

            if(RainbowBorder.ActualWidth > 1)
            {
                RainbowHandle.Width = RainbowBorder.ActualWidth - 1;
            }

            if(e.NewSize.Height < 100)
            {
                RainbowHandle.Height = 10;
            }
            else if(e.NewSize.Height > 300)
            {
                RainbowHandle.Height = 30;
            }
            else
            {
                RainbowHandle.Height = e.NewSize.Height / 10;
            }
        }

        #endregion

        private void TurnEverythingOff(object sender, MouseButtonEventArgs e)
        {
            isMouseDownOnRainbow = false;
            isMouseDownOnCanvas = false;
        }
    }
}