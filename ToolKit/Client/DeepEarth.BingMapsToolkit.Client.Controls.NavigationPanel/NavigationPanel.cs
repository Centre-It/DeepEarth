// Deep Earth is a community project available under the Microsoft Public License (Ms-PL)
// Code is provided as is and with no warrenty – Use at your own risk
// View the project and the latest code at http://DeepEarth.codeplex.com/

using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using DeepEarth.BingMapsToolkit.Client.Common;
using Microsoft.Maps.MapControl.Core;

namespace DeepEarth.BingMapsToolkit.Client.Controls
{
    [TemplatePart(Name = PART_horizontalSplineDoubleKeyFrame, Type = typeof (SplineDoubleKeyFrame))]
    [TemplatePart(Name = PART_borderHorizontalToolbar, Type = typeof (Border))]
    [TemplatePart(Name = PART_rightButton, Type = typeof (RepeatButton))]
    [TemplatePart(Name = PART_downButton, Type = typeof (RepeatButton))]
    [TemplatePart(Name = PART_leftButton, Type = typeof (RepeatButton))]
    [TemplatePart(Name = PART_upButton, Type = typeof (RepeatButton))]
    [TemplatePart(Name = PART_spinButton, Type = typeof (Button))]
    [TemplatePart(Name = PART_zoomInButton, Type = typeof (RepeatButton))]
    [TemplatePart(Name = PART_zoomOutButton, Type = typeof (RepeatButton))]
    [TemplatePart(Name = PART_NavigationGrid, Type = typeof(Panel))]
    [TemplateVisualState(Name = VSM_Expanded, GroupName = VSM_DashboardStates)]
    [TemplateVisualState(Name = VSM_Collapsed, GroupName = VSM_DashboardStates)]
    [TemplateVisualState(Name = VSM_Normal, GroupName = VSM_CommonStates)]
    [TemplateVisualState(Name = VSM_MouseOver, GroupName = VSM_CommonStates)]
    public class NavigationPanel : ContentControl, IMapControl<MapCore>
    {
        private const int panAmount = 250;
        private const string PART_borderHorizontalToolbar = "PART_borderHorizontalToolbar";
        private const string PART_downButton = "PART_downButton";
        private const string PART_horizontalSplineDoubleKeyFrame = "PART_HorizontalSplineDoubleKeyFrame";
        private const string PART_leftButton = "PART_leftButton";
        private const string PART_rightButton = "PART_rightButton";
        private const string PART_spinButton = "PART_spinButton";
        private const string PART_upButton = "PART_upButton";
        private const string PART_zoomInButton = "PART_zoomInButton";
        private const string PART_zoomOutButton = "PART_zoomOutButton";
        private const string PART_NavigationGrid = "PART_NavigationGrid";
        private const string VSM_Collapsed = "Collapsed";
        private const string VSM_CommonStates = "CommonStates";
        private const string VSM_DashboardStates = "DashboardStates";
        private const string VSM_Expanded = "Expanded";
        private const string VSM_MouseOver = "MouseOver";
        private const string VSM_Normal = "Normal";

        public static readonly DependencyProperty HorizontalToolBarLeftProperty =
            DependencyProperty.Register("HorizontalToolBarLeft",
                                        typeof (double),
                                        typeof (NavigationPanel),
                                        null);

        public static readonly DependencyProperty NavCircleScaleProperty =
            DependencyProperty.Register("NavCircleScale",
                                        typeof (double),
                                        typeof (NavigationPanel),
                                        null);

        public static readonly DependencyProperty VerticalToolBarTopProperty =
            DependencyProperty.Register("VerticalToolBarTop",
                                        typeof (double),
                                        typeof (NavigationPanel),
                                        null);

        private Border borderHorizontalToolbar;
        private RepeatButton downButton;
        private SplineDoubleKeyFrame horizontalSplineDoubleKeyFrame;
        private bool isExpanded = true;
        private bool isMouseOver;
        private RepeatButton leftButton;
        private string mapName;
        private bool panelDimensionsSet;
        private RepeatButton rightButton;
        private Button spinButton;
        private RepeatButton upButton;
        private RepeatButton zoomInButton;
        private RepeatButton zoomOutButton;
        private Panel navigationGrid;

        public NavigationPanel()
        {
            DefaultStyleKey = typeof (NavigationPanel);
            Loaded += NavigationPanel_Loaded;
        }

        public double HorizontalToolBarLeft
        {
            get { return (double) GetValue(HorizontalToolBarLeftProperty); }
            set { SetValue(HorizontalToolBarLeftProperty, value); }
        }

        public double VerticalToolBarTop
        {
            get { return (double) GetValue(VerticalToolBarTopProperty); }
            set { SetValue(VerticalToolBarTopProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TheName.  
        // This enables animation, styling, binding, etc...

        public double NavCircleScale
        {
            get { return (double) GetValue(NavCircleScaleProperty); }
            set
            {
                SetValue(NavCircleScaleProperty, value);
                //binding doesn't work for some reason, set it manually
                if (navigationGrid != null)
                {
                    var transformGroup = navigationGrid.RenderTransform as TransformGroup;
                    if (transformGroup != null)
                    {
                        var scaleTransform = (ScaleTransform)transformGroup.Children[0];
                        scaleTransform.ScaleX = value;
                        scaleTransform.ScaleY = value;
                    }
                }
            }
        }

        // Using a DependencyProperty as the backing store for TheName.  
        // This enables animation, styling, binding, etc...

        #region IMapControl<MapCore> Members

        public string MapName
        {
            get { return mapName; }
            set
            {
                mapName = value;
                setMapInstance(MapName);
            }
        }

        public MapCore MapInstance { get; set; }

        public void Dispose()
        {
            MouseEnter -= NavigationPanel_MouseEnter;
            MouseLeave -= NavigationPanel_MouseLeave;
            if (IsEnabled)
            {
                rightButton.Click -= rightButton_Click;
                downButton.Click -= downButton_Click;
                leftButton.Click -= leftButton_Click;
                upButton.Click -= upButton_Click;
                spinButton.Click -= spinButton_Click;
                zoomOutButton.Click -= zoomOutButton_Click;
                zoomInButton.Click -= zoomInButton_Click;
            }
        }

        #endregion

        private void setMapInstance(string mapname)
        {
            MapInstance = Utilities.FindVisualChildByName<MapCore>(Application.Current.RootVisual, mapname);
        }

        private void NavigationPanel_Loaded(object sender, RoutedEventArgs e)
        {
            setMapInstance(MapName);
            MouseEnter += NavigationPanel_MouseEnter;
            MouseLeave += NavigationPanel_MouseLeave;
            GoToState(true);
        }

        private void NavigationPanel_MouseLeave(object sender, MouseEventArgs e)
        {
            isMouseOver = false;
            GoToState(true);
        }

        private void NavigationPanel_MouseEnter(object sender, MouseEventArgs e)
        {
            isMouseOver = true;
            GoToState(true);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            rightButton = (RepeatButton) GetTemplateChild(PART_rightButton);
            downButton = (RepeatButton) GetTemplateChild(PART_downButton);
            leftButton = (RepeatButton) GetTemplateChild(PART_leftButton);
            upButton = (RepeatButton) GetTemplateChild(PART_upButton);
            spinButton = (Button) GetTemplateChild(PART_spinButton);
            zoomOutButton = (RepeatButton) GetTemplateChild(PART_zoomOutButton);
            zoomInButton = (RepeatButton) GetTemplateChild(PART_zoomInButton);
            horizontalSplineDoubleKeyFrame =
                (SplineDoubleKeyFrame) GetTemplateChild(PART_horizontalSplineDoubleKeyFrame);
            borderHorizontalToolbar = (Border) GetTemplateChild(PART_borderHorizontalToolbar);
            navigationGrid = (Panel) GetTemplateChild(PART_NavigationGrid);

            NavCircleScale = NavCircleScale;

            //events
            rightButton.Click += rightButton_Click;
            downButton.Click += downButton_Click;
            leftButton.Click += leftButton_Click;
            upButton.Click += upButton_Click;
            spinButton.Click += spinButton_Click;
            zoomOutButton.Click += zoomOutButton_Click;
            zoomInButton.Click += zoomInButton_Click;
            IsEnabled = true;
        }

        private void zoomInButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsEnabled)
            {
                zoomMap(1);
            }
        }

        private void zoomOutButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsEnabled)
            {
                zoomMap(-1);
            }
        }

        private void spinButton_Click(object sender, RoutedEventArgs e)
        {
            isExpanded = !isExpanded;
            GoToState(true);
        }

        private void upButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsEnabled)
            {
                PanMap(0, -panAmount);
            }
        }

        private void leftButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsEnabled)
            {
                PanMap(-panAmount, 0);
            }
        }

        private void downButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsEnabled)
            {
                PanMap(0, panAmount);
            }
        }

        private void rightButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsEnabled)
            {
                PanMap(panAmount, 0);
            }
        }

        private void PanMap(int deltaX, int deltaY)
        {
            if (MapInstance != null)
            {
                Utilities.Pan(deltaX, deltaY, MapInstance);
            }
        }

        private void zoomMap(int delta)
        {
            MapInstance.ZoomLevel = MapInstance.ZoomLevel + delta;
        }

        private void GoToState(bool useTransitions)
        {
            if (!panelDimensionsSet && borderHorizontalToolbar != null)
            {
                borderHorizontalToolbar.Width = borderHorizontalToolbar.ActualWidth;
                horizontalSplineDoubleKeyFrame.Value = borderHorizontalToolbar.ActualWidth;
                panelDimensionsSet = true;
            }

            if (isExpanded)
            {
                VisualStateManager.GoToState(this, VSM_Expanded, useTransitions);
            }
            else
            {
                VisualStateManager.GoToState(this, VSM_Collapsed, useTransitions);
            }

            if (isMouseOver)
            {
                VisualStateManager.GoToState(this, VSM_MouseOver, useTransitions);
            }
            else
            {
                VisualStateManager.GoToState(this, VSM_Normal, useTransitions);
            }
        }
    }
}