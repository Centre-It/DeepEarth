// Deep Earth is a community project available under the Microsoft Public License (Ms-PL)
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
    public partial class SinglePoint
    {
        private EnhancedMapLayer layer;
        public SinglePoint()
        {
            InitializeComponent();
            Loaded += SinglePoint_Loaded;
        }

        void SinglePoint_Loaded(object sender, RoutedEventArgs e)
        {
            var styles = new Dictionary<string, StyleSpecification>();
            styles.Add("defaultstyle", new StyleSpecification
            {
                ID = "defaultstyle",
                LineColour = "FF1B0AA5",
                LineWidth = 2,
                PolyFillColour = "88677E1E",
                ShowFill = true,
                ShowLine = true,
                IconURL = "http://soulsolutions.com.au/Images/flag-yellow.png",
                IconScale = 2,
                IconOffsetX = 70, //35.41666666666667,
                IconOffsetY = -90 //-45.83333333333333
            });

            layer = new EnhancedMapLayer(map)
                        {
                            Styles = styles,
                            LayerDefinition = new LayerDefinition
                                                  {
                                                      CurrentVersion = DateTime.Now,
                                                      IsEditable = false,
                                                      LabelOn = true,
                                                      LayerAlias = "Sample Layer",
                                                      LayerID = "1",
                                                      LayerStyleName = "defaultstyle",
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
                                                      IconURI = "http://soulsolutions.com.au/Images/flag-yellow.png",
                                                      ObjectAttributes =
                                                          new Dictionary<int, LayerObjectAttributeDefinition>()
                                                  },
                              EnableBalloon = false,
                        };
            map.Children.Add(layer);

            //get layer data for layer
            var data = new ObservableCollection<VectorLayerData>();

            var point = new Point(153.0278, -27.4667);
            data.Add(new VectorLayerData
            {
                Geo = point.AsBinary(),
                ID = "Brisbane",
                Label = "Brisbane",
            });
            
            layer.Add(data);
        }
    }
}

