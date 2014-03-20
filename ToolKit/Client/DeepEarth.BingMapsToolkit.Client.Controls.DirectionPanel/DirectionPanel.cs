// Deep Earth is a community project available under the Microsoft Public License (Ms-PL)
// Code is provided as is and with no warrenty – Use at your own risk
// View the project and the latest code at http://DeepEarth.codeplex.com/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.Windows;
using System.Windows.Browser;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using DeepEarth.BingMapsToolkit.Client.Common;
using DeepEarth.BingMapsToolkit.Client.Common.Converters;
using DeepEarth.BingMapsToolkit.Client.Common.Entities;
using DeepEarth.BingMapsToolkit.Client.Controls.GeoCodeService;
using DeepEarth.BingMapsToolkit.Client.Controls.RouteService;
using DeepEarth.BingMapsToolkit.Client.MapGeometry;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using Microsoft.Maps.MapControl;
using Microsoft.Maps.MapControl.Core;
using Location=Microsoft.Maps.MapControl.Location;
using Point=GisSharpBlog.NetTopologySuite.Geometries.Point;

namespace DeepEarth.BingMapsToolkit.Client.Controls
{
    public delegate void DirectionEventHandler(object sender, DirectionArgs args);

    public class DirectionArgs : EventArgs
    {
        public ICoordinate Coordinate { get; set; }
    }


    [TemplateVisualState(Name = VSM_Normal, GroupName = VSM_CommonStates)]
    [TemplateVisualState(Name = VSM_MouseOver, GroupName = VSM_CommonStates)]
    public class DirectionPanel : Control, IMapControl<MapCore>
    {
        private const string PART_bClear = "PART_bClear";
        private const string PART_FromToggleButton = "PART_FromToggleButton";
        private const string PART_sokFrom = "PART_sokFrom";
        private const string PART_sokTo = "PART_sokTo";
        private const string PART_tbFrom = "PART_tbFrom";
        private const string PART_tbTo = "PART_tbTo";
        private const string PART_ToToggleButton = "PART_ToToggleButton";
        private const string PART_RadioWalking = "PART_RadioWalking";
        private const string PART_RadioDriving = "PART_RadioDriving";
        private const string PART_RadioTime = "PART_RadioTime";
        private const string PART_RadioDistance = "PART_RadioDistance";

        private const string VSM_CommonStates = "CommonStates";
        private const string VSM_MouseOver = "MouseOver";
        private const string VSM_Normal = "Normal";

        private Button bClear;

        private FindAddressPoint findAddressPoint;
        private Button findFrom;
        private Button findTo;
        private AddressCoordinate fromAddress;
        private EnhancedMapLayer fromLayer;

        private EditGeometry fromPoint;
        private TextBox fromTextBox;
        private ToggleButton fromToggleButton;
        private GeocodeServiceClient geoCodeService;
        private bool isMouseOver;
        private RadioButton radioWalking;
        private RadioButton radioDriving;
        private RadioButton radioTime;
        private RadioButton radioDistance;

        private MapCore mapInstance;
        private string mapName;
        private EnhancedMapLayer routeLayer;
        private RouteServiceClient routeService;
        private Dictionary<string, StyleSpecification> styles;
        private AddressCoordinate toAddress;
        private EnhancedMapLayer toLayer;
        private EditGeometry toPoint;
        private TextBox toTextBox;
        private ToggleButton toToggleButton;

        public DirectionPanel()
        {
            IsEnabled = false;
            DefaultStyleKey = typeof (DirectionPanel);
            Loaded += DirectionPanel_Loaded;
        }

        public SelectGeometry CurrentGeometry { get; set; }
        RouteOptimization routeOptimization;
        public RouteOptimization RouteOptimization 
        {
            get 
            {
                if (routeOptimization == null)
                    routeOptimization = new RouteOptimization();
                if (radioDistance.IsChecked.GetValueOrDefault(false))
                    routeOptimization = RouteOptimization.MinimizeDistance;
                else
                    routeOptimization = RouteOptimization.MinimizeTime;
                return routeOptimization; 
            }
            set
            {
                if (routeOptimization == null)
                    routeOptimization = new RouteOptimization();
                routeOptimization = value;
            }
        }

        TravelMode travelMode;
        public TravelMode TravelMode 
        {
            get
            {
                if (travelMode == null)
                    travelMode = new TravelMode();
                if (radioDriving.IsChecked.GetValueOrDefault(false))
                    travelMode = TravelMode.Driving;
                else
                    travelMode = TravelMode.Walking;
                return travelMode;
            }
            set
            {
                if (travelMode == null)
                    travelMode = new TravelMode();
                travelMode = value;
            }
        }

        public Location CurrentLocation { get; set; }

        private RouteServiceClient RouteClient
        {
            get
            {
                if (null == routeService)
                {
                    //Handle http/https; OutOfBrowser is currently supported on the MapControl only for http pages
                    bool httpsUriScheme = !Application.Current.IsRunningOutOfBrowser &&
                                          HtmlPage.Document.DocumentUri.Scheme.Equals(Uri.UriSchemeHttps);

                    BasicHttpBinding binding = httpsUriScheme
                                                   ? new BasicHttpBinding(BasicHttpSecurityMode.Transport)
                                                   : new BasicHttpBinding(BasicHttpSecurityMode.None);
                    var serviceUri =
                        new UriBuilder("http://dev.virtualearth.net/webservices/v1/routeservice/routeservice.svc");
                    if (httpsUriScheme)
                    {
                        //For https, change the UriSceheme to https and change it to use the default https port.
                        serviceUri.Scheme = Uri.UriSchemeHttps;
                        serviceUri.Port = -1;
                    }
                    binding.MaxReceivedMessageSize = 2147483647;
                    routeService = new RouteServiceClient(binding, new EndpointAddress(serviceUri.Uri));
                    routeService.CalculateRouteCompleted += routeService_CalculateRouteCompleted;
                    return routeService;
                }
                return null;
            }
        }

        private GeocodeServiceClient GeocodeClient
        {
            get
            {
                if (null == geoCodeService)
                {
                    //Handle http/https; OutOfBrowser is currently supported on the MapControl only for http pages
                    bool httpsUriScheme = !Application.Current.IsRunningOutOfBrowser &&
                                          HtmlPage.Document.DocumentUri.Scheme.Equals(Uri.UriSchemeHttps);
                    BasicHttpBinding binding = httpsUriScheme
                                                   ? new BasicHttpBinding(BasicHttpSecurityMode.Transport)
                                                   : new BasicHttpBinding(BasicHttpSecurityMode.None);
                    var serviceUri =
                        new UriBuilder("http://dev.virtualearth.net/webservices/v1/GeocodeService/GeocodeService.svc");
                    if (httpsUriScheme)
                    {
                        //For https, change the UriSceheme to https and change it to use the default https port.
                        serviceUri.Scheme = Uri.UriSchemeHttps;
                        serviceUri.Port = -1;
                    }

                    //Create the Service Client
                    geoCodeService = new GeocodeServiceClient(
                        binding, new EndpointAddress(serviceUri.Uri));

                    geoCodeService.GeocodeCompleted += geoCodeService_GeocodeCompleted;
                    GeocodeClient.ReverseGeocodeCompleted += geoCodeService_ReverseGeocodeCompleted;
                }
                return geoCodeService;
            }
        }

        public AddressCoordinate FromAddress
        {
            get { return fromAddress; }
            set
            {
                if (value == null)
                {
                    fromTextBox.Text = "";
                    fromAddress = null;
                    return;
                }

                if (fromAddress == null)
                    fromAddress = new AddressCoordinate();
                fromAddress.Location = new Location();
                fromAddress.Location = value.Location;
                fromAddress.DisplayName = value.DisplayName;
                fromTextBox.Text = value.DisplayName;
            }
        }

        public AddressCoordinate ToAddress
        {
            get { return toAddress; }
            set
            {
                if (value == null)
                {
                    toTextBox.Text = "";
                    toAddress = null;
                    return;
                }
                if (toAddress == null)
                    toAddress = new AddressCoordinate();
                toAddress.Location = new Location();
                toAddress.Location = value.Location;
                toAddress.DisplayName = value.DisplayName;
                toTextBox.Text = value.DisplayName;
            }
        }

        #region MouseEvents

        private void mouseLeave(object sender, MouseEventArgs e)
        {
            isMouseOver = false;
            if (IsEnabled)
            {
                GoToState(true);
            }
        }

        private void mouseEnter(object sender, MouseEventArgs e)
        {
            isMouseOver = true;
            if (IsEnabled)
            {
                GoToState(true);
            }
        }

        #endregion

        #region IMapControl<MapCore> Members

        public void Dispose()
        {
            MouseEnter -= mouseEnter;
            MouseLeave -= mouseLeave;
            fromToggleButton.Checked -= fromToggleButton_Checked;
            fromToggleButton.Unchecked -= fromToggleButton_Unchecked;
            toToggleButton.Checked -= toToggleButton_Checked;
            toToggleButton.Unchecked -= toToggleButton_Unchecked;
            fromPoint.EditChange -= fromPoint_EditChanged;
            MapInstance.MouseClick -= MapInstance_MouseClick;
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

        public MapCore MapInstance
        {
            get { return mapInstance; }
            set
            {
                if (mapInstance != null)
                {
                    MapInstance.MouseClick -= MapInstance_MouseClick;
                }
                mapInstance = value;
                if (mapInstance != null)
                {
                    MapInstance.MouseClick += MapInstance_MouseClick;
                }
            }
        }

        #endregion

        public event DirectionEventHandler OnRouteNodeChanged;

        private void DirectionPanel_Loaded(object sender, RoutedEventArgs e)
        {
            GoToState(true);
            MouseEnter += mouseEnter;
            MouseLeave += mouseLeave;


            setMapInstance(MapName);

            toPoint = new EditGeometry {MapInstance = MapInstance};
            fromPoint = new EditGeometry {MapInstance = MapInstance};

            geoCodeService = GeocodeClient;
            routeService = RouteClient;


            fromLayer = new EnhancedMapLayer(MapInstance) {ID = "from layer", ZIndex = 0, MinZoomLevel = 1, MaxZoomLevel = 21};
            toLayer = new EnhancedMapLayer(MapInstance) { ID = "to layer", ZIndex = 0, MinZoomLevel = 1, MaxZoomLevel = 21 };
            routeLayer = new EnhancedMapLayer(MapInstance) { ID = "route layer", ZIndex = 0, MinZoomLevel = 1, MaxZoomLevel = 21 };

            MapInstance.Children.Insert(0, fromLayer);
            MapInstance.Children.Insert(1, toLayer);
            MapInstance.Children.Insert(2, routeLayer);


            styles = new Dictionary<string, StyleSpecification>();
            styles.Add("directionStyle", new StyleSpecification
                                             {
                                                 ID = "directionStyle",
                                                 LineColour = "FF1B0AA5",
                                                 LineWidth = 20,
                                                 PolyFillColour = "88677E1E",
                                                 ShowFill = true,
                                                 ShowLine = true,
                                                 IconURL = "http://soulsolutions.com.au/Images/pin.png",
                                                 IconScale = 1.4
                                             });

            OnRouteNodeChanged += DirectionPanel_OnRouteNodeChanged;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            fromToggleButton = (ToggleButton) GetTemplateChild(PART_FromToggleButton);
            toToggleButton = (ToggleButton) GetTemplateChild(PART_ToToggleButton);

            fromToggleButton.Checked += fromToggleButton_Checked;
            fromToggleButton.Unchecked += fromToggleButton_Unchecked;

            toToggleButton.Checked += toToggleButton_Checked;
            toToggleButton.Unchecked += toToggleButton_Unchecked;

            fromTextBox = (TextBox) GetTemplateChild(PART_tbFrom);
            fromTextBox.KeyDown += fromTextBox_KeyDown;

            toTextBox = (TextBox) GetTemplateChild(PART_tbTo);
            toTextBox.KeyDown += toTextBox_KeyDown;

            findFrom = (Button) GetTemplateChild(PART_sokFrom);
            findFrom.Click += sok_Click;

            findTo = (Button) GetTemplateChild(PART_sokTo);
            findTo.Click += sok_Click;

            bClear = (Button) GetTemplateChild(PART_bClear);
            bClear.Click += bClear_Click;

            radioDistance = (RadioButton)GetTemplateChild(PART_RadioDistance);
            radioDriving = (RadioButton)GetTemplateChild(PART_RadioDriving);
            radioTime = (RadioButton)GetTemplateChild(PART_RadioTime);
            radioWalking = (RadioButton)GetTemplateChild(PART_RadioWalking);

            IsEnabled = true;
        }

        private void toTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                sok_Click(findTo, null);
            }
        }

        private void fromTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                sok_Click(findFrom, null);
            }
        }

        private void bClear_Click(object sender, RoutedEventArgs e)
        {
            findAddressPoint = FindAddressPoint.None;
            routeLayer.Clear();
            fromLayer.Clear();
            toLayer.Clear();

            if (FromAddress != null)
            {
                FromAddress.DisplayName = "";
                FromAddress.Location = null;
                FromAddress = null;
            }

            if (ToAddress != null)
            {
                ToAddress.DisplayName = "";
                ToAddress.Location = null;
                ToAddress = null;
            }

            UpdateToggleStatus(null);
        }

        private void sok_Click(object sender, RoutedEventArgs e)
        {
            var req = new GeocodeRequest();
            if (sender.Equals(findFrom))
            {
                if (geoCodeService != null && !string.IsNullOrEmpty(fromTextBox.Text.Trim()))
                {
                    fromLayer.Children.Clear();
                    findAddressPoint = FindAddressPoint.From;
                    req.Query = fromTextBox.Text.Trim();
                }
            }
            else if (sender.Equals(findTo))
            {
                if (geoCodeService != null && !string.IsNullOrEmpty(toTextBox.Text.Trim()))
                {
                    toLayer.Children.Clear();
                    findAddressPoint = FindAddressPoint.To;
                    req.Query = toTextBox.Text.Trim();
                }
            }
            req.Culture = MapInstance.Culture;
            MapInstance.CredentialsProvider.GetCredentials(
                credentials =>
                    {
                        req.Credentials = credentials;
                        GeocodeClient.GeocodeAsync(req, findAddressPoint);
                    }
                );
        }

        private void GoToState(bool useTransitions)
        {
            if (isMouseOver)
            {
                VisualStateManager.GoToState(this, VSM_MouseOver, useTransitions);
            }
            else
            {
                VisualStateManager.GoToState(this, VSM_Normal, useTransitions);
            }
        }

        private void setMapInstance(string mapname)
        {
            MapInstance = Utilities.FindVisualChildByName<MapCore>(Application.Current.RootVisual, mapname);
        }

        private void MapInstance_MouseClick(object sender, MapMouseEventArgs arg)
        {
            if (MapInstance != null &&
                (fromToggleButton.IsChecked.GetValueOrDefault(false) ||
                 toToggleButton.IsChecked.GetValueOrDefault(false)))
            {
                Location location = MapInstance.ViewportPointToLocation(arg.ViewportPoint);
                var coordinate = new Coordinate(
                    location.Latitude, location.Longitude);

                OnRouteNodeChanged(this, new DirectionArgs
                                             {
                                                 Coordinate = coordinate
                                             }
                    );
            }
        }

        private void DirectionPanel_OnRouteNodeChanged(object sender, DirectionArgs args)
        {
            IGeometry geom = new Point(args.Coordinate);

            // Call bing maps webservice
            var req = new ReverseGeocodeRequest();
            req.Location = new Location(geom.Coordinate.X, geom.Coordinate.Y);

            MapInstance.CredentialsProvider.GetCredentials(
                credentials =>
                    {
                        req.Credentials = credentials;
                        GeocodeClient.ReverseGeocodeAsync(req, findAddressPoint);
                    });
        }

        private void geoCodeService_GeocodeCompleted(object sender, GeocodeCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show("Something went wrong: " + e.Error.Message);
                return;
            }

            if (e.Result.Results.Count == 0)
                return;

            var location = new Location
                               {
                                   Latitude = e.Result.Results[0].Locations[0].Latitude,
                                   Longitude = e.Result.Results[0].Locations[0].Longitude
                               };

            var AddressCoord = new AddressCoordinate
                                   {
                                       Location = location,
                                       DisplayName = e.Result.Results[0].Address.AddressLine
                                   };

            if (findAddressPoint == FindAddressPoint.From)
            {
                AddressCoord.DisplayName = fromTextBox.Text.Trim();
                FromAddress = AddressCoord;
                RenderGeometry(location, fromLayer);
            }
            else
            {
                AddressCoord.DisplayName = toTextBox.Text.Trim();
                ToAddress = AddressCoord;
                RenderGeometry(location, toLayer);
            }

            TriggerCalcRoute();
        }

        private void geoCodeService_ReverseGeocodeCompleted(object sender, ReverseGeocodeCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message);
            }
            else if (e.Result != null && e.Result.Results.Count > 0)
            {
                var location = new Location
                                   {
                                       Latitude = e.Result.Results[0].Locations[0].Latitude,
                                       Longitude = e.Result.Results[0].Locations[0].Longitude
                                   };

                var Address = new AddressCoordinate
                                  {
                                      Location = location,
                                      DisplayName = e.Result.Results[0].DisplayName
                                  };

                if (findAddressPoint == FindAddressPoint.From)
                {
                    FromAddress = Address;
                    RenderGeometry(location, fromLayer);
                }
                else
                {
                    ToAddress = Address;
                    RenderGeometry(location, toLayer);
                }
                TriggerCalcRoute();
            }
        }

        private void TriggerCalcRoute()
        {
            if (FromAddress != null && ToAddress != null && findAddressPoint != FindAddressPoint.None)
                CalcRoute();
            UpdateToggleStatus(null);
        }

        private void CalcRoute()
        {
            if (routeService != null && findAddressPoint != FindAddressPoint.None)
            {
                var req = new RouteRequest();
                var fromWP = new Waypoint();
                fromWP.Location = new Location();
                fromWP.Location = FromAddress.Location;
                fromWP.Description = FromAddress.DisplayName;

                var toWP = new Waypoint();
                toWP.Location = new Location();
                toWP.Location = ToAddress.Location;
                toWP.Description = ToAddress.DisplayName;

                req.Waypoints = new ObservableCollection<Waypoint>();
                req.Waypoints.Add(fromWP);
                req.Waypoints.Add(toWP);
                req.Options = new RouteOptions() {RoutePathType = RoutePathType.Points};
                
                req.Options.Optimization = RouteOptimization;
                req.Options.Mode = TravelMode;
                
                req.Culture = MapInstance.Culture;

                MapInstance.CredentialsProvider.GetCredentials(
                    credentials =>
                        {
                            req.Credentials = credentials;
                            routeService.CalculateRouteAsync(req);
                        }
                    );
            }
        }

        private void routeService_CalculateRouteCompleted(object sender, CalculateRouteCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message);
                return;
            }

            var coordinates =
                new Coordinate[e.Result.Result.RoutePath.Points.Count];

            if (e.Result.Result.RoutePath != null)
            {
                int i = 0;
                foreach (Location item in e.Result.Result.RoutePath.Points)
                {
                    coordinates[i] = new Coordinate
                                         {
                                             X = item.Longitude,
                                             Y = item.Latitude
                                         };
                    i++;
                }
            }

            LocationCollection locationCollection = CoordinateConvertor.CoordinatesToLocationCollection(coordinates);

            var rect = new LocationRect(locationCollection);

            var ls = new LineString(coordinates);

            var selectGeom = new SelectGeometry(ls, styles["directionStyle"], MapInstance, routeLayer);

            RenderGeometry(selectGeom.Geometry, routeLayer);

            MapInstance.SetView(rect);
        }

        #region RenderGeometry

        private void RenderGeometry(Location location, EnhancedMapLayer layer)
        {
            var point = new Point(location.Longitude, location.Latitude);
            RenderGeometry(point, layer);
        }

        private void RenderGeometry(IGeometry geometry, EnhancedMapLayer layer)
        {
            layer.Children.Clear();
            CurrentGeometry = new SelectGeometry(geometry, styles["directionStyle"], MapInstance, layer);
            //TODO: What type of object is added here? Please fix.
            //layer.AddGeometry(CurrentGeometry);
        }

        #endregion

        #region ToggleButtons

        private void toToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdateToggleStatus(null);
        }

        private void fromToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdateToggleStatus(null);
        }

        private void toToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            findAddressPoint = FindAddressPoint.To;
            UpdateToggleStatus(sender);
        }

        private void fromToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            findAddressPoint = FindAddressPoint.From;
            UpdateToggleStatus(sender);
        }

        private void fromPoint_EditChanged(object sender, EditGeometryChangedEventArgs args)
        {
            var info = new InfoGeometry(fromPoint.Geometry, new StyleSpecification(), MapInstance, routeLayer);
        }

        private void UpdateToggleStatus(object sender)
        {
            if (fromToggleButton != null && fromToggleButton != sender &&
                toToggleButton.IsChecked.GetValueOrDefault(false))
            {
                fromToggleButton.IsChecked = false;
            }

            if (toToggleButton != null && toToggleButton != sender && toToggleButton.IsChecked.GetValueOrDefault(false))
            {
                toToggleButton.IsChecked = false;
            }

            if (toToggleButton != null && fromToggleButton != null && sender == null)
            {
                toToggleButton.IsChecked = false;
                fromToggleButton.IsChecked = false;
            }
        }

        #endregion-

        #region Nested type: FindAddressPoint

        private enum FindAddressPoint
        {
            None,
            From,
            To
        }

        #endregion
    }
}