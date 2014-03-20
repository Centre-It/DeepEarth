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
using System.Runtime.Serialization;
using System.IO;
using System.Xml;
using DeepEarth.BingMapsToolkit.Client.Common;

namespace ExampleControlBing.ControlDemos
{
    public partial class SnapshotExample : UserControl
    {
        private Dictionary<string, ObservableCollection<VectorLayerData>> vectorData = new Dictionary<string, ObservableCollection<VectorLayerData>>();

        public SnapshotExample()
        {
            InitializeComponent();
            Loaded += LayerPanelExample_Loaded;
            snapshot.SnapshotLoaded += new EventHandler<OnSnapshotLoadedArgs<PersistedSettings>>(PersistedCommands_SnapshotLoaded);
            snapshot.SnapshotSave += new EventHandler<OnSnapshotSaveArgs<PersistedSettings>>(PersistedCommands_SnapshotSave);
        }

        static int count = 1;

        void PersistedCommands_SnapshotSave(object sender, OnSnapshotSaveArgs<PersistedSettings> e)
        {
            IsolatedStorage.SaveData<PersistedSettings>(e.Data, "SnapshotExampleStorageID=" + count.ToString());
            e.OnSnapshotSaveComplete(Application.Current.Host.Source.Host + "?" + Snapshot.DEurl + "=" + count.ToString());
            count++;
        }

        void PersistedCommands_SnapshotLoaded(object sender, OnSnapshotLoadedArgs<PersistedSettings> e)
        {
            var serializer = new DataContractSerializer(typeof(PersistedSettings));
            //string maml = "<Root> <Pause time='2000' /> <ZoomToXY value='-40,-40'/> <Pause time='1000' /> <ZoomToXY value='-30,-30'/> <Pause time='1000' /> <ZoomToXY value='-20,-20'/> </Root>";
            e.OnSnapshotLoadedComplete(IsolatedStorage.LoadData<PersistedSettings>("SnapshotExampleStorageID=" + e.ID), null);
        }

        private void LayerPanelExample_Loaded(object sender, RoutedEventArgs e)
        {
            var styles = new Dictionary<string, StyleSpecification>();
            styles.Add("defaultstyle", new StyleSpecification
            {
                ID = "style 1",
                LineColour = "FFFFFFFF",
                LineWidth = 6,
                PolyFillColour = "88677E1E",
                PolygonLineColour = "FF440044",
                PolygonLineWidth = 3,
                ShowFill = true,
                ShowLine = true,
                IconURL = "http://soulsolutions.com.au/Images/pin.png",
                IconScale = 2
            });

            styles.Add("style 2", new StyleSpecification
            {
                ID = "style 2",
                LineColour = "FFFF0000",
                LineWidth = 6,
                PolyFillColour = "88677E1E",
                PolygonLineColour = "FF440044",
                PolygonLineWidth = 3,
                ShowFill = true,
                ShowLine = true,
                IconURL = "http://www.iconarchive.com/icons/pixelmixer/basic-2/64/key-icon.png",
                IconScale = 2
            });

            var layers = new ObservableCollection<LayerDefinition>();
            layers.Add(new LayerDefinition
            {
                CurrentVersion = DateTime.Now,
                IsEditable = false,
                LabelOn = false,
                LayerAlias = "First Sample Layer",
                LayerID = "1",
                LayerStyleName = "style 2",
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
                Style = styles["style 2"],
                ObjectAttributes = new Dictionary<int, LayerObjectAttributeDefinition>()

            });

            layers.Add(new LayerDefinition
            {
                CurrentVersion = DateTime.Now,
                IsEditable = false,
                LabelOn = false,
                LayerAlias = "Second Sample Layer",
                LayerID = "2",
                LayerStyleName = "style 1",
                LayerTimeout = -1,
                LayerType = 1,
                MaxDisplayLevel = 100,
                MBR = new byte[0],
                MinDisplayLevel = 1,
                PermissionToEdit = false,
                Selected = false,
                Tags = "Test Group",
                ZIndex = 30,
                Temporal = true,
                IconURI = "http://soulsolutions.com.au/Images/pin.png",
                Style = styles["defaultstyle"],
                ObjectAttributes = new Dictionary<int, LayerObjectAttributeDefinition>()
            });

            ObservableCollection<VectorLayerData> data1 = new ObservableCollection<VectorLayerData>();
            List<GeoAPI.Geometries.ICoordinate> coord = new List<GeoAPI.Geometries.ICoordinate>();
            for (double x = -50; x <= 0; x += 1)
            {
                var point = new Point(x, x);
                coord.Add(point.Coordinate);

                data1.Add(new VectorLayerData
                {
                    Geo = point.AsBinary(),
                    ID = x.ToString(),
                    Label = x.ToString()
                });
            }

            var line = new LineString(coord.ToArray());

            data1.Add(new VectorLayerData
            {
                Geo = line.AsBinary(),
                ID = "Line 1",
                Label = "Line 2"
            });
            vectorData["1"] = data1;

            ObservableCollection<VectorLayerData> data2 = new ObservableCollection<VectorLayerData>();
            List<GeoAPI.Geometries.ICoordinate> coord2 = new List<GeoAPI.Geometries.ICoordinate>();
            for (double x = 0; x <= 50; x += 1)
            {
                var point = new Point(x, x);
                coord2.Add(point.Coordinate);
                data2.Add(new VectorLayerData
                {
                    Geo = point.AsBinary(),
                    ID = x.ToString(),
                    Label = x.ToString()
                    //TimeStamp = DateTime.Now.AddHours(x),
                });
            }
            coord2.Add(new Coordinate(50, 10));
            coord2.Add(coord2[0]);
            var line2 = new LinearRing(coord2.ToArray());
            Polygon poly = new Polygon(line2);
            data2.Add(new VectorLayerData
            {
                Geo = poly.AsBinary(),
                ID = "Line 2",
                Label = "Line 2"
            });

            vectorData["2"] = data2;

            layerPanel.Styles = styles;
            layerPanel.EnableBalloon = true;

            layerPanel.LoadLayerData += layerPanel_LoadLayer;
            layerPanel.BalloonLaunch += layerPanel_BalloonLaunch;

            layerPanel.Layers = layers;

            layerPanel.OnGoToLayer += layerPanel_OnGoToLayer;
            layerPanel.OnGoToFeature += new EventHandler<AccordionLayerPanelGotoFeatureEventArgs>(layerPanel_OnGoToFeature);
            layerPanel.OnVectorDataRequest += new EventHandler<VectorDataRequestEventArgs>(layerPanel_OnVectorDataRequest);
        }

        void layerPanel_OnVectorDataRequest(object sender, VectorDataRequestEventArgs e)
        {
            List<GeoVectorData> data = new List<GeoVectorData>();

            foreach (var item in vectorData[e.LayerID])
            {
                data.Add(new GeoVectorData
                {
                    Geo = item.Geo,
                    ID = item.ID,
                    Additional = item.ID
                    //TimeStamp = DateTime.Now.AddHours(x),
                });
            }
            e.OnVectorDataRequestEnd(data);
        }

        void layerPanel_OnGoToFeature(object sender, AccordionLayerPanelGotoFeatureEventArgs e)
        {
            if (vectorData.ContainsKey(e.LayerID))
            {
                var wkbReader = new WKBReader(new GeometryFactory(new PrecisionModel(), 4326));

                map.SetView(
                    new LocationRect(
                        ((IEnumerable<VectorLayerData>)vectorData[e.LayerID])
                        .Where(u => u.ID == e.ItemID)
                        .Select(d => wkbReader.Read(d.Geo))
                        .SelectMany(u => u.Coordinates.Select(k => CoordinateConvertor.ConvertBack(k))
                        .Select(s => new Microsoft.Maps.MapControl.Location() { Longitude = s.Longitude, Latitude = s.Latitude })).ToList()));
            }
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
            string xamlContent = "<Grid xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" Width=\"300\" Height=\"200\"><TextBlock xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" Foreground=\"White\" Text=\"Your Ballon xaml here\" /></Grid>";
            UIElement element = (UIElement)XamlReader.Load(xamlContent);

            ((InfoGeometry)sender).BalloonData.Add(element);
        }

        private void layerPanel_LoadLayer(object sender, LoadLayerEventArgs args,
                                          Action<ObservableCollection<VectorLayerData>, LayerDefinition> callback)
        {
            callback(vectorData[args.Layer.LayerID], args.Layer);
        }
    }
}
