/// Deep Earth is a community project available under the Microsoft Public License (Ms-PL)
/// Code is provided as is and with no warrenty – Use at your own risk
/// View the project and the latest code at http://codeplex.com/deepearth/

using System;
using System.Collections.Generic;
using System.Windows;
using DeepEarth.Client.Services.Bing.VERouteService;

namespace DeepEarth.Client.Services.Bing
{
    public class Route
    {
        private string appID;

        public Route(string ApplicationID)
        {
            appID = ApplicationID;
            TravelMode = TravelMode.Driving;
            RouteOptimization = RouteOptimization.MinimizeTime;
            TrafficUsage = TrafficUsage.None;
        }

        public TravelMode TravelMode { get; set; }
        public RouteOptimization RouteOptimization { get; set; }
        public TrafficUsage TrafficUsage { get; set; }

        public void GetDirections(List<Point> locations, EventHandler onResults)
        {

            var routeRequest = new RouteRequest {Credentials = new Credentials {ApplicationId = appID}};

            var waypoints = new List<Waypoint>();
            foreach (Point loc in locations)
            {
                waypoints.Add(new Waypoint {Location = new Location {Longitude = loc.X, Latitude = loc.Y}});
            }
            routeRequest.Waypoints = waypoints;
            routeRequest.Options = new RouteOptions
                                       {
                                           Mode = TravelMode,
                                           Optimization = RouteOptimization,
                                           RoutePathType = RoutePathType.Points,
                                           TrafficUsage = TrafficUsage
                                       };

            var routeService = new RouteServiceClient();
            routeService.CalculateRouteCompleted += (o, e) =>
                                                        {
                                                            RouteResponse routeResponse = e.Result;
                                                            onResults(this, new RouteResultArgs {Result = routeResponse.Result});
                                                        };
            routeService.CalculateRouteAsync(routeRequest);

        }
    }

    public class RouteResultArgs : EventArgs
    {
        public RouteResult Result { get; set; }
    }
}