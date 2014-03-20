// Deep Earth is a community project available under the Microsoft Public License (Ms-PL)
// Code is provided as is and with no warrenty – Use at your own risk
// View the project and the latest code at http://DeepEarth.codeplex.com/

using System;
using DeepEarth.BingMapsToolkit.Client.Controls;
using Microsoft.Maps.MapControl;

namespace ExampleControlBing.ControlDemos
{
    public partial class StatusPanelExample
    {
        public StatusPanelExample()
        {
            InitializeComponent();
            map.ViewChangeEnd += map_ViewChangeEnd;
        }

        private void map_ViewChangeEnd(object sender, MapEventArgs e)
        {
            StatusCommands.ReportStatusCommand.Execute(String.Format("Lon:{0} Lat:{1}", map.Center.Longitude,
                                                                     map.Center.Latitude));
        }
    }
}