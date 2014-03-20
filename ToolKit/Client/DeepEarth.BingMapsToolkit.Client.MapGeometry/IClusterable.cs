// Deep Earth is a community project available under the Microsoft Public License (Ms-PL)
// Code is provided as is and with no warrenty – Use at your own risk
// View the project and the latest code at http://DeepEarth.codeplex.com/

using System;
using System.Collections.Generic;
using System.Windows;
using Microsoft.Maps.MapControl;

namespace DeepEarth.BingMapsToolkit.Client.MapGeometry
{
    public interface IClusterable
    {
        event EventHandler ProjectionComplete;

        Location Location { get; set; }
        bool Initalized { get; set; }
        Dictionary<int, Point> ProjectedPoints { get; set; }
        List<IClusterable> ClusteredElements { get; set; }
        bool IsCluster { get; set; }
        bool IsClustered { get; set; }
    }
}
