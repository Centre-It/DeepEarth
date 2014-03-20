// Deep Earth is a community project available under the Microsoft Public License (Ms-PL)
// Code is provided as is and with no warrenty – Use at your own risk
// View the project and the latest code at http://DeepEarth.codeplex.com/

using Microsoft.Maps.MapControl.Core;

namespace DeepEarth.BingMapsToolkit.Client.MapGeometry.Drawing
{
    public class EditLineString : EditShapeBase
    {
        public EditLineString(MapCore mapInstance)
            : base(mapInstance)
        {
            IsClosed = false;
        }
    }
}