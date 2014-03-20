﻿// Deep Earth is a community project available under the Microsoft Public License (Ms-PL)
// Code is provided as is and with no warrenty – Use at your own risk
// View the project and the latest code at http://DeepEarth.codeplex.com/

using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DeepEarth.BingMapsToolkit.Client.Common.Entities;
using DeepEarth.BingMapsToolkit.Client.Controls;
using DeepEarth.BingMapsToolkit.Client.MapGeometry;
using DeepEarth.BingMapsToolkit.Common.Entities;
using Point = GisSharpBlog.NetTopologySuite.Geometries.Point;
using GisSharpBlog.NetTopologySuite.IO;
using GisSharpBlog.NetTopologySuite.Geometries;
using DeepEarth.BingMapsToolkit.Client.Common.Converters;
using Microsoft.Maps.MapControl;
using System.Windows.Markup;

namespace ExampleControlBing.DataDemos
{
    public partial class BalloonXamlExample
    {
        private Dictionary<string, ObservableCollection<VectorLayerData>> vectorData = new Dictionary<string, ObservableCollection<VectorLayerData>>();
        static Dictionary<string, string> imageSource = new Dictionary<string, string>();
        static Dictionary<string, string> description = new Dictionary<string, string>();

        public BalloonXamlExample()
        {
            InitializeComponent();
            Loaded += BalloonXamlExample_Loaded;
        }

        private void BalloonXamlExample_Loaded(object sender, RoutedEventArgs e)
        {
            imageSource.Add("1", "/ExampleControlBing;component/Images/Flag_of_Russia.png");
            description.Add("1", "http://en.wikipedia.org/wiki/Russia");

            imageSource.Add("2", "/ExampleControlBing;component/Images/Flag_of_England.png");
            description.Add("2", "http://en.wikipedia.org/wiki/England");

            imageSource.Add("4", "/ExampleControlBing;component/Images/Flag_of_Australia.png");
            description.Add("4", "http://en.wikipedia.org/wiki/Australia");

            var styles = new Dictionary<string, StyleSpecification>();
            styles.Add("defaultstyle", new StyleSpecification
            {
                ID = "style 1",
                LineColour = "FFFFFFFF",
                LineWidth = 2,
                PolyFillColour = "88677E1E",
                PolygonLineColour = "FF440044",
                PolygonLineWidth = 3,
                ShowFill = true,
                ShowLine = true,
                IconURL = "http://soulsolutions.com.au/Images/pin.png",
                IconScale = 2
            });

            var layers = new ObservableCollection<LayerDefinition>();
            layers.Add(new LayerDefinition
            {
                CurrentVersion = DateTime.Now,
                IsEditable = false,
                LabelOn = false,
                LayerAlias = "Countries",
                LayerID = "1",
                LayerStyleName = "style 1",
                LayerTimeout = -1,
                LayerType = 1,
                MaxDisplayLevel = 100,
                MBR = new byte[0],
                MinDisplayLevel = 1,
                PermissionToEdit = false,
                Selected = true,
                Tags = "Test Group",
                ZIndex = 30,
                Temporal = true,
                IconURI = "http://soulsolutions.com.au/Images/pin.png",
                Style = styles["defaultstyle"],
                ObjectAttributes = new Dictionary<int, LayerObjectAttributeDefinition>()

            });

            layerPanel.Styles = styles;
            layerPanel.EnableBalloon = true;

            layerPanel.LoadLayerData += layerPanel_LoadLayer;
            layerPanel.BalloonLaunch += layerPanel_BalloonLaunch;

            layerPanel.Layers = layers;

            layerPanel.OnGoToLayer += layerPanel_OnGoToLayer;
        }

        void layerPanel_OnGoToLayer(object sender, AccordionLayerPanelGotoEventArgs e)
        {
            if (vectorData.ContainsKey(e.LayerID))
            {
                var wkbReader = new WKBReader(new GeometryFactory(new PrecisionModel(), 4326));

                map.SetView(
                    new LocationRect(
                        ((IEnumerable<VectorLayerData>)vectorData[e.LayerID])
                        .Select(d => wkbReader.Read(d.Geo))
                        .SelectMany(u => u.Coordinates.Select(k => CoordinateConvertor.ConvertBack(k))
                        .Select(s => new Microsoft.Maps.MapControl.Location() { Longitude = s.Longitude, Latitude = s.Latitude })).ToList()));
            }
        }

        private void layerPanel_BalloonLaunch(object sender, BalloonEventArgs args)
        {
            string xamlContent = "<Grid xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" Width=\"230\" Height=\"153\">"
                + "<Image Margin=\"10,20,10,10\" Source=\"" + imageSource[args.ItemID] + "\"/>"
                + "<HyperlinkButton TargetName=\"_blank\"  NavigateUri=\"" + description[args.ItemID] + "\" Content=\"" + description[args.ItemID] + "\"/>"
                + "</Grid>";

            UIElement element = (UIElement)XamlReader.Load(xamlContent);

            ((InfoGeometry)sender).BalloonData.Add(element);
        }

        private void layerPanel_LoadLayer(object sender, LoadLayerEventArgs args,
                                          Action<ObservableCollection<VectorLayerData>, LayerDefinition> callback)
        {
            var data = new ObservableCollection<VectorLayerData>();

            Point rpoint = new Point(100, 67);
            data.Add(new VectorLayerData
            {
                Geo = rpoint.AsBinary(),
                ID = "1",
                Label = "Russia"
            });

            Point epoint = new Point(-3, 53);
            data.Add(new VectorLayerData
            {
                Geo = epoint.AsBinary(),
                ID = "2",
                Label = "England"
            });

            Point apoint = new Point(130, -25);
            data.Add(new VectorLayerData
            {
                Geo = apoint.AsBinary(),
                ID = "4",
                Label = "Australia"
            });

            vectorData[args.Layer.LayerID] = data;

            callback(data, args.Layer);
        }
    }
}