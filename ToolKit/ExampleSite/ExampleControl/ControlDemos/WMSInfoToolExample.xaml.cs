using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ExampleControlBing.WMSService;
using System.ServiceModel;
using DeepEarth.BingMapsToolkit.Client.Controls;

namespace ExampleControlBing.ControlDemos
{
    public partial class WMSInfoToolExample : UserControl
    {
        public WMSInfoToolExample()
        {
            InitializeComponent();

            wmsinfotool.OnWMSInfoToolRequest += new EventHandler<WMSInfoToolEventArgs>(wmsinfotool_OnWMSInfoToolRequest);
        }

        void wmsinfotool_OnWMSInfoToolRequest(object sender, WMSInfoToolEventArgs e)
        {
            WMSClient.WMSFeatureRequestCompleted += (s, arg) =>
            {
                e.OnWMSInfoToolRequestComplete(arg.Result);
            };
            WMSClient.WMSFeatureRequestAsync(e.Minx, e.Maxx, e.Miny, e.Maxy, e.X, e.Y, e.Width, e.Height);
        }

        private WMSServiceClient serviceClient;

        private WMSServiceClient WMSClient
        {
            get
            {
                if (null == serviceClient)
                {
                    //Handle http/https; OutOfBrowser is currently supported on the MapControl only for http pages

                    var serviceUri =
                        new UriBuilder("http://" + Application.Current.Host.Source.Host + ":" + Application.Current.Host.Source.Port + "/Services/WMSService.svc");

                    //Create the Service Client

                    serviceClient = new WMSServiceClient();
                    serviceClient.Endpoint.Address = new EndpointAddress(serviceUri.Uri);
                }
                return serviceClient;
            }
        }
    }
}
