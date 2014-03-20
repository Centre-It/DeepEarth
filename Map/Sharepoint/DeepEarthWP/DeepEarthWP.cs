using System;
using System.Runtime.InteropServices;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Serialization;
using System.Collections;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using Microsoft.SharePoint.WebPartPages;

#region Readme about deployment and modifications

// You have to manually deploy the TokenService in order to get
// bing-maps to work in SharePoint. 
//
// The easiest way to deploy this webpart to SharePoint is to use 
// "Visual Studio 2008 extensions for SharePoint (VSeWSS) 1.3" tool
// That tool should generate a setup.bat tool.
// 
// Copy the DeepEarth.xap file to a location (eaither a Document Library) or a
// psysical directory where SharePoint can access the file. Use the property-field
// of this WebPart so this WebPart knows where the .xap-file is.
// And of course, you have to have SilverLight enabled on SharePoint
//
// Use the Places.stp for an example list in SharePoint.
// 
// In order to adjust the client for SharePoint replace the Page() method
// with the following
    //public Page(StartupEventArgs e)
    //        {
    //            InitializeComponent();


    //            //Choose your startup provider
    //            //map.BaseLayer.Source = new VeTileSource(VeMapModes.VeHybrid);
    //            map.BaseLayer.Source = 
    //                new DeepEarth.Client.Services.Yahoo.YhooTileSource(DeepEarth.Client.Services.Yahoo.YhooMapModes.YahooStreet);
    //            //map.BaseLayer.Source = new OsmTileSource(OsmMapModes.Mapnik);
    //            //map.BaseLayer.Source = new OamTileSource(OamMapModes.OAM);

    //            GeometryLayerTest();

    //            GeometryAnchorTest();

    //            #region SharePoint
    //            if (e != null && e.InitParams != null)
    //            {
    //                string lat, lon, zoom;
                   
    //                e.InitParams.TryGetValue("lat", out lat);
    //                e.InitParams.TryGetValue("lon", out lon);
    //                e.InitParams.TryGetValue("zoom", out zoom);

    //                if (!string.IsNullOrEmpty(lat) && !string.IsNullOrEmpty(lon))
    //                {
    //                    Point point = new Point(double.Parse(lon), double.Parse(lat));   
    //                    double dZoom = string.IsNullOrEmpty(zoom) ? 10.0 : double.Parse(zoom);
    //                    map.SetViewCenter(point, dZoom);
    //                }
    //            }
    //            #endregion

    //        }

// In the App.xaml.cs file update replace the method Application_Startup with the following

        //private void Application_Startup(object sender, StartupEventArgs e)
        //{
        //    RootVisual = new Page(e);
//}

#endregion

namespace DeepEarth.Client.SharePoint
{
    [Guid("87522800-e396-4b0c-b04f-2cc3d7f82aa6")]
    public class DeepEarthWP : System.Web.UI.WebControls.WebParts.WebPart
    {
        string _fieldValue;
        private IWebPartField _provider;

        
        protected override void OnLoad(EventArgs e)
        {
            //base.OnLoad(e);

            ScriptManager scriptManager = ScriptManager.GetCurrent(Page);

            if (scriptManager == null)
            {
                scriptManager = new ScriptManager();
                Controls.Add(scriptManager);
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            if (_provider != null)
            {
                LoadDeepEarthControl();
            }
        }
        
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            LoadDeepEarthControl();
        }

        void LoadDeepEarthControl()
        {

            try
            {
                if (string.IsNullOrEmpty(DeepEarthXapPath))
                    throw new Exception("DeepEarthXapPath not set. Cannot load DeepEath.");


                System.Web.UI.SilverlightControls.Silverlight deMap = new System.Web.UI.SilverlightControls.Silverlight();
                deMap.ID = "deMapControl";
                deMap.Source = DeepEarthXapPath;

                if (Height.IsEmpty)
                    Height = new Unit(500, UnitType.Pixel);

                deMap.Height = new Unit(100, UnitType.Percentage);
                deMap.Width = new Unit(100, UnitType.Percentage);
                
                if (_provider != null)
                {
                    _provider.GetFieldValue(new FieldCallback(GetFieldData));
                   
                    if (!string.IsNullOrEmpty(_fieldValue))
                        deMap.InitParameters = _fieldValue;
                    
                }

                Controls.Add(deMap);
            }
            catch (Exception ex)
            {
                while (ex.InnerException != null)
                {
                    Controls.Add(new LiteralControl("Error: " + ex.Message + " <br />"));
                }
            }
        }
        

        [ConnectionConsumer("position", "connPoint1", AllowsMultipleConnections=true)]
        public void SetConnectionInterface(System.Web.UI.WebControls.WebParts.IWebPartField provider)
        {
            _provider = provider;
        }

        private void GetFieldData(object fieldData)
        {
            if (fieldData != null)
            {
                _fieldValue = fieldData.ToString();
            }
        }

        #region DeepEearthWPProperties

        [Personalizable(PersonalizationScope.Shared)]
        [WebBrowsable(true)]
        [WebDisplayName("Path to Deep Earth .xaml to use.")]
        [WebDescription("The path to the Deep Earth xaml to use i.e http://server/apps/DeepEarthSP.xap.")]
        [SPWebCategoryName("Deep Earth")]
        public string DeepEarthXapPath { get; set; }

        #endregion
    }
}
