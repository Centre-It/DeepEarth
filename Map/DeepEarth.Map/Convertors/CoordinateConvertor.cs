// Deep Earth is a community project available under the Microsoft Public License (Ms-PL)
// Code is provided as is and with no warrenty – Use at your own risk
// View the project and the latest code at http://DeepEarth.codeplex.com/

using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using Point=System.Windows.Point;

namespace DeepEarth.Client.MapControl.Convertors
{
    public static class CoordinateConvertor
    {
        public static ICoordinate Convert(Point location)
        {
            return new Coordinate {Y = location.Y, X = location.X};
        }

        public static Point ConvertBack(ICoordinate location)
        {
            return new Point {X = location.Y, Y = location.X,};
        }
    }
}