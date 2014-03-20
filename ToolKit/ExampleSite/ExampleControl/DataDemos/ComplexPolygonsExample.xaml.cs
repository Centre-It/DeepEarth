﻿// Deep Earth is a community project available under the Microsoft Public License (Ms-PL)
// Code is provided as is and with no warrenty – Use at your own risk
// View the project and the latest code at http://DeepEarth.codeplex.com/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DeepEarth.BingMapsToolkit.Client.Common.Entities;
using DeepEarth.BingMapsToolkit.Client.Controls;
using DeepEarth.BingMapsToolkit.Client.MapGeometry;
using DeepEarth.BingMapsToolkit.Common.Entities;
using GisSharpBlog.NetTopologySuite.Geometries;
using Point = GisSharpBlog.NetTopologySuite.Geometries.Point;

namespace ExampleControlBing.DataDemos
{
    public partial class ComplexPolygonsExample
    {
        public ComplexPolygonsExample()
        {
            InitializeComponent();
            Loaded += ComplexPolygonsExample_Loaded;
        }

        void ComplexPolygonsExample_Loaded(object sender, RoutedEventArgs e)
        {
            var styles = new Dictionary<string, StyleSpecification>();
            styles.Add("defaultstyle", new StyleSpecification
            {
                ID = "style 1",
                LineColour = "FF1B0AA5",
                LineWidth = 2,
                PolyFillColour = "88677E1E",
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
                LabelOn = true,
                LayerAlias = "Sample Layer",
                LayerID = "1",
                LayerStyleName = "style 3",
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
                ObjectAttributes = new Dictionary<int, LayerObjectAttributeDefinition>()
            });

            layerPanel.Styles = styles;
            layerPanel.EnableBalloon = true;

            layerPanel.LoadLayerData += layerPanel_LoadLayer;
            layerPanel.BalloonLaunch += layerPanel_BalloonLaunch;

            layerPanel.Layers = layers;
        }

        private void layerPanel_BalloonLaunch(object sender, BalloonEventArgs args)
        {
            //get balloon data for item (on demand), eg make a database call here.
            var grid = new Grid { Width = 300, Height = 200 };
            var textblock = new TextBlock { Foreground = new SolidColorBrush(Colors.White), Text = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") };
            grid.Children.Add(textblock);
            ((InfoGeometry)sender).BalloonData.Add(grid);
        }

        private void layerPanel_LoadLayer(object sender, LoadLayerEventArgs args,
                                          Action<ObservableCollection<VectorLayerData>, LayerDefinition> callback)
        {
            //get layer data for layer
            var data = new ObservableCollection<VectorLayerData>();

            //add a 11 detailed circles, 3600 nodes.
            for (double y = -50; y <= 50; y = y + 10)
            {
                var locs = new Coordinate[3601];
                double R = 6371; // earth's mean radius in km
                double lat = (y * Math.PI) / 180; //rad
                double lon = (-y * Math.PI) / 180; //rad
                double d = 500 / R; // d = angular distance covered on earth's surface
                for (int x = 0; x <= 3600; x++)
                {
                    double brng = (((double)x)/10) * Math.PI / 180; //rad
                    double latitude = Math.Asin(Math.Sin(lat) * Math.Cos(d) + Math.Cos(lat) * Math.Sin(d) * Math.Cos(brng));
                    double longitude = ((lon +
                                         Math.Atan2(Math.Sin(brng) * Math.Sin(d) * Math.Cos(lat),
                                                    Math.Cos(d) - Math.Sin(lat) * Math.Sin(latitude))) * 180) / Math.PI;
                    latitude = (latitude * 180) / Math.PI;
                    locs[x] = new Coordinate(longitude, latitude);
                }
                locs[3600] = locs[0];

                var polygon = new Polygon(new LinearRing(locs));
                data.Add(new VectorLayerData
                {
                    Geo = polygon.AsBinary(),
                    ID = y.ToString(),
                    Label = y.ToString(),
                    //TimeStamp = DateTime.Now.AddHours(x),
                });
            }

            callback(data, args.Layer);
        }
    }
}
