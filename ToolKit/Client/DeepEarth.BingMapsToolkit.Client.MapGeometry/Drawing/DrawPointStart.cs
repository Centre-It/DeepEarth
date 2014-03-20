// Deep Earth is a community project available under the Microsoft Public License (Ms-PL)
// Code is provided as is and with no warrenty – Use at your own risk
// View the project and the latest code at http://DeepEarth.codeplex.com/

using System.Windows.Controls;
using Microsoft.Maps.MapControl.Core;

namespace DeepEarth.BingMapsToolkit.Client.MapGeometry.Drawing
{
    public class DrawPointStart : DrawPoint
    {
        public DrawPointStart(MapCore mapInstance)
            : base(mapInstance)
        {
            DefaultStyleKey = typeof(DrawPointStart);
            ToolTipService.SetToolTip(this,"Click here to save the shape.");
        }
    }
}