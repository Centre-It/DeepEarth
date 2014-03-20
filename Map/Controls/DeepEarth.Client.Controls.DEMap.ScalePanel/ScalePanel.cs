using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DeepEarth.Client.MapControl;

namespace DeepEarth.Client.Controls.DEMap
{
    [TemplatePart(Name = PART_scaleTextBlock, Type = typeof (TextBlock))]
    [TemplateVisualState(Name = VSM_Normal, GroupName = VSM_CommonStates)]
    [TemplateVisualState(Name = VSM_MouseOver, GroupName = VSM_CommonStates)]
    public class ScalePanel : Control, IMapControl<Map>
    {
        private const string PART_scaleTextBlock = "PART_scaleTextBlock";
        private const string VSM_CommonStates = "CommonStates";
        private const string VSM_MouseOver = "MouseOver";
        private const string VSM_Normal = "Normal";
        private bool isMouseOver;

        private Map mapInstance;

        private string mapName;
        private TextBlock scaleTextBlock;

        public ScalePanel()
        {
            IsEnabled = false;
            DefaultStyleKey = typeof (ScalePanel);
            Loaded += ScalePanel_Loaded;
        }

        #region IMapControl<Map> Members

        public string MapName
        {
            get { return mapName; }
            set
            {
                mapName = value;
                setMapInstance(MapName);
            }
        }

        public Map MapInstance
        {
            get { return mapInstance; }
            set
            {
                if (mapInstance != null)
                {
                    mapInstance.Events.MapViewChanged -= map_ViewChange;
                }
                mapInstance = value;
                if (mapInstance != null)
                {
                    mapInstance.Events.MapViewChanged += map_ViewChange;
                }
            }
        }

        public void Dispose()
        {
            MapInstance = null;
            MouseEnter -= mouseEnter;
            MouseLeave -= mouseLeave;
        }

        #endregion

        private void ScalePanel_Loaded(object sender, RoutedEventArgs e)
        {
            setMapInstance(MapName);
            MouseEnter += mouseEnter;
            MouseLeave += mouseLeave;
            GoToState(true);
        }


        private void setMapInstance(string mapname)
        {
            MapInstance = Utilities.FindVisualChildByName<Map>(Application.Current.RootVisual, mapname);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            scaleTextBlock = (TextBlock) GetTemplateChild(PART_scaleTextBlock);
            IsEnabled = true;

            //set inital values
            setMapScale();
        }

        private void map_ViewChange(object sender, EventArgs e)
        {
            if (IsEnabled)
            {
                setMapScale();
            }
        }

        private void mouseLeave(object sender, MouseEventArgs e)
        {
            isMouseOver = false;
            if (IsEnabled)
            {
                GoToState(true);
            }
        }

        private void mouseEnter(object sender, MouseEventArgs e)
        {
            isMouseOver = true;
            if (IsEnabled)
            {
                GoToState(true);
            }
        }

        private void setMapScale()
        {
            scaleTextBlock.Text = "Scale 1:" +
                                  Math.Round(
                                      Utilities.GetScaleAtZoomLevel(MapInstance.GeoCenter.Y, MapInstance.ZoomLevel), 0);
        }

        private void GoToState(bool useTransitions)
        {
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