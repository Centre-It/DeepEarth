// Deep Earth is a community project available under the Microsoft Public License (Ms-PL)
// Code is provided as is and with no warrenty – Use at your own risk
// View the project and the latest code at http://DeepEarth.codeplex.com/

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DeepEarth.BingMapsToolkit.Client.Controls
{
    public delegate void ColorChangedEventHandler(object sender, ColorChangedEventArgs e);

    [TemplatePart(Name = PART_colorCanvas, Type = typeof (Canvas))]
    [TemplatePart(Name = PART_hueCanvas, Type = typeof (Canvas))]
    [TemplatePart(Name = PART_backgroundCanvas, Type = typeof (Canvas))]
    [TemplatePart(Name = PART_rainbowBorder, Type = typeof (Border))]
    [TemplatePart(Name = PART_resultCanvas, Type = typeof (Border))]
    [TemplatePart(Name = PART_rootControl, Type = typeof (Border))]
    [TemplatePart(Name = PART_rainbowHandle, Type = typeof (Grid))]
    [TemplatePart(Name = PART_finalColor, Type = typeof (Grid))]
    [TemplatePart(Name = PART_layoutRoot, Type = typeof (Grid))]
    [TemplatePart(Name = PART_largePanel, Type = typeof (StackPanel))]
    [TemplatePart(Name = PART_compactPanel, Type = typeof (StackPanel))]
    [TemplatePart(Name = PART_rightColumn, Type = typeof (ColumnDefinition))]
    [TemplatePart(Name = PART_copyColorText, Type = typeof (ClickTextBox))]
    [TemplatePart(Name = PART_compactRGBText, Type = typeof (ClickTextBox))]
    [TemplatePart(Name = PART_compactHexText, Type = typeof (ClickTextBox))]
    [TemplatePart(Name = PART_copyHexText, Type = typeof (ClickTextBox))]
    [TemplatePart(Name = PART_redText, Type = typeof (TextBlock))]
    [TemplatePart(Name = PART_blueText, Type = typeof (TextBlock))]
    [TemplatePart(Name = PART_greenText, Type = typeof (TextBlock))]
    [TemplatePart(Name = PART_Opacity, Type = typeof (Slider))]
    public class ColorPicker : Control, IDisposable
    {
        private const string PART_backgroundCanvas = "PART_backgroundCanvas";
        private const string PART_blueText = "PART_blueText";
        private const string PART_colorCanvas = "PART_colorCanvas";
        private const string PART_compactHexText = "PART_compactHexText";
        private const string PART_compactPanel = "PART_compactPanel";
        private const string PART_compactRGBText = "PART_compactRGBText";
        private const string PART_copyColorText = "PART_copyColorText";
        private const string PART_copyHexText = "PART_copyHexText";
        private const string PART_finalColor = "PART_finalColor";
        private const string PART_greenText = "PART_greenText";
        private const string PART_hueCanvas = "PART_hueCanvas";
        private const string PART_largePanel = "PART_largePanel";
        private const string PART_layoutRoot = "PART_layoutRoot";
        private const string PART_Opacity = "PART_Opacity";
        private const string PART_rainbowBorder = "PART_rainbowBorder";
        private const string PART_rainbowHandle = "PART_rainbowHandle";
        private const string PART_redText = "PART_redText";
        private const string PART_resultCanvas = "PART_resultCanvas";
        private const string PART_rightColumn = "PART_rightColumn";
        private const string PART_rootControl = "PART_rootControl";
        private Canvas backgroundCanvas;

        private SolidColorBrush baseHueProperty;
        private TextBlock blueText;
        private Canvas colorCanvas;
        private double colorCanvasXProperty;
        private double colorCanvasYProperty;
        private ClickTextBox compactHexText;
        private StackPanel compactPanel;
        private ClickTextBox compactRGBText;
        private ClickTextBox copyColorText;
        private ClickTextBox copyHexText;
        private Grid finalColor;
        private TextBlock greenText;
        private Canvas hueCanvas;
        private bool isCompactProperty;
        private bool isMouseDownOnCanvas;
        private bool isMouseDownOnRainbow;
        private StackPanel largePanel;
        private Grid layoutRoot;
        private Slider opacitySlider;

        private Border rainbowBorder;
        private Grid rainbowHandle;
        private TextBlock redText;
        private Border resultCanvas;
        private ColumnDefinition rightColumn;
        private Border rootControl;
        private byte alpha;


        private SolidColorBrush selectedColorProperty;

        public ColorPicker()
        {
            IsEnabled = false;
            alpha = Convert.ToByte(255);
            DefaultStyleKey = typeof (ColorPicker);
            Loaded += (ColorPicker_Loaded);
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (colorCanvas != null)
            {
                colorCanvas.MouseLeftButtonUp -= (Canvas_MouseLeftButtonUp);
                colorCanvas.MouseLeftButtonDown -= (Canvas_MouseLeftButtonDown);
                colorCanvas.MouseLeave -= (Canvas_MouseLeave);
                colorCanvas.MouseMove -= (Canvas_MouseMove);
                colorCanvas.SizeChanged -= (ColorCanvas_SizeChanged);
            }

            if (hueCanvas != null)
            {
                hueCanvas.SizeChanged -= (HueCanvas_SizeChanged);
            }

            if (rainbowBorder != null)
            {
                rainbowBorder.MouseLeftButtonDown -= (RainbowBorder_TurnOn);
                rainbowBorder.MouseLeftButtonUp -= (RainbowBorder_TurnOff);
                rainbowBorder.MouseMove -= (RainbowBorder_UpdateHue);
            }
        }

        #endregion

        private void ColorPicker_Loaded(object sender, RoutedEventArgs e)
        {
            colorCanvasXProperty = 1;
            ColorCanvasY = 1;
            isMouseDownOnCanvas = false;
            UpdateLayout();
            if (rainbowBorder.ActualWidth > 1)
            {
                rainbowHandle.Width = rainbowBorder.ActualWidth - 1;
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            backgroundCanvas = (Canvas) GetTemplateChild(PART_backgroundCanvas);
            colorCanvas = (Canvas) GetTemplateChild(PART_colorCanvas);
            hueCanvas = (Canvas) GetTemplateChild(PART_hueCanvas);
            resultCanvas = (Border) GetTemplateChild(PART_resultCanvas);
            rootControl = (Border) GetTemplateChild(PART_rootControl);
            rainbowBorder = (Border) GetTemplateChild(PART_rainbowBorder);
            rainbowHandle = (Grid) GetTemplateChild(PART_rainbowHandle);
            finalColor = (Grid) GetTemplateChild(PART_finalColor);
            layoutRoot = (Grid) GetTemplateChild(PART_layoutRoot);
            largePanel = (StackPanel) GetTemplateChild(PART_largePanel);
            compactPanel = (StackPanel) GetTemplateChild(PART_compactPanel);
            rightColumn = (ColumnDefinition) GetTemplateChild(PART_rightColumn);
            copyColorText = (ClickTextBox) GetTemplateChild(PART_copyColorText);
            compactRGBText = (ClickTextBox) GetTemplateChild(PART_compactRGBText);
            compactHexText = (ClickTextBox) GetTemplateChild(PART_compactHexText);
            copyHexText = (ClickTextBox) GetTemplateChild(PART_copyHexText);
            redText = (TextBlock) GetTemplateChild(PART_redText);
            blueText = (TextBlock) GetTemplateChild(PART_blueText);
            greenText = (TextBlock) GetTemplateChild(PART_greenText);
            opacitySlider = (Slider) GetTemplateChild(PART_Opacity);

            colorCanvas.MouseLeftButtonUp += (Canvas_MouseLeftButtonUp);
            colorCanvas.MouseLeftButtonDown += (Canvas_MouseLeftButtonDown);
            colorCanvas.MouseLeave += (Canvas_MouseLeave);
            colorCanvas.MouseMove += (Canvas_MouseMove);
            colorCanvas.SizeChanged += (ColorCanvas_SizeChanged);

            hueCanvas.SizeChanged += (HueCanvas_SizeChanged);

            rainbowBorder.MouseLeftButtonDown += (RainbowBorder_TurnOn);
            rainbowBorder.MouseLeftButtonUp += (RainbowBorder_TurnOff);
            rainbowBorder.MouseMove += (RainbowBorder_UpdateHue);

            opacitySlider.ValueChanged += opacitySlider_ValueChanged;

            IsEnabled = true;
        }

        #region ColorChanged Event code

        public event ColorChangedEventHandler ColorChanged;

        protected virtual void OnColorChanged(ColorChangedEventArgs e)
        {
            if (ColorChanged != null)
            {
                ColorChanged(this, e);
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
                if (backgroundCanvas != null)
                {
                    backgroundCanvas.Background = baseHueProperty;
                }
            }
        }

        public bool IsCompact
        {
            get { return isCompactProperty; }
            set
            {
                isCompactProperty = value;
                if (layoutRoot != null)
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
                if (selectedColorProperty == value) return;
                selectedColorProperty = value;
                if (resultCanvas != null)
                {
                    resultCanvas.Background = selectedColorProperty;
                }
                if (opacitySlider != null)
                {
                    opacitySlider.Value = SelectedColor.Color.A;
                }
            }
        }

        private void CompactLayoutChange(bool compactChange)
        {
            var oldColorPosition = new Point(Canvas.GetLeft(finalColor), Canvas.GetTop(finalColor));
            var oldHuePosition = new Point(0.0, Canvas.GetTop(rainbowHandle));
            var oldColorCanvasSize = new Size(colorCanvas.ActualWidth, colorCanvas.ActualHeight);
            var oldHueCanvasSize = new Size(hueCanvas.ActualWidth, hueCanvas.ActualHeight);

            if (compactChange)
            {
                largePanel.Visibility = Visibility.Collapsed;
                compactPanel.Visibility = Visibility.Visible;
                rightColumn.MinWidth = 0;
                rootControl.MinHeight = 0;
                rootControl.MinWidth = 0;
                rootControl.Width = 130;
                rootControl.Height = 200;
            }
            else
            {
                largePanel.Visibility = Visibility.Visible;
                compactPanel.Visibility = Visibility.Collapsed;
                rightColumn.MinWidth = 90;
                rootControl.Height = double.NaN;
                rootControl.Width = double.NaN;
                rootControl.MinHeight = 190;
                rootControl.MinWidth = 240;
            }
            UpdateLayout();

            RealignElement(oldColorPosition, oldColorCanvasSize,
                           new Size(colorCanvas.ActualWidth, colorCanvas.ActualHeight), finalColor);
            RealignElement(oldHuePosition, oldHueCanvasSize, new Size(hueCanvas.ActualWidth, hueCanvas.ActualHeight),
                           rainbowHandle);

            if (rainbowBorder.ActualWidth > 1)
            {
                rainbowHandle.Width = rainbowBorder.ActualWidth - 1;
            }
        }

        private static void RealignElement(Point oldPosition, Size oldCanvasSize, Size newCanvasSize,
                                           UIElement elementToRealign)
        {
            //OK... so we find the old size and the old position, turn them into a
            // percentage an apply the new position based on the new size
            if ((oldCanvasSize.Width != 0) && (oldCanvasSize.Height != 0))
            {
                double relativeX = oldPosition.X/oldCanvasSize.Width;
                double relativeY = oldPosition.Y/oldCanvasSize.Height;

                Canvas.SetLeft(elementToRealign, (newCanvasSize.Width*relativeX));
                Canvas.SetTop(elementToRealign, (newCanvasSize.Height*relativeY));
            }
        }

        #endregion

        #region Update Color methods

        public void UpdateSelectedColor()
        {
            if (baseHueProperty == null)
            {
                UpdateColorCanvas(150, 0);
            }

            if (baseHueProperty == null) return;

            Color baseColor = ((baseHueProperty)).Color;

            var newColor = new Color();
            if (colorCanvasXProperty > 1.0)
            {
                colorCanvasXProperty = 1.0;
            }

            if (colorCanvasYProperty > 1.0)
            {
                colorCanvasYProperty = 1.0;
            }

            newColor.R =
                Convert.ToByte(
                    (int) (colorCanvasYProperty*(baseColor.R + ((255 - baseColor.R)*colorCanvasXProperty))));
            newColor.G =
                Convert.ToByte(
                    (int) (colorCanvasYProperty*(baseColor.G + ((255 - baseColor.G)*colorCanvasXProperty))));
            newColor.B =
                Convert.ToByte(
                    (int) (colorCanvasYProperty*(baseColor.B + ((255 - baseColor.B)*colorCanvasXProperty))));

            newColor.A = alpha;

            var updatedColor = new SolidColorBrush(newColor);

            OnColorChanged(new ColorChangedEventArgs(SelectedColor, updatedColor));

            SelectedColor = new SolidColorBrush(newColor);

            if (copyColorText == null) return;

            redText.Text = newColor.R.ToString();
            greenText.Text = newColor.G.ToString();
            blueText.Text = newColor.B.ToString();
            copyColorText.Text = compactRGBText.Text = "" + newColor.R + "," + newColor.G + "," + newColor.B;
            copyHexText.Text = compactHexText.Text = newColor.ToString();
        }

        private void UpdateColorCanvas(double max, double position)
        {
            var targetColor = new Color();

            if (position > max)
            {
                position = max;
            }

            targetColor.R = GetRedValue(max, position);
            targetColor.G = GetGreenValue(max, position);
            targetColor.B = GetBlueValue(max, position);
            targetColor.A = alpha;

            BaseHue = new SolidColorBrush(targetColor);
        }

        #endregion

        #region Color Calculations

        private static byte GetRedValue(double range, double value)
        {
            double percentage = value/range;
            int redValue = 0;

            if (percentage <= .3333)
            {
                redValue = percentage <= .16666 ? 255 : Convert.ToInt32(255*(Math.Abs(.3333 - percentage)/.16666));
            }
            else if (percentage >= .66666)
            {
                redValue = percentage >= .83333 ? 255 : Convert.ToInt32(255*(Math.Abs(percentage - .6666)/.16666));
            }

            return Convert.ToByte(redValue);
        }

        private static byte GetBlueValue(double range, double value)
        {
            double percentage = value/range;
            int blueValue = 0;

            if (percentage >= .66666)
            {
                blueValue = 0;
            }
            else if (percentage <= .16666)
            {
                blueValue = Convert.ToInt32(255*(Math.Abs(percentage)/.16666));
            }
            else if (percentage <= .5)
            {
                blueValue = 255;
            }
            else if (percentage >= .5)
            {
                blueValue = Convert.ToInt32(255*(Math.Abs(.66666 - percentage)/.16666));
            }

            return Convert.ToByte(blueValue);
        }

        private static byte GetGreenValue(double range, double value)
        {
            double percentage = value/range;
            int greenValue = 0;

            if (percentage <= .3333)
            {
                greenValue = 0;
            }
            else if (percentage >= .8333)
            {
                greenValue = Convert.ToInt32(255*(Math.Abs(1 - percentage)/.16666));
            }
            else if (percentage >= .5)
            {
                greenValue = 255;
            }
            else if (percentage <= .5)
            {
                greenValue = Convert.ToInt32(255*(Math.Abs(.3333 - percentage)/.16666));
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
            var canvas = sender as Canvas;

            //move the little circular canvas thingy
            Point newGridPoint = e.GetPosition(canvas);
            Canvas.SetTop(finalColor, (newGridPoint.Y - 6));
            Canvas.SetLeft(finalColor, (newGridPoint.X - 6));

            //Set the new Brush
            if (canvas == null) return;

            colorCanvasXProperty = (Math.Abs(canvas.ActualWidth - newGridPoint.X))/
                                   canvas.ActualWidth;
            ColorCanvasY = (Math.Abs(newGridPoint.Y - canvas.ActualHeight))/canvas.ActualHeight;
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isMouseDownOnCanvas) return;

            var canvas = sender as Canvas;
            Point newGridPoint = e.GetPosition(canvas);
            Canvas.SetTop(finalColor, (newGridPoint.Y - 6));
            Canvas.SetLeft(finalColor, (newGridPoint.X - 6));

            //Set the new Brush
            if (canvas == null) return;

            colorCanvasXProperty = (Math.Abs(canvas.ActualWidth - newGridPoint.X))/
                                   canvas.ActualWidth;
            ColorCanvasY = (Math.Abs(newGridPoint.Y - canvas.ActualHeight))/
                           canvas.ActualHeight;
        }

        private void Canvas_MouseLeave(object sender, MouseEventArgs e)
        {
            //find out which side the mouse left
            if (!isMouseDownOnCanvas) return;

            if ((colorCanvasXProperty - 1) > -.1)
            {
                colorCanvasXProperty = 1;
            }
            else if ((colorCanvasXProperty - .1) < 0)
            {
                colorCanvasXProperty = 0;
            }

            if ((colorCanvasYProperty - 1) > -.1)
            {
                colorCanvasYProperty = 1;
            }
            else if ((colorCanvasYProperty - .1) < 0)
            {
                colorCanvasYProperty = 0;
            }
            ColorCanvasY = colorCanvasYProperty;
        }

        private void ColorCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RealignElement(new Point(Canvas.GetLeft(finalColor), Canvas.GetTop(finalColor)), e.PreviousSize, e.NewSize,
                           finalColor);
        }

        #endregion

        #region Rainbow Border Event Handlers

        private void RainbowBorder_TurnOn(object sender, MouseButtonEventArgs e)
        {
            isMouseDownOnRainbow = true;
            var thisRainbowBorder = (FrameworkElement) sender;
            Point mousePosInRainbow = e.GetPosition(thisRainbowBorder);
            UpdateColorCanvas(thisRainbowBorder.ActualHeight, (thisRainbowBorder.ActualHeight - mousePosInRainbow.Y));
            Canvas.SetTop(rainbowHandle, (mousePosInRainbow.Y - (rainbowHandle.ActualHeight/2)));
        }

        private void RainbowBorder_TurnOff(object sender, MouseButtonEventArgs e)
        {
            isMouseDownOnRainbow = false;
        }

        private void RainbowBorder_UpdateHue(object sender, MouseEventArgs e)
        {
            if (!isMouseDownOnRainbow) return;

            var thisRainbowBorder = (FrameworkElement) sender;
            Point mousePosInRainbow = e.GetPosition(thisRainbowBorder);
            if ((mousePosInRainbow.Y >= rainbowBorder.ActualHeight) || (mousePosInRainbow.Y <= 0)) return;

            UpdateColorCanvas(thisRainbowBorder.ActualHeight,
                              (thisRainbowBorder.ActualHeight - mousePosInRainbow.Y));
            Canvas.SetTop(rainbowHandle, (mousePosInRainbow.Y - (rainbowHandle.ActualHeight/2)));
        }

        private void HueCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RealignElement(new Point(0.0, Canvas.GetTop(rainbowHandle)), e.PreviousSize, e.NewSize, rainbowHandle);

            if (rainbowBorder.ActualWidth > 1)
            {
                rainbowHandle.Width = rainbowBorder.ActualWidth - 1;
            }

            if (e.NewSize.Height < 100)
            {
                rainbowHandle.Height = 10;
            }
            else if (e.NewSize.Height > 300)
            {
                rainbowHandle.Height = 30;
            }
            else
            {
                rainbowHandle.Height = e.NewSize.Height/10;
            }
        }

        private void opacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            alpha = Convert.ToByte(e.NewValue);
            UpdateSelectedColor();
        }

        #endregion
    }
}