// Deep Earth is a community project available under the Microsoft Public License (Ms-PL)
// Code is provided as is and with no warrenty – Use at your own risk
// View the project and the latest code at http://DeepEarth.codeplex.com/

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using DeepEarth.BingMapsToolkit.Client.Common;
using DeepEarth.BingMapsToolkit.Client.Common.Converters;
using DeepEarth.BingMapsToolkit.Client.Common.Entities;
using DeepEarth.BingMapsToolkit.Common.Entities;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.IO;
using Microsoft.Maps.MapControl;
using Microsoft.Maps.MapControl.Core;

namespace DeepEarth.BingMapsToolkit.Client.Controls
{
    public delegate void TilePollingEventHandler(object sender, TilePollingEventArgs args);

    public class IdentifiableTileLayer : MapTileLayer, IEnhancedLayer
    {
        private LayerDefinition layerDefinition;
        private MapCore mapInstance;

        private LocationRect mbr;
        private bool firstCheck;
        private bool processOutstanding;
        private DispatcherTimer timer;
        private int zIndex;

        public IdentifiableTileLayer()
        {
            Loaded += IdentifiableTileLayer_Loaded;
            timer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 5) };
            timer.Tick += timer_Tick;
        }

        public LocationRect MBR
        {
            get { return mbr; }
        }

        public string TileServerPath { get; set; }

        public LayerDefinition LayerDefinition
        {
            get { return layerDefinition; }
            set
            {
                layerDefinition = value;
                //get MBR
                var wkbReader = new WKBReader(new GeometryFactory(new PrecisionModel(), 4326));
                IGeometry geom = wkbReader.Read(layerDefinition.MBR);
                mbr = new LocationRect(CoordinateConvertor.CoordinatesToLocationCollection(geom.Envelope.Coordinates));
                ID = layerDefinition.LayerID;
            }
        }

        public bool EnableTileRefreshPolling { get; set; }

        public MapCore MapInstance
        {
            get
            {
                if (mapInstance == null)
                {
                    MapInstance = Utilities.GetParentInstance<MapCore>(this);
                }
                return mapInstance;
            }
            set
            {
                if (mapInstance != null)
                {
                    mapInstance.ViewChangeStart -= mapInstance_ViewChangeStart;
                    mapInstance.ViewChangeEnd -= mapInstance_ViewChangeEnd;
                }
                mapInstance = value;
                if (mapInstance != null)
                {
                    mapInstance.ViewChangeStart += mapInstance_ViewChangeStart;
                    mapInstance.ViewChangeEnd += mapInstance_ViewChangeEnd;
                }
            }
        }

        #region IEnhancedLayer Members

        public string ID { get; set; }

        public int ZIndex
        {
            get { return zIndex; }
            set
            {
                zIndex = value;
                if (Parent != null)
                {
                    SetValue(Canvas.ZIndexProperty, zIndex);
                }
            }
        }

        public void Dispose()
        {
            TileSources.Clear();
            timer.Stop();
            timer = null;
            MapInstance = null;
        }

        #endregion

        private void addTileSource()
        {
            switch (layerDefinition.PublishMethod)
            {
                case 0:
                    var tileSource = new LocationRectTileSource
                    {
                        UriFormat = TileServerPath + "&layerid=" + layerDefinition.LayerID,
                        ZoomRange =
                            new Range<double>(layerDefinition.MinDisplayLevel,
                                              layerDefinition.MaxDisplayLevel),
                        BoundingRectangle = MBR
                    };

                    // Adds the tile overlay to the map layer

                    TileSources.Add(tileSource);
                    break;
                case 1:
                    var tileSourceXYZ = new XYZLocationRectTileSource
                    {
                        UriFormat = layerDefinition.LayerURI,
                        ZoomRange =
                            new Range<double>(layerDefinition.MinDisplayLevel,
                                              layerDefinition.MaxDisplayLevel),
                        BoundingRectangle = MBR
                    };

                    // Adds the tile overlay to the map layer
                    TileSources.Add(tileSourceXYZ);
                    break;
            }
        }

        private void startTimer()
        {
            timer.Stop();
            timer.Start();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            timer.Stop();
            if (processOutstanding)
            {
                //ask server if tiles outstanding
                OnTilePolling(new TilePollingEventArgs(LayerDefinition.LayerID, MapInstance.BoundingRectangle, (int)MapInstance.ZoomLevel));
            }
        }

        public event TilePollingEventHandler TilePolling;

        protected virtual void OnTilePolling(TilePollingEventArgs e)
        {
            if (TilePolling != null)
                TilePolling(this, e);
        }

        public void UpdateTilePolling(bool tilesProcessing)
        {
            if (processOutstanding)
            {
                if (tilesProcessing)
                {
                    startTimer();
                    firstCheck = false;
                }
                else if (!firstCheck)
                {
                    refreshTileSource();
                }
            }
        }

        private void refreshTileSource()
        {
            TileSources.Clear();
            addTileSource();
        }

        private void IdentifiableTileLayer_Loaded(object sender, RoutedEventArgs e)
        {
            addTileSource();
            if (Parent != null)
            {
                SetValue(Canvas.ZIndexProperty, zIndex);
            }
        }

        private void mapInstance_ViewChangeEnd(object sender, MapEventArgs e)
        {
            //only if mapview overlaps this layer
            if (EnableTileRefreshPolling && LayerDefinition.MinDisplayLevel <= MapInstance.ZoomLevel &&
                LayerDefinition.MaxDisplayLevel >= MapInstance.ZoomLevel &&
                MapInstance.BoundingRectangle.Intersects(MBR))
            {
                //ask server if tiles outstanding
                processOutstanding = true;
                firstCheck = true;
                OnTilePolling(new TilePollingEventArgs(LayerDefinition.LayerID, MapInstance.BoundingRectangle, (int)MapInstance.ZoomLevel));
            }
        }

        private void mapInstance_ViewChangeStart(object sender, MapEventArgs e)
        {
            processOutstanding = false;
        }
    }

    public class TilePollingEventArgs : EventArgs
    {
        public TilePollingEventArgs(string layerID, LocationRect bounds, int zoomLevel)
        {
            LayerID = layerID;
            Bounds = bounds;
            ZoomLevel = zoomLevel;
        }

        public string LayerID { get; set; }
        public LocationRect Bounds { get; set; }
        public int ZoomLevel { get; set; }
    }
}