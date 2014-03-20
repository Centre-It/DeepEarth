// Deep Earth is a community project available under the Microsoft Public License (Ms-PL)
// Code is provided as is and with no warrenty – Use at your own risk
// View the project and the latest code at http://DeepEarth.codeplex.com/

using System;

namespace DeepEarth.BingMapsToolkit.Client.Common
{
    [Flags]
    public enum GeometryType : ulong
    {
        None = 0,
        Point = 1,
        LineString = 2,
        Polygon = 4,
        MultiPoint = 8,
        MultiLineString = 16,
        MultiPolygon = 32,
        GeometryCollection = 64
    }
}
