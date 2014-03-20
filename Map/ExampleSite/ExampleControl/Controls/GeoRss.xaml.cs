/// Deep Earth is a community project available under the Microsoft Public License (Ms-PL)
/// Code is provided as is and with no warrenty – Use at your own risk
/// View the project and the latest code at http://codeplex.com/deepearth/

using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Browser;
using System.Windows.Controls;
using DeepEarth.Client.MapControl;
using DeepEarth.Client.MapControl.Geometry;
using DeepEarth.Client.MapControl.Layers;
using DeepEarth.Client.Services.Bing;
using DeepEarth.Client.Services.GeoRss;

namespace ExampleControl.Controls
{
    public partial class GeoRss : ILayer
    {
        //sample layer
        public static string Token;
        private readonly Random _Rand = new Random(DateTime.Now.Second);
        private readonly List<int> _ZOrderBucket = new List<int> {0, 1, 2, 3, 4};        
        private Map _Map;
        private GeometryLayer georss_gl;        

        private GeoRssReader reader;

        public GeoRss()
        {
            // Required to initialize variables
            InitializeComponent();

            // Test for DesignTime for display in Blend
            if (HtmlPage.IsEnabled)
            {
                _Map = MapInstance;

                //MapInstance.Events.MapLoaded += (o, e) => ConfigureDevPins();
            }

            reader = new DeepEarth.Client.Services.GeoRss.GeoRssReader();
            reader.DownloadGeoRssCompleted += new DeepEarth.Client.Services.GeoRss.DownloadGeoRssCompletedEventHandler(reader_DownloadGeoRssCompleted);
            reader.DownloadGeoRssException += new DeepEarth.Client.Services.GeoRss.DownloadGeoRssExceptionEventHandler(reader_DownloadGeoRssException);
        }

        private void AddGeoRss_Click(object sender, RoutedEventArgs e)
        {
            if (!Uri.IsWellFormedUriString(GeoRssUri.Text, UriKind.Absolute))
            {
                MessageBox.Show("Invalid URI.");
                return;
            }

            reader.uri = new Uri(GeoRssUri.Text);            
            reader.ReadAsync();
            
            AddGeoRss.Content = "Clear/Add GeoRSS feed";
        }

        void reader_DownloadGeoRssException(Exception e)
        {
            MessageBox.Show("Error when fetching/reading GeoRss: " + e.Message);
        }

        void reader_DownloadGeoRssCompleted(List<GeoRssItem> items)
        {
            if (georss_gl != null)
                georss_gl.Clear();
            else
            {
                georss_gl = new GeometryLayer(MapInstance) { ID = "", UpdateMode = GeometryLayer.UpdateModes.ElementUpdate };
                MapInstance.Layers.Add(georss_gl);
            }

            AddGeoRssPinsToLayer(items, georss_gl);
        }

        public void AddGeoRssPinsToLayer(List<GeoRssItem> gi, GeometryLayer layer)
        {
            layer.Clear();

            for (int i = 0; i < gi.Count; i++)
            {
                var pin = new GeoRssPin(gi[i]);
                layer.Add(pin);
                Canvas.SetZIndex(pin, i);
            }
        }

        public Orientation Orientation
        {
            get { return _OrientationPanel.Orientation; }
            set { _OrientationPanel.Orientation = value; }
        }

        #region ILayer APIs

        public string ID { get; set; }
        public bool IsVisible { get; set; }

        public Map MapInstance
        {
            get
            {
                if (_Map == null) _Map = Map.GetMapInstance(this);
                return _Map;
            }
            set
            {
                if (ReferenceEquals(_Map, value))
                {
                    return;
                }

                _Map = value;
            }
        }

        #endregion                 
    }
}