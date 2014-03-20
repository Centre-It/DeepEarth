using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;
using DeepEarth.BingMapsToolkit.Client.Common;
using DeepEarth.BingMapsToolkit.Client.Controls.GeoCodeService;
using Microsoft.Maps.MapControl;
using Microsoft.Maps.MapControl.Core;

namespace DeepEarth.BingMapsToolkit.Client.Controls
{
    [TemplatePart(Name = PART_map, Type = typeof (Map))]
    public class MiniMap : Control, IMapControl<MapCore>
    {
        private const double defaultZoomOffset = -7;
        private const string PART_map = "PART_map";
        private const string PART_reverseServiceResultText = "PART_reverseServiceResultText";

        private Map map;

        private MapCore mapInstance;
        private string mapName;
        private TextBlock reverseServiceResultText;

        public double RvsGeocodeMinZoomLevel;

        public MiniMap()
        {
            DefaultStyleKey = typeof (MiniMap);
            RvsGeocodeEnabled = true;
            ZoomOffset = defaultZoomOffset;
            Loaded += MiniMap_Loaded;
        }

        public double ZoomOffset { get; set; }
        public bool RvsGeocodeEnabled { get; set; }

        #region IMapControl<MapCore> Members

        public string MapName
        {
            get { return mapName; }
            set
            {
                mapName = value;
                setMapInstance(MapName);
            }
        }

        public MapCore MapInstance
        {
            get { return mapInstance; }
            set
            {
                if (mapInstance != null)
                {
                    mapInstance.Loaded -= mapInstance_Loaded;
                    mapInstance.TargetViewChanged -= mapInstance_TargetViewChanged;
                }
                mapInstance = value;
                if (mapInstance != null)
                {
                    mapInstance.Loaded += mapInstance_Loaded;
                    mapInstance.TargetViewChanged += mapInstance_TargetViewChanged;
                    if (map != null)
                    {
                        map.CredentialsProvider = MapInstance.CredentialsProvider;
                    }
                }
            }
        }

        public void Dispose()
        {
            MapInstance = null;
        }

        #endregion

        private void MiniMap_Loaded(object sender, RoutedEventArgs e)
        {
            setMapInstance(MapName);
            TargetViewChanged();
        }

        private void mapInstance_Loaded(object sender, RoutedEventArgs e)
        {
            TargetViewChanged();
        }

        private void mapInstance_TargetViewChanged(object sender, MapEventArgs e)
        {
            TargetViewChanged();
        }

        private void setMapInstance(string mapname)
        {
            MapInstance = Utilities.FindVisualChildByName<MapCore>(Application.Current.RootVisual, mapname);
        }

        private void TargetViewChanged()
        {
            if (map != null && MapInstance != null)
            {
                //change center of mini map
                map.Center = MapInstance.TargetCenter;
                //make sure minimap is at least zoomlevel 1
                double level = MapInstance.TargetZoomLevel;
                map.ZoomLevel = (level + ZoomOffset) < 0 ? 1 : (level + ZoomOffset);
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            map = (Map) GetTemplateChild(PART_map);
            reverseServiceResultText = (TextBlock) GetTemplateChild(PART_reverseServiceResultText);
            reverseServiceResultText.Text = "";
            if (map != null)
            {
                if (MapInstance != null)
                {
                    map.CredentialsProvider = MapInstance.CredentialsProvider;
                }
                //turn off interactivity
                map.MouseClick += (o, e) => { e.Handled = true; };
                map.MouseDoubleClick += (o, e) => { e.Handled = true; };
                map.MouseWheel += (o, e) => { e.Handled = true; };
                map.MousePan += (o, e) => { e.Handled = true; };
                map.KeyDown += (o, e) => { e.Handled = true; };
                map.KeyHeld += (o, e) => { e.Handled = true; };
                map.MouseDragBox += (o, e) => { e.Handled = true; };

                map.ViewChangeEnd += map_ViewChangeEnd;
            }
            TargetViewChanged();
        }

        private void map_ViewChangeEnd(object sender, MapEventArgs e)
        {
            if ((map.ZoomLevel >= RvsGeocodeMinZoomLevel) && (RvsGeocodeEnabled))
            {
                RequestReverseService();
            }
        }

        private void RequestReverseService()
        {
            var client = new GeocodeServiceClient(new BasicHttpBinding(BasicHttpSecurityMode.None),
                                                  new EndpointAddress(
                                                      "http://dev.virtualearth.net/webservices/v1/GeocodeService/GeocodeService.svc"));

            client.ReverseGeocodeCompleted += (send, args) =>
                                                  {
                                                      try
                                                      {
                                                          if (args.Result != null && MapInstance != null)
                                                          {
                                                              if (MapInstance.ZoomLevel >= 1 &&
                                                                  MapInstance.ZoomLevel <= 8)
                                                              {
                                                                  reverseServiceResultText.Text =
                                                                      args.Result.Results[0].Address.CountryRegion;
                                                              }
                                                              else if (MapInstance.ZoomLevel >= 9 &&
                                                                       MapInstance.ZoomLevel <= 16)
                                                              {
                                                                  reverseServiceResultText.Text =
                                                                      args.Result.Results[0].Address.AdminDistrict +
                                                                      ", " +
                                                                      args.Result.Results[0].Address.CountryRegion;
                                                              }
                                                              else
                                                              {
                                                                  reverseServiceResultText.Text =
                                                                      args.Result.Results[0].Address.
                                                                          FormattedAddress;
                                                              }
                                                          }
                                                          else
                                                              reverseServiceResultText.Text = "";
                                                      }
                                                      catch
                                                      {
                                                          reverseServiceResultText.Text = "";
                                                      }
                                                  };
            var request = new ReverseGeocodeRequest
                              {
                                  Location =
                                      new Location {Latitude = map.Center.Latitude, Longitude = map.Center.Longitude}
                              };
            map.CredentialsProvider.GetCredentials(
                credentials =>
                {
                    //Pass in credentials for web services call.
                    request.Credentials = credentials;
                    client.ReverseGeocodeAsync(request);
                });
            
        }
    }
}