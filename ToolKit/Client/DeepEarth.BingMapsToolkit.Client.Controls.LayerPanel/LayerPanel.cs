// Deep Earth is a community project available under the Microsoft Public License (Ms-PL)
// Code is provided as is and with no warrenty – Use at your own risk
// View the project and the latest code at http://DeepEarth.codeplex.com/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using DeepEarth.BingMapsToolkit.Client.Common;
using DeepEarth.BingMapsToolkit.Client.Common.Commanding;
using DeepEarth.BingMapsToolkit.Client.Common.Entities;
using DeepEarth.BingMapsToolkit.Client.MapGeometry;
using DeepEarth.BingMapsToolkit.Common.Entities;
using Microsoft.Maps.MapControl;
using Microsoft.Maps.MapControl.Core;

namespace DeepEarth.BingMapsToolkit.Client.Controls
{
    public abstract class LayerPanel : ContentControl, IMapControl<MapCore>
    {
        private MapCore mapInstance;
        private DateRange dateRangeDisplay;
        private DateRange dateRangeLoaded;

        private bool enableBalloon;
        private bool layersSelectable = true;
        private bool enableEdit;
        private string mapName;
        private Dictionary<string, StyleSpecification> styles;

        public event BalloonLaunchEventHandler BalloonLaunch;
        public event LoadLayerEventHandler LoadLayerData;

        protected Polling RefreshPolling;
        private DispatcherTimer vectorLayerRefreshThrottleTimer;

        public abstract ObservableCollection<LayerDefinition> Layers { get; set; }

        protected LayerPanel()
        {
            Loaded += LayerPanel_Loaded;
            Styles = new Dictionary<string, StyleSpecification>();
        }

        void LayerPanel_Loaded(object sender, RoutedEventArgs e)
        {
            setMapInstance(MapName);

            //setup refresh polling
            RefreshPolling = new Polling();
            RefreshPolling.RefreshNeeded += refreshPolling_RefreshNeeded;

            Commands.LoadBalloonDataCommand.Executed += LoadBalloonData_Executed;
        }

        private void LoadBalloonData_Executed(object sender, ExecutedEventArgs e)
        {
            if (BalloonLaunch != null)
            {
                BalloonLaunch(e.Source, (BalloonEventArgs)e.Parameter);
            }
        }

        public Dictionary<string, StyleSpecification> Styles
        {
            get { return styles; }
            set
            {
                if (styles != null)
                {
                    styles = value;
                    RefreshLayers();
                }
                styles = value;
            }
        }

        public bool EnableBalloon
        {
            get { return enableBalloon; }
            set
            {
                enableBalloon = value;
                //set on all items
                if (MapInstance != null)
                {
                    foreach (UIElement layer in MapInstance.Children)
                    {
                        if (layer is EnhancedMapLayer)
                        {
                            ((EnhancedMapLayer)layer).EnableBalloon = enableBalloon;
                        }
                    }
                }
            }
        }

        public bool EnableEdit
        {
            get { return enableEdit; }
            set
            {
                enableEdit = value;
                //set on all items
                foreach (UIElement layer in MapInstance.Children)
                {
                    if (layer is EnhancedMapLayer)
                    {
                        ((EnhancedMapLayer)layer).EnableSelection = enableEdit;
                    }
                }
            }
        }

        public string TileServerPath { get; set; }
        public bool EnableTileRefreshPolling { get; set; }

        //Used to filter the items loaded for display (slow refresh of data from server)
        public DateRange DateRangeLoaded
        {
            get { return dateRangeLoaded; }
            set
            {
                //only if it changes.
                if (dateRangeLoaded == null || dateRangeLoaded.ValueLow.CompareTo(value.ValueLow) != 0 || dateRangeLoaded.ValueHigh.CompareTo(value.ValueHigh) != 0)
                {
                    dateRangeLoaded = value;
                    //refresh temporal layers
                    RefreshTemporalLayers();
                    //also causes a reset to the DateRangeDisplay
                    DateRangeDisplay = dateRangeLoaded;
                }
            }
        }

        //Used to filter the items on display (quickly hide / show items).
        public DateRange DateRangeDisplay
        {
            get { return dateRangeDisplay; }
            set
            {
                //only if it changes.
                if (dateRangeDisplay == null || dateRangeDisplay.ValueLow.CompareTo(value.ValueLow) != 0 || dateRangeDisplay.ValueHigh.CompareTo(value.ValueHigh) != 0)
                {
                    dateRangeDisplay = value;
                    //refresh temporal layers
                    foreach (LayerDefinition layer in Layers)
                    {
                        if (layer.Selected && layer.Temporal)
                        {
                            EnhancedMapLayer mapLayer = Vector.GetLayerByID(layer.LayerID, MapInstance);
                            if (mapLayer != null)
                            {
                                mapLayer.DateRangeDisplay = value;
                            }
                        }
                    }
                }
            }
        }

        public bool LayersSelectable
        {
            get { return layersSelectable; }
            set
            {
                layersSelectable = value;
                if (layersSelectable)
                {
                    Vector.ShowOverlays(MapInstance, EnableBalloon, EnableEdit);
                }
                else
                {
                    Vector.HideOverlays(MapInstance);
                    Commands.ClosePopupCommand.Execute();
                }
            }
        }


        public void RefreshLayers()
        {
            RefreshVectorLayers();
            RefreshScaledImageLayers();
        }

        public void RefreshVectorLayersThrottled()
        {
            //Setup a timer, while method is being called we delay until period expires. 
            if (vectorLayerRefreshThrottleTimer == null)
            {
                vectorLayerRefreshThrottleTimer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 0, 200) };
                vectorLayerRefreshThrottleTimer.Tick += vectorLayerRefreshThrottleTimer_Tick;
                vectorLayerRefreshThrottleTimer.Start();
            }
            else
            {
                vectorLayerRefreshThrottleTimer.Stop();
                vectorLayerRefreshThrottleTimer.Start();
            }
        }

        void vectorLayerRefreshThrottleTimer_Tick(object sender, EventArgs e)
        {
            if (vectorLayerRefreshThrottleTimer != null)
            {
                vectorLayerRefreshThrottleTimer.Stop();
                vectorLayerRefreshThrottleTimer.Tick -= vectorLayerRefreshThrottleTimer_Tick;
                vectorLayerRefreshThrottleTimer = null;
            }

            RefreshVectorLayers();
        }


        public void RefreshVectorLayers()
        {
            Vector.ShowOverlays(MapInstance, EnableBalloon, EnableEdit);
            foreach (var layer in Layers)
            {
                if (layer.LayerType == 1 && layer.Selected)
                {
                    LoadLayer(layer);
                }
            }
        }

        public void RefreshTemporalLayers()
        {
            foreach (var layer in Layers)
            {
                if (layer.Selected && layer.Temporal)
                {
                    LoadLayer(layer);
                }
            }
        }

        public void RefreshScaledImageLayers()
        {
            Vector.ShowOverlays(MapInstance, EnableBalloon, EnableEdit);
            foreach (var layer in Layers)
            {
                if (layer.LayerType == 3 && layer.Selected)
                {
                    LoadLayer(layer);
                }
            }
        }

        public bool SetLayerVisibility(string layerID, bool visible)
        {
            foreach (var layer in Layers)
            {
                if (layer.LayerID == layerID)
                {
                    layer.Selected = visible;
                    return true;
                }
            }
            return false;
        }

        private void refreshPolling_RefreshNeeded(object sender, PollingEventArgs args)
        {
            LoadLayer(args.LayerDefinition);
        }

        public LayerDefinition GetLayerDefinitionById(string layerID)
        {
            foreach (LayerDefinition layer in Layers)
            {
                if (layer.LayerID == layerID)
                {
                    return layer;
                }
            }
            return null;
        }

        public BaseGeometry GetItemByID(string layerID, string itemID)
        {
            EnhancedMapLayer layer = Vector.GetLayerByID(layerID, MapInstance);
            if (layer != null)
            {
                return layer.GetGeometryByItemID(itemID);
            }
            return null;
        }

        protected virtual void UnloadLayer(LayerDefinition layerDefinition)
        {
            switch (layerDefinition.LayerType)
            {
                case 1: //Vector Layer
                    Vector.RemoveOverlay(layerDefinition, MapInstance);
                    break;
                case 2: //Raster Layer
                    Raster.RemoveOverlay(layerDefinition, MapInstance);
                    break;
                case 3: //Scaled Image Overlay
                    Raster.RemoveImage(layerDefinition, MapInstance);
                    break;
            }
        }

        protected virtual void LoadLayer(LayerDefinition layerDefinition)
        {
            switch (layerDefinition.LayerType)
            {
                case 1: //Vector Layer
                    if (LoadLayerData != null)
                    {
                        //raise event to get vector data
                        LoadLayerData(this, new LoadLayerEventArgs(layerDefinition, DateRangeLoaded), CreateVectorOverlay);
                    }
                    break;
                case 2: //Raster Layer
                    Raster.CreateOverlay(layerDefinition, MapInstance, TileServerPath, EnableTileRefreshPolling);
                    break;
                case 3: //Scaled Image Overlay
                    Raster.CreateImage(layerDefinition, MapInstance, styles, TileServerPath);
                    break;
            }
        }

        public void CreateVectorOverlay(ObservableCollection<VectorLayerData> layerData, LayerDefinition layerDefinition)
        {
            Vector.CreateOverlay(layerData, layerDefinition, MapInstance, styles, EnableBalloon, LayersSelectable,
                                 DateRangeDisplay);
        }


        #region IMapControl<MapCore> Members

        public void Dispose()
        {
            MapInstance = null;
        }

        public MapCore MapInstance
        {
            get { return mapInstance; }
            set
            {
                if (mapInstance != null)
                {
                    //detach any map events
                }
                mapInstance = value;
                if (mapInstance != null)
                {
                    //attach any map events
                }
            }
        }

        public string MapName
        {
            get { return mapName; }
            set
            {
                mapName = value;
                setMapInstance(MapName);
            }
        }

        private void setMapInstance(string mapname)
        {
            MapInstance = Utilities.FindVisualChildByName<Map>(Application.Current.RootVisual, mapname);
        }

        #endregion
    }
}
