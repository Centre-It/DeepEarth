/// Deep Earth is a community project available under the Microsoft Public License (Ms-PL)
/// Code is provided as is and with no warrenty – Use at your own risk
/// View the project and the latest code at http://codeplex.com/deepearth/

using System;
using System.Collections.Generic;
using System.Windows;
using DeepEarth.Client.Services.Bing.VEGeocodeService;

namespace DeepEarth.Client.Services.Bing
{
    public class Geocode
    {
        private readonly string appID;

        public Geocode(string ApplicationID)
        {
            appID = ApplicationID;
        }

        public void Find(string location, EventHandler onResults)
        {
            var geocodeRequest = new GeocodeRequest
                                     {
                                         Credentials = new Credentials {ApplicationId = appID},
                                         Query = location
                                     };

            var geocodeService = new GeocodeServiceClient();
            geocodeService.GeocodeCompleted += (o, e) =>
                                                   {
                                                       GeocodeResponse geocodeResponse = e.Result;
                                                       onResults(this, new GeocodeResultArgs {Results = geocodeResponse.Results});
                                                   };
            geocodeService.GeocodeAsync(geocodeRequest);
        }

        public void Find(Point location, EventHandler onResults)
        {
  
            var reverseGeocodeRequest = new ReverseGeocodeRequest
                                            {
                                                Credentials = new Credentials {ApplicationId = appID},
                                                Location = new Location {Longitude = location.X, Latitude = location.Y}
                                            };

            var geocodeService = new GeocodeServiceClient();

            geocodeService.ReverseGeocodeCompleted += (o, e) =>
                                                          {
                                                              GeocodeResponse geocodeResponse = e.Result;
                                                              onResults(this, new GeocodeResultArgs {Results = geocodeResponse.Results});
                                                          };
            geocodeService.ReverseGeocodeAsync(reverseGeocodeRequest);

        }

    }

    public class GeocodeResultArgs : EventArgs
    {
        public List<GeocodeResult> Results { get; set; }
    }
}