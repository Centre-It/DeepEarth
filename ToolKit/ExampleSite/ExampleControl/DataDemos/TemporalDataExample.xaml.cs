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
using Point = GisSharpBlog.NetTopologySuite.Geometries.Point;

namespace ExampleControlBing.DataDemos
{
    public partial class TemporalDataExample
    {
        public TemporalDataExample()
        {
            InitializeComponent();
            dateRangeDisplayPanel.DateRangeSlider = dateRangeSlider;
            Loaded += TemporalDataExample_Loaded;
        }

        void TemporalDataExample_Loaded(object sender, RoutedEventArgs e)
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

            dateRangeSlider.RangeChanged += dateRangeSlider_RangeChanged;
            dateRangeDisplayPanel.ValueChangedThrottled += dateRangeDisplayPanel_ValueChangedThrottled;
        }

        void dateRangeDisplayPanel_ValueChangedThrottled(object sender, EventArgs e)
        {
            layerPanel.DateRangeLoaded = new DateRange
            {
                ValueLow = dateRangeSlider.Minimum,
                ValueHigh = dateRangeSlider.Maximum
            };
        }

        void dateRangeSlider_RangeChanged(object sender, EventArgs e)
        {
            layerPanel.DateRangeDisplay = new DateRange
                                              {
                                                  ValueLow = dateRangeSlider.RangeStart,
                                                  ValueHigh = dateRangeSlider.RangeEnd
                                              };
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
            for (double x = -50; x <= 50; x += 1)
            {
                var point = new Point(x, x);
                data.Add(new VectorLayerData
                {
                    Geo = point.AsBinary(),
                    ID = x.ToString(),
                    Label = x.ToString(),
                    TimeStamp = DateTime.Now.AddHours(x),
                });
            }

            callback(data, args.Layer);
        }
    }
}
