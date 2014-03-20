/// Deep Earth is a community project available under the Microsoft Public License (Ms-PL)
/// Code is provided as is and with no warrenty – Use at your own risk
/// View the project and the latest code at http://codeplex.com/deepearth/

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Maps.MapControl;

namespace DeepEarth.Client.Services.GWC
{
    public class GWCTileSourceBing : TileSource
    {
        private const string TilePath = @"{GWC_ADDRESS}/service/gmaps?layers={L}&zoom={Z}&x={X}&y={Y}";
        private string gwc_address;
        private string layername;

        private string[] cyclestring;
        private static int cyclecount = 0;

        //Constructor Called by XAML instanciation;
        public GWCTileSourceBing() : base()
        {
        }

        public GWCTileSourceBing(string gwc_address, string layername) : base()
        {
            this.gwc_address = gwc_address;
            this.layername = layername;
            cyclestring = new string[] { "" };           
        }

        public GWCTileSourceBing(string gwc_address, string[] cyclestring, string layername)
            : this(gwc_address, layername)
        {
            this.cyclestring = cyclestring;
        }

        #region TileSource Overrides

        public override Uri GetUri(int x, int y, int zoomLevel) 
        {
            string url = TilePath;           

            url = url.Replace("{GWC_ADDRESS}", gwc_address);
            url = url.Replace("{CYCLE}", cyclestring[cyclecount]); 
            url = url.Replace("{L}", layername);
            url = url.Replace("{Z}", zoomLevel.ToString());
            url = url.Replace("{X}", x.ToString());
            url = url.Replace("{Y}", y.ToString());

            cyclecount = (cyclecount + 1) % cyclestring.Length;

            return new Uri(url);
        }        

        #endregion

    }
}
