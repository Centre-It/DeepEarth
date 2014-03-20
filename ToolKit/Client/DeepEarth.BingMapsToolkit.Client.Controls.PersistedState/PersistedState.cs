// Deep Earth is a community project available under the Microsoft Public License (Ms-PL)
// Code is provided as is and with no warrenty – Use at your own risk
// View the project and the latest code at http://DeepEarth.codeplex.com/

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using DeepEarth.BingMapsToolkit.Client.Common;
using DeepEarth.BingMapsToolkit.Client.Common.Commanding;
using Microsoft.Maps.MapControl;
using Microsoft.Maps.MapControl.Core;
using System.Windows.Browser;

namespace DeepEarth.BingMapsToolkit.Client.Controls
{
    public class PersistedState : Control, IMapControl<MapCore>
    {
        public const string PersistedStateISOKey = "PersistedStateISOKey";

        private string mapName;

        public PersistedState()
        {
            if (!Utilities.IsDesignTime())
            {
                Loaded += PersistedState_Loaded;
                Application.Current.Exit += Current_Exit;
                PersistedCommands.PersistedStateForceSaveCommand.Executed += PersistedStateForceSaveCommand_Executed;
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

        public MapCore MapInstance { get; set; }

        public bool DisableMode { get; set; }
        public bool DisableZoomlevel { get; set; }
        public bool DisableCenter { get; set; }

        #region IDisposable Members

        public void Dispose()
        {
        }

        #endregion

        private void PersistedStateForceSaveCommand_Executed(object sender, ExecutedEventArgs e)
        {
            saveCurrentView();
        }

        private void PersistedState_Loaded(object sender, RoutedEventArgs e)
        {
            setMapInstance(MapName);
            Load();
        }

        protected virtual void Load()
        {
            try
            {
                var persistedSettings = IsolatedStorage.LoadData<PersistedSettings>(PersistedStateISOKey);
                ProcessLoadedPersistedSettings(persistedSettings);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        protected void ProcessLoadedPersistedSettings(PersistedSettings persistedSettings)
        {
            if (persistedSettings != null)
            {
                if (MapInstance != null)
                {
                    if (!DisableCenter)
                    {
                        MapInstance.Center = new Location(persistedSettings.Latitude, persistedSettings.Longitude,
                                                          persistedSettings.Altitude);
                    }
                    if (!DisableZoomlevel)
                    {
                        MapInstance.ZoomLevel = persistedSettings.ZoomLevel;
                    }
                    if (!DisableMode)
                    {
                        MapInstance.Mode = ModeConvertor.ConvertBack(persistedSettings.Mode);
                    }
                }
                PersistedCommands.PersistedStateLoadedCommand.Execute(persistedSettings);
            }
        }

        private void setMapInstance(string mapname)
        {
            MapInstance = Utilities.FindVisualChildByName<MapCore>(Application.Current.RootVisual, mapname);
        }

        private void Current_Exit(object sender, EventArgs e)
        {
            saveCurrentView();
        }

        private void saveCurrentView()
        {
            var persistedSettings = GetCurrentPersistedSettings();
            IsolatedStorage.SaveData(persistedSettings, PersistedStateISOKey);
        }

        public PersistedSettings GetCurrentPersistedSettings()
        {
            var persistedSettings = new PersistedSettings();
            if (MapInstance != null)
            {
                var center = MapInstance.Center;
                persistedSettings = new PersistedSettings
                {
                    Longitude = center.Longitude,
                    Latitude = center.Latitude,
                    Altitude = center.Altitude,
                    ZoomLevel = MapInstance.ZoomLevel,
                    Mode = ModeConvertor.Convert(MapInstance.Mode),
                };
            }

            //get custom settings
            PersistedCommands.PersistedStateSaveCommand.Execute(persistedSettings);
            return persistedSettings;
        }
    }
}