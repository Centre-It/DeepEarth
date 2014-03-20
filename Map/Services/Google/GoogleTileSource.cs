/// Deep Earth is a community project available under the Microsoft Public License (Ms-PL)
/// Code is provided as is and with no warrenty – Use at your own risk
/// View the project and the latest code at http://codeplex.com/deepearth/

using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DeepEarth.Client.MapControl.Layers;

namespace DeepEarth.Client.Services.Google
{
    public enum GoogleMapModes
    {
        Street,
        Satellite,
        SatelliteHybrid,
        Physical,
        PhysicalHybrid, 
        StreetOverlay,
        StreetWaterOverlay
    }

    public class GoogleTileSource : TileSource 
    {       
        private const string TilePathBase = @"http://mt{0}.google.com/vt/lyrs={1}&z={2}&x={3}&y={4}";

        private const string charStreet = "m";
        private const string charSatellite = "s";
        private const string charSatelliteHybrid = "y";
        private const string charPhysical = "t";
        private const string charPhysicalHybrid = "p";
        private const string charStreetOverlay = "h";
        private const string charStreetWaterOverlay = "r";
 
        private bool _IsInitialized;

        private bool _IsTileDownloadStarted;
        private GoogleMapModes _MapMode;

        private int server_rr = 0;

        //Constructor Called by XAML instanciation; Wait for MapMode to be set to initialize services
        public GoogleTileSource()
        {
        }

        public GoogleTileSource(GoogleMapModes mode)
        {
            MapMode = mode;
        }

        public GoogleMapModes MapMode
        {
            get { return _MapMode; }
            set
            {
                if (_IsTileDownloadStarted)
                {
                    throw new InvalidOperationException();
                }

                _MapMode = value;
                ID = value.ToString();
                _IsInitialized = true;
                if (InitializeCompleted != null) InitializeCompleted(this, null);
            }
        }

        public override bool IsInitialized
        {
            get { return _IsInitialized; }
        }

        public override Color TileColor
        {
            get
            {
                Color baseColor = Colors.Gray;
               
                return baseColor;
            }
        }


        public override UIElement GetCopyright()
        {
            try
            {
                const string logoPath = "Enter logo-type URI if you want a google logo";
                return new Image { Source = new BitmapImage(new Uri(logoPath)), MaxHeight = 48, MaxWidth = 96, Stretch = Stretch.Uniform };
            }
            catch
            {                
            }

            return null;
        }

        public override Uri GetTile(int tileLevel, int tilePositionX, int tilePositionY)
        {
            if (IsInitialized)
            {
                int zoom = TileToZoom(tileLevel);
                _IsTileDownloadStarted = true;

                string url = string.Empty;
                server_rr = (server_rr + 1) % 4;

                switch (MapMode)
                {
                    case GoogleMapModes.Street:                        
                        url = XYZUrl(TilePathBase, server_rr, charStreet, zoom, tilePositionX, tilePositionY);
                        break;
                    case GoogleMapModes.Satellite:
                        url = XYZUrl(TilePathBase, server_rr, charSatellite, zoom, tilePositionX, tilePositionY);
                        break;
                    case GoogleMapModes.SatelliteHybrid:
                        url = XYZUrl(TilePathBase, server_rr, charSatelliteHybrid, zoom, tilePositionX, tilePositionY);
                        break;
                    case GoogleMapModes.Physical:
                        url = XYZUrl(TilePathBase, server_rr, charPhysical, zoom, tilePositionX, tilePositionY);
                        break;
                    case GoogleMapModes.PhysicalHybrid:
                        url = XYZUrl(TilePathBase, server_rr, charPhysicalHybrid, zoom, tilePositionX, tilePositionY);
                        break;
                    case GoogleMapModes.StreetOverlay:
                        url = XYZUrl(TilePathBase, server_rr, charStreetOverlay, zoom, tilePositionX, tilePositionY);
                        break;
                    case GoogleMapModes.StreetWaterOverlay:
                        url = XYZUrl(TilePathBase, server_rr, charStreetWaterOverlay, zoom, tilePositionX, tilePositionY);
                        break; 
                }

                return new Uri(url);
            }

            return null;
        }

        private static string XYZUrl(string url, int server, string mapmode, int zoom, int tilePositionX, int tilePositionY)
        {
            url = string.Format(url, server, mapmode, zoom, tilePositionX, tilePositionY);

            return url;
        }     

        public override event EventHandler InitializeCompleted;
    }
}