/// Deep Earth is a community project available under the Microsoft Public License (Ms-PL)
/// Code is provided as is and with no warrenty – Use at your own risk
/// View the project and the latest code at http://codeplex.com/deepearth/

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DeepEarth.Client.MapControl.Layers;

namespace DeepEarth.Client.Services.GWC
{
    public class GWCTileSourceDE : TileSource
    {
        private const string TilePath = @"{GWC_ADDRESS}/service/gmaps?layers={L}&zoom={Z}&x={X}&y={Y}";
        private string gwc_address;
        private string layername;

        //Constructor Called by XAML instanciation;
        public GWCTileSourceDE() : base()
        {
        }

        public GWCTileSourceDE(string gwc_address, string layername) : base()
        {
            ID = layername;
            this.gwc_address = gwc_address;
            this.layername = layername;
            _IsInitialized = true;
            if (InitializeCompleted != null) InitializeCompleted(this, null);
        }

        #region TileSource Overrides

        private bool _IsInitialized;

        public override bool IsInitialized
        {
            get { return _IsInitialized; }
        }

        public override Uri GetTile(int tileLevel, int tilePositionX, int tilePositionY)
        {
            if (IsInitialized)
            {
                int zoom = TileToZoom(tileLevel);
                string url = TilePath;

                url = url.Replace("{GWC_ADDRESS}", gwc_address);
                url = url.Replace("{L}", layername);
                url = url.Replace("{Z}", zoom.ToString());
                url = url.Replace("{X}", tilePositionX.ToString());
                url = url.Replace("{Y}", tilePositionY.ToString());
                return new Uri(url);
            }
            return null;
        }

        public override event EventHandler InitializeCompleted;

        #endregion

    }
}