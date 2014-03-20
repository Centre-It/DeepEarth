/// Deep Earth is a community project available under the Microsoft Public License (Ms-PL)
/// Code is provided as is and with no warrenty – Use at your own risk
/// View the project and the latest code at http://codeplex.com/deepearth/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using DeepEarth.Client.MapControl;
using DeepEarth.Client.MapControl.Layers;
using DeepEarth.Client.Services.Bing;
using DeepEarth.Client.Services.BlueMarble;
using DeepEarth.Client.Services.CloudMade;
using DeepEarth.Client.Services.GooglePlanets;
using DeepEarth.Client.Services.NearMap;
using DeepEarth.Client.Services.OAM;
using DeepEarth.Client.Services.OSM;
using DeepEarth.Client.Services.WMS;
using DeepEarth.Client.Services.Yahoo;

namespace ExampleControl.Controls
{
    [ContentProperty("TileSources")]
    public partial class MapSourceControl : ILayer 
    {
        public enum TileSourceIds
        {
            Default,
            VeAerial,
            VeHybrid,
            VeRoad,
            Mapnik,
            Osmarend,
            OAM,
            CmWeb,
            CmMobile,
            CmNoNames,
            CmCycle,
            YahooAerial,
            YahooHybrid,
            YahooStreet,
            BlueMarbleWeb, 
            BlueMarbleLocal,
            GoogleMoon, 
            GoogleMoonClemBw,
            GoogleMoonTerrain, 
            GoogleMarsInfraRed, 
            GoogleMarsElevation, 
            GoogleMarsVisible, 
            WMS,
            NearMap
        }

        private List<TileSourceIds> _TileSources = new List<TileSourceIds>();
        private TileSourceIds _SelectedSourceId;
        private Map _Map;
        public string ApplicationID {get;set;}

        public MapSourceControl()
        {
            InitializeComponent();

            _Map = Map.DefaultInstance;

            if (_TileSources != null)
            {
                _Toolbar.SelectionChanged += SelectionChanged;
                _Map = MapInstance;
                _Map.Events.MapTileSourceChanged += OnTileSourceChanged;
                InitializeList();
            }
        }

        public TileSourceIds SelectedSource
        {
            get { return _SelectedSourceId; }
            set
            {
                _SelectedSourceId = value;
                if (_Toolbar != null)
                {
                    foreach (ListBoxItem item in _Toolbar.Items)
                    {
                        if ((TileSourceIds)item.Tag == _SelectedSourceId)
                        {
                            _Toolbar.SelectedItem = item;
                            break;
                        }
                    }
                }
            }
        }

        public List<TileSourceIds> TileSources
        {
            get { return _TileSources; }
            set
            {
                _TileSources = value;
                InitializeList();
            }
        }


        private void SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (MapInstance != null && _Toolbar.SelectedItem != null)
            {
                TileSourceIds selectedTileSource = (TileSourceIds)((ListBoxItem)_Toolbar.SelectedItem).Tag;
                foreach (TileSourceIds source in _TileSources)
                {
                    if (source == selectedTileSource)
                    {
                        if (_SelectedSourceId != TileSourceIds.Default)
                        {
                            SetMapSource(source);
                        }
                        _SelectedSourceId = source;
                        break;
                    }
                }
            }
        }
 
        private void InitializeList()
        {
            _Toolbar.Items.Clear();

            TileSourceIds baseLayerId = TileSourceIds.Default;
            if(MapInstance.BaseLayer.Source != null)
            {
                baseLayerId = GetSourceId(MapInstance.BaseLayer.Source);
            }

            foreach (TileSourceIds sourceId in _TileSources)
            {
                var newItem = new ListBoxItem { Content = GetDisplayName(sourceId), Tag = sourceId };

                _Toolbar.Items.Add(newItem);
                if (sourceId == baseLayerId)
                {
                    _Toolbar.SelectedItem = newItem;
                    _SelectedSourceId = sourceId;
                }
            }
        }


        private void OnTileSourceChanged(object sender, EventArgs e)
        {
            //Try to find the correct BaseLayer and select it's radio button
            TileSource baseSource = _Map.BaseLayer.Source;
            TileSourceIds baseSourceId = GetSourceId(baseSource);

            foreach (ListBoxItem item in _Toolbar.Items)
            {
                if ((TileSourceIds)item.Tag == baseSourceId)
                {
                    _Toolbar.SelectedItem = item;
                    if (baseSource != null) _SelectedSourceId = baseSourceId;
                    break;
                }
            }
        }

        private string GetDisplayName(TileSourceIds tileSourceId)
        {
            string displayName;
            switch (tileSourceId)
            {
                case TileSourceIds.VeAerial: displayName = "VE: Aerial";break;
                case TileSourceIds.VeHybrid:displayName = "VE: Hybrid";break;
                case TileSourceIds.VeRoad:displayName = "VE: Street";break;
                case TileSourceIds.BlueMarbleWeb:displayName = "Blue Marble Web";break;
                case TileSourceIds.BlueMarbleLocal:displayName = "Blue Marble Local";break;
                case TileSourceIds.YahooAerial:displayName = "YHOO: Aerial";break;
                case TileSourceIds.YahooHybrid:displayName = "YHOO: Hybrid";break;
                case TileSourceIds.YahooStreet:displayName = "YHOO: Street";break;
                case TileSourceIds.CmWeb: displayName = "CM: Web"; break;
                case TileSourceIds.CmMobile: displayName = "CM: Mobile"; break;
                case TileSourceIds.CmNoNames: displayName = "CM: No Name"; break;
                case TileSourceIds.CmCycle: displayName = "CM: Cycle"; break;
                case TileSourceIds.Mapnik: displayName = "OSM: Mapnik"; break;
                case TileSourceIds.Osmarend: displayName = "OSM: Osmarender"; break;
                case TileSourceIds.OAM: displayName = "Open Aerial Maps"; break;
                case TileSourceIds.GoogleMoon:displayName = "GM: Moon";break;
                case TileSourceIds.GoogleMoonClemBw:displayName = "GM: Moon ClemBW";break;
                case TileSourceIds.GoogleMoonTerrain:displayName = "GM: Moon Terrain";break;
                case TileSourceIds.GoogleMarsInfraRed:displayName = "GM: Mars Infrared";break;
                case TileSourceIds.GoogleMarsElevation:displayName = "GM: Mars Elevation";break;
                case TileSourceIds.GoogleMarsVisible:displayName = "GM: Mars Visible";break;
                case TileSourceIds.WMS: displayName = "WMS"; break;
                case TileSourceIds.NearMap: displayName = "NearMap"; break;
                default: displayName = string.Empty; break;
            }
            return displayName;
        }


        private void SetMapSource(TileSourceIds tileSourceId)
        {
            switch (tileSourceId)
            {
                case TileSourceIds.VeAerial: MapInstance.BaseLayer.Source = new VeTileSource(ApplicationID, VeMapModes.VeAerial); break;
                case TileSourceIds.VeHybrid: MapInstance.BaseLayer.Source = new VeTileSource(ApplicationID, VeMapModes.VeHybrid); break;
                case TileSourceIds.VeRoad: MapInstance.BaseLayer.Source = new VeTileSource(ApplicationID, VeMapModes.VeRoad); break;
                case TileSourceIds.BlueMarbleWeb: MapInstance.BaseLayer.Source = new BmTileSource(BmMapModes.BlueMarbleWeb); break;
                case TileSourceIds.BlueMarbleLocal: MapInstance.BaseLayer.Source = new BmTileSource(BmMapModes.BlueMarbleLocal); break;
                case TileSourceIds.YahooAerial: MapInstance.BaseLayer.Source = new YhooTileSource(YhooMapModes.YahooAerial); break;
                case TileSourceIds.YahooHybrid: MapInstance.BaseLayer.Source = new YhooTileSource(YhooMapModes.YahooHybrid); break;
                case TileSourceIds.YahooStreet: MapInstance.BaseLayer.Source = new YhooTileSource(YhooMapModes.YahooStreet); break;
                case TileSourceIds.CmWeb: MapInstance.BaseLayer.Source = new CmTileSource(CmMapModes.CmWeb); break;
                case TileSourceIds.CmMobile: MapInstance.BaseLayer.Source = new CmTileSource(CmMapModes.CmMobile); break;
                case TileSourceIds.CmNoNames: MapInstance.BaseLayer.Source = new CmTileSource(CmMapModes.CmNoNames); break;
                case TileSourceIds.CmCycle: MapInstance.BaseLayer.Source = new CmTileSource(CmMapModes.CmCycle); break;
                case TileSourceIds.Mapnik: MapInstance.BaseLayer.Source = new OsmTileSource(OsmMapModes.Mapnik); break;
                case TileSourceIds.Osmarend: MapInstance.BaseLayer.Source = new OsmTileSource(OsmMapModes.Osmarend); break;
                case TileSourceIds.OAM: MapInstance.BaseLayer.Source = new OamTileSource(OamMapModes.OAM); break;
                case TileSourceIds.GoogleMoon: MapInstance.BaseLayer.Source = new GpTileSource(GpMapModes.GoogleMoon); break;
                case TileSourceIds.GoogleMoonClemBw: MapInstance.BaseLayer.Source = new GpTileSource(GpMapModes.GoogleMoonClemBw); break;
                case TileSourceIds.GoogleMoonTerrain: MapInstance.BaseLayer.Source = new GpTileSource(GpMapModes.GoogleMarsInfraRed); break;
                case TileSourceIds.GoogleMarsInfraRed: MapInstance.BaseLayer.Source = new GpTileSource(GpMapModes.GoogleMoonTerrain); break;
                case TileSourceIds.GoogleMarsElevation: MapInstance.BaseLayer.Source = new GpTileSource(GpMapModes.GoogleMarsElevation); break;
                case TileSourceIds.GoogleMarsVisible: MapInstance.BaseLayer.Source = new GpTileSource(GpMapModes.GoogleMarsVisible); break;
                case TileSourceIds.WMS: MapInstance.BaseLayer.Source = new WmsTileSource(WmsMapModes.WMS); break;
                case TileSourceIds.NearMap: MapInstance.BaseLayer.Source = new NearMapTileSource(NearMapModes.NearMap); break;
            }
        }


        private TileSourceIds GetSourceId(TileSource source)
        {
            TileSourceIds matchingSourceId = TileSourceIds.Default;
            foreach(TileSourceIds sourceId in TileSources)
            {
                if(source.ID == sourceId.ToString())
                {
                    matchingSourceId = sourceId;
                }
            }
            return matchingSourceId;
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


