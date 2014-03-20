/// Deep Earth is a community project available under the Microsoft Public License (Ms-PL)
/// Code is provided as is and with no warrenty – Use at your own risk
/// View the project and the latest code at http://codeplex.com/deepearth/

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DeepEarth.Client.MapControl;
using DeepEarth.Client.MapControl.Layers;

namespace DeepEarth.Client.Services.NearMap
{
    public enum NearMapModes
    {
        NearMap
    }
    public class NearMapTileSource : TileSource
    {
        private const int DefaultMaxZoomLevel = 21;
        private const int DefaultTileSize = 256;
        private const string TilePath = "http://www.nearmap.com/maps/hl=en&x={X}&y={Y}&z={Z}&nml=Vert&s=Ga";
        private bool _IsTilesDownloadStarted;
        private NearMapModes _MapMode;

		public string DateMap {get; set;}

        //Constructor Called by XAML instanciation; Wait for MapMode to be set to initialize services
        public NearMapTileSource()
            : base(Map.DefaultInstance, (int)Math.Pow(2, DefaultMaxZoomLevel) * DefaultTileSize,
                   (int)Math.Pow(2, DefaultMaxZoomLevel) * DefaultTileSize,
                   DefaultTileSize,
                   DefaultTileSize,
                    0)
        {
            
        }

        public NearMapTileSource(NearMapModes mode)
            : base(Map.DefaultInstance, (int)Math.Pow(2, DefaultMaxZoomLevel) * DefaultTileSize,
                (int)Math.Pow(2, DefaultMaxZoomLevel) * DefaultTileSize,
                DefaultTileSize,
                DefaultTileSize,
                0)
        {
            MapMode = mode;
        }

        public NearMapModes MapMode
        {
            get { return _MapMode; }
            set
            {
                if (_IsTilesDownloadStarted)
                {
                    throw new InvalidOperationException();
                }

                _MapMode = value;
                ID = value.ToString();
                MaxZoomLevel = DefaultMaxZoomLevel;
                _IsInitialized = true;
                if (InitializeCompleted != null) InitializeCompleted(this, null);
            }
        }

        #region TileSource Overrides

        private bool _IsInitialized;

        public override bool IsInitialized
        {
            get { return _IsInitialized; }
        }

        public override Color TileColor
        {
            get
            {
                Color baseColor = Colors.Black;
                switch (_MapMode)
                {
                    case NearMapModes.NearMap:
                        baseColor = Color.FromArgb(0xFF, 0x1A, 0x44, 0x7A);
                        break;
                }
                return baseColor;
            }
        }

        public override Uri GetTile(int tileLevel, int tilePositionX, int tilePositionY)
        {
            if (IsInitialized)
            {
                int zoom = TileToZoom(tileLevel);
                _IsTilesDownloadStarted = true;
                string url = TilePath;

                url = url.Replace("{Z}", zoom.ToString());
                url = url.Replace("{X}", tilePositionX.ToString());
                url = url.Replace("{Y}", tilePositionY.ToString());
				if (!string.IsNullOrEmpty(DateMap))
				{
					url = url + "&nmd=" + DateMap;
				}
                return new Uri(url);
            }
            return null;
        }

        public override event EventHandler InitializeCompleted;

        #endregion

        public override UIElement GetCopyright()
        {
            const string logoPath = "http://a1.twimg.com/profile_images/472103490/Nearmap-arrow_normal.jpg";
            var link = new HyperlinkButton { TargetName = "_blank", NavigateUri = new Uri("http://www.nearmap.com/legal/licence.aspx") };
            ToolTipService.SetToolTip(link, "Copyright NearMap");
            link.Content = new Image { Source = new BitmapImage(new Uri(logoPath)), Height = 48, Width = 48 };
            return link;
        }
    }
}