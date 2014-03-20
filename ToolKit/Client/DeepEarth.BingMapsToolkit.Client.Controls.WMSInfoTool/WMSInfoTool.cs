using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using DeepEarth.BingMapsToolkit.Client.Common;
using Microsoft.Maps.MapControl.Core;
using Microsoft.Maps.MapControl;
using System.IO;
using System.Xml.Linq;
using System.Windows.Markup;
using System.Text.RegularExpressions;
using System.Windows.Browser;
using System.ServiceModel;
using System.Collections.ObjectModel;

namespace DeepEarth.BingMapsToolkit.Client.Controls
{
    [TemplatePart(Name = PART_requestResultGrid, Type = typeof(Grid))]
    [TemplateVisualState(Name = VSM_Normal, GroupName = VSM_CommonStates)]
    [TemplateVisualState(Name = VSM_MouseOver, GroupName = VSM_CommonStates)]
    public class WMSInfoTool : Control, IMapControl<MapCore>
    {
        private const string VSM_CommonStates = "CommonStates";
        private const string VSM_MouseOver = "MouseOver";
        private const string VSM_Normal = "Normal";
        private const string PART_requestResultGrid = "PART_requestResultGrid";

        private Grid requestResultGrid;

        private MapCore mapInstance;
        private string mapName;

        private const string stylePath = "/DeepEarth.BingMapsToolkit.Client.Common;component/Resources/CommonStyles.xaml";

        private List<string> currentActiveLayers = new List<string>();

        private bool isMouseOver;

        public event EventHandler<WMSInfoToolEventArgs> OnWMSInfoToolRequest = delegate(object s, WMSInfoToolEventArgs e) { ;};

        public WMSInfoTool()
        {
            this.DefaultStyleKey = typeof(WMSInfoTool);

            Loaded += new RoutedEventHandler(WMSInfoTool_Loaded);
        }

        void WMSInfoTool_Loaded(object sender, RoutedEventArgs e)
        {
            MouseEnter += mouseEnter;
            MouseLeave += mouseLeave;
            setMapInstance(MapName);
            LoadMergedDictionary();
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

        private void LoadMergedDictionary()
        {
            XDocument xaml = XDocument.Load(stylePath);
            ResourceDictionary rd = XamlReader.Load(xaml.ToString()) as ResourceDictionary;
            Application.Current.Resources.MergedDictionaries.Add(rd);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            requestResultGrid = (Grid)GetTemplateChild(PART_requestResultGrid);
        }

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

        public MapCore MapInstance
        {
            get { return mapInstance; }
            set
            {
                if (mapInstance != null)
                {
                    mapInstance.MouseClick -= mapInstance_MouseClick;
                }
                mapInstance = value;
                if (mapInstance != null)
                {
                    mapInstance.MouseClick += mapInstance_MouseClick;
                }
            }
        }

        private void setMapInstance(string mapname)
        {
            MapInstance = Utilities.FindVisualChildByName<Map>(Application.Current.RootVisual, mapname);
        }

        public void Dispose()
        {
            MouseEnter -= mouseEnter;
            MouseLeave -= mouseLeave;
            MapInstance = null;
        }

        #endregion

        void mapInstance_MouseClick(object sender, Microsoft.Maps.MapControl.MapMouseEventArgs e)
        {
            WMSInfoToolEventArgs args = new WMSInfoToolEventArgs();

            args.Minx = Math.Min(mapInstance.TargetBoundingRectangle.West, mapInstance.TargetBoundingRectangle.East);
            args.Maxx = Math.Max(mapInstance.TargetBoundingRectangle.West, mapInstance.TargetBoundingRectangle.East);
            args.Miny = Math.Min(mapInstance.TargetBoundingRectangle.North, mapInstance.TargetBoundingRectangle.South);
            args.Maxy = Math.Max(mapInstance.TargetBoundingRectangle.North, mapInstance.TargetBoundingRectangle.South);

            args.X = e.ViewportPoint.X;
            args.Y = e.ViewportPoint.Y;

            args.Width = mapInstance.ActualWidth;
            args.Height = mapInstance.ActualHeight;

            args.OnWMSInfoToolRequestComplete = serviceClient_WMSFeatureRequestCompleted;

            OnWMSInfoToolRequest(this, args);
        }

        void serviceClient_WMSFeatureRequestCompleted(IEnumerable<WMSFeature> result)
        {

            requestResultGrid.Children.Clear();

            int row = 0;
            foreach (var item in result)
            {
                CreateResultTextBlock(row, item, 0, item.Layer);
                CreateResultTextBlock(row, item, 1, item.ID);
                CreateResultTextBlock(row, item, 2, item.Description);
                CreateResultTextBlock(row, item, 3, item.Value);
                ++row;
            }
        }

        private void CreateResultTextBlock(int row, WMSFeature item, int i, string data)
        {
            TextBlock text = new TextBlock();
            text.Text = data;
            text.Foreground = new SolidColorBrush(Colors.White);
            text.SetValue(Grid.ColumnProperty, i);
            text.Margin = new Thickness(0, (row) * text.ActualHeight, 0, 0);

            requestResultGrid.Children.Add(text);
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
