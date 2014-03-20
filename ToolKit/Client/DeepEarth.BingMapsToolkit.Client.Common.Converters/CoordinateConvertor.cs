// Deep Earth is a community project available under the Microsoft Public License (Ms-PL)
// Code is provided as is and with no warrenty – Use at your own risk
// View the project and the latest code at http://DeepEarth.codeplex.com/

using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using Microsoft.Maps.MapControl;
using ProjNet.CoordinateSystems.Transformations;
using Location=Microsoft.Maps.MapControl.Location;


namespace DeepEarth.BingMapsToolkit.Client.Common.Converters
{
    public static class CoordinateConvertor
    {
        public static ICoordinate Convert(Location location)
        {
            return new Coordinate {Y = location.Latitude, X = location.Longitude};
        }

        public static Location ConvertBack(ICoordinate location)
        {
            return new Location {Latitude = location.Y, Longitude = location.X};
        }

        public static LocationCollection CoordinatesToLocationCollection(ICoordinate[] coordinates)
        {
            var locations = new LocationCollection();
            foreach (var coordinate in coordinates)
            {
                locations.Add(ConvertBack(coordinate));
            }
            return locations;
        }

        public static ICoordinate[] LocationCollectionToCoordinates(LocationCollection locations)
        {
            var coordinates = new Coordinate[locations.Count];
            for (var x = 0; x < locations.Count;x++ )
            {
                coordinates[x] = (Coordinate)Convert(locations[x]);
            }
            return coordinates;
        }

        public static Coordinate ConvertDegreesToUTM(ICoordinate coordinate)
        {
            var ctfac = new CoordinateTransformationFactory();
            ProjNet.CoordinateSystems.ICoordinateSystem wgs84geo = ProjNet.CoordinateSystems.GeographicCoordinateSystem.WGS84;
            int zone = (int)System.Math.Ceiling((coordinate.X + 180) / 6);
            ProjNet.CoordinateSystems.ICoordinateSystem utm = ProjNet.CoordinateSystems.ProjectedCoordinateSystem.WGS84_UTM(zone, coordinate.Y > 0);
            var trans = ctfac.CreateFromCoordinateSystems(wgs84geo, utm);

            double[] pUtm = trans.MathTransform.Transform(new[] { coordinate.X, coordinate.Y });

            return new Coordinate
                       { 
                           X = pUtm[0],
                           Y = pUtm[1],
                       };
        }

        public static LocationCollection LocationRectToLocationCollection(LocationRect locationRect)
        {
            var locations = new LocationCollection
                                {
                                    locationRect.Northwest,
                                    locationRect.Southwest,
                                    locationRect.Southeast,
                                    locationRect.Northeast,
                                    locationRect.Northwest
                                };
            return locations;
        }
    }
}