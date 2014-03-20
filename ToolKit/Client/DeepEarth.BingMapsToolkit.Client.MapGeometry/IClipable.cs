// Deep Earth is a community project available under the Microsoft Public License (Ms-PL)
// Code is provided as is and with no warrenty – Use at your own risk
// View the project and the latest code at http://DeepEarth.codeplex.com/

using Microsoft.Maps.MapControl;

namespace DeepEarth.BingMapsToolkit.Client.MapGeometry
{
    public interface IClipable
    {
        LocationRect MBR { get; set; }
    }
}
