using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using DeepEarth.BingMapsToolkit.Client.Common;
using DeepEarth.BingMapsToolkit.Client.Common.Entities;

namespace DeepEarth.BingMapsToolkit.Client.Controls
{
    [TemplatePart(Name = PART_lineColor, Type = typeof(ToggleButton))]
    [TemplatePart(Name = PART_polygonLineColor, Type = typeof(ToggleButton))]
    [TemplatePart(Name = PART_fillColor, Type = typeof(ToggleButton))]
    [TemplatePart(Name = PART_colourPicker, Type = typeof(ColorPicker))]
    [TemplatePart(Name = PART_SaveButton, Type = typeof(Button))]
    [TemplatePart(Name = PART_CancelButton, Type = typeof(Button))]
    [TemplatePart(Name = PART_Icons, Type = typeof(ListBox))]
    [TemplateVisualState(Name = VSM_ShowColorPicker, GroupName = VSM_ColorPickerState)]
    [TemplateVisualState(Name = VSM_HideColorPicker, GroupName = VSM_ColorPickerState)]
    public class GeometryStylePicker : Control, IDisposable
    {
        private const string PART_lineColor = "PART_lineColor";
        private const string PART_polygonLineColor = "PART_polygonLineColor";
        private const string PART_fillColor = "PART_fillColor";
        private const string PART_colourPicker = "PART_colourPicker";
        private const string PART_SaveButton = "PART_SaveButton";
        private const string PART_CancelButton = "PART_CancelButton";
        private const string PART_Icons = "PART_Icons";
        private const string VSM_ColorPickerState = "ColorPickerState";
        private const string VSM_ShowColorPicker = "ShowColorPicker";
        private const string VSM_HideColorPicker = "HideColorPicker";

        private ToggleButton lineColor;
        private ToggleButton polygonLineColor;
        private ToggleButton fillColor;
        private ColorPicker colorPicker;
        private Button saveButton;
        private Button cancelButton;
        private ListBox iconsListBox;

        private bool isColourPickerVisible;

        private StyleSpecification selectedStyle;

        public event EventHandler Save;
        public event EventHandler Cancel;

        protected void OnSave()
        {
            if (Save != null)
                Save(this, new EventArgs());
        }

        protected void OnCancel()
        {
            if (Cancel != null)
                Cancel(this, new EventArgs());
        }

        public GeometryStylePicker()
        {
            IsEnabled = false;
            DefaultStyleKey = typeof(GeometryStylePicker);
        }

        public StyleSpecification SelectedStyle
        {
            get { return selectedStyle; }
            set
            {
                selectedStyle = value;
                DataContext = SelectedStyle;
            }
        }

        private IEnumerable icons;
        public IEnumerable Icons
        {
            get
            {
                if (iconsListBox != null)
                {
                    return iconsListBox.ItemsSource;
                }
                return icons;
            }
            set
            {
                icons = value;
                if (iconsListBox != null)
                {
                    iconsListBox.ItemsSource = icons;
                }
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            lineColor = (ToggleButton)GetTemplateChild(PART_lineColor);
            polygonLineColor = (ToggleButton) GetTemplateChild(PART_polygonLineColor);
            fillColor = (ToggleButton)GetTemplateChild(PART_fillColor);
            colorPicker = (ColorPicker)GetTemplateChild(PART_colourPicker);
            saveButton = (Button)GetTemplateChild(PART_SaveButton);
            cancelButton = (Button)GetTemplateChild(PART_CancelButton);
            iconsListBox = (ListBox) GetTemplateChild(PART_Icons);

            if (lineColor != null)
            {
                lineColor.Checked += (lineColor_Checked);
                lineColor.Unchecked += (lineColor_Unchecked);
            }

            if (polygonLineColor != null)
            {
                polygonLineColor.Checked += (polygonLineColor_Checked);
                polygonLineColor.Unchecked += (polygonLineColor_Unchecked);
            }

            if (fillColor != null)
            {
                fillColor.Checked += (fillColor_Checked);
                fillColor.Unchecked += (fillColor_Unchecked);
            }

            if (colorPicker != null)
            {
                colorPicker.ColorChanged += (colorPicker_ColorChanged);
            }

            if (saveButton != null)
            {
                saveButton.Click += saveButton_Click;
            }

            if (cancelButton != null)
            {
                cancelButton.Click += cancelButton_Click;
            }

            if (iconsListBox != null && icons !=null)
            {
                iconsListBox.ItemsSource = icons;
            }

            IsEnabled = true;
        }

        void saveButton_Click(object sender, RoutedEventArgs e)
        {
            OnSave();
        }

        void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            OnCancel();
        }

        void lineColor_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!fillColor.IsChecked.GetValueOrDefault() && !polygonLineColor.IsChecked.GetValueOrDefault())
            {
                isColourPickerVisible = false;
                goToState(true);
            }
        }

        void lineColor_Checked(object sender, RoutedEventArgs e)
        {
            fillColor.IsChecked = false;
            polygonLineColor.IsChecked = false;
            showColorPicker(new SolidColorBrush(Utilities.ColorFromHexString(SelectedStyle.LineColour)));
        }

        void polygonLineColor_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!lineColor.IsChecked.GetValueOrDefault() && !fillColor.IsChecked.GetValueOrDefault())
            {
                isColourPickerVisible = false;
                goToState(true);
            }
        }

        void polygonLineColor_Checked(object sender, RoutedEventArgs e)
        {
            lineColor.IsChecked = false;
            fillColor.IsChecked = false;
            showColorPicker(new SolidColorBrush(Utilities.ColorFromHexString(SelectedStyle.PolygonLineColour)));
        }

        void colorPicker_ColorChanged(object sender, ColorChangedEventArgs e)
        {
            if (lineColor.IsChecked.GetValueOrDefault())
            {
                SelectedStyle.LineColour = Utilities.ColorToHexString(e.newColor.Color);
            }
            else if (fillColor.IsChecked.GetValueOrDefault())
            {
                SelectedStyle.PolyFillColour = Utilities.ColorToHexString(e.newColor.Color);
            }
            else if (polygonLineColor.IsChecked.GetValueOrDefault())
            {
                SelectedStyle.PolygonLineColour = Utilities.ColorToHexString(e.newColor.Color);
            }
        }

        void fillColor_Checked(object sender, RoutedEventArgs e)
        {
            lineColor.IsChecked = false;
            polygonLineColor.IsChecked = false;
            showColorPicker(new SolidColorBrush(Utilities.ColorFromHexString(SelectedStyle.PolyFillColour)));
        }

        void fillColor_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!lineColor.IsChecked.GetValueOrDefault() && !polygonLineColor.IsChecked.GetValueOrDefault())
            {
                isColourPickerVisible = false;
                goToState(true);
            }

        }

        private void showColorPicker(SolidColorBrush selectedColor)
        {
            colorPicker.SelectedColor = selectedColor;
            isColourPickerVisible = true;
            goToState(true);
        }

        private void goToState(bool useTransitions)
        {
            if (isColourPickerVisible)
            {
                VisualStateManager.GoToState(this, VSM_ShowColorPicker, useTransitions);
            }
            else
            {
                VisualStateManager.GoToState(this, VSM_HideColorPicker, useTransitions);
            }
        }


        public void Dispose()
        {
            if (lineColor != null)
            {
                lineColor.Checked -= (lineColor_Checked);
                lineColor.Unchecked-= (lineColor_Unchecked);
            }

            if (fillColor != null)
            {
                fillColor.Checked -= (fillColor_Checked);
                fillColor.Unchecked -= (fillColor_Unchecked);
            }

            if (colorPicker != null )
            {
                colorPicker.ColorChanged -= (colorPicker_ColorChanged);
            }
        }
    }
}
