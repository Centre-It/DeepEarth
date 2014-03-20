// Deep Earth is a community project available under the Microsoft Public License (Ms-PL)
// Code is provided as is and with no warrenty – Use at your own risk
// View the project and the latest code at http://DeepEarth.codeplex.com/

using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Windows.Browser;

using DeepEarth.BingMapsToolkit.Client.Controls;

namespace ExampleControlBing.ControlDemos
{
	public partial class AutomationExample : UserControl
	{
		public AutomationExample()
		{
			InitializeComponent();

			automation.OnGetFeatureLocation += automation_OnGetFeatureLocation;
			automation.OnGetLayerLocations += automation_OnGetLayerLocations;
		}

		void automation_OnGetLayerLocations(object sender, GetLayerLocationsEventArgs e)
		{
			List<string> features = new List<string>();
			features.Add("1");
			features.Add("2");
			e.OnGetLayerLocationsComplete(features);
		}

		void automation_OnGetFeatureLocation(object sender, GetFeatureLocationEventArgs e)
		{
			if (e.ItemID.CompareTo("1") == 0)
				e.OnGetFeatureLocationComplete(new Microsoft.Maps.MapControl.LocationRect(new Microsoft.Maps.MapControl.Location(35, -70), new Microsoft.Maps.MapControl.Location(45, -80)));
			if (e.ItemID.CompareTo("2") == 0)
			e.OnGetFeatureLocationComplete(new Microsoft.Maps.MapControl.LocationRect(new Microsoft.Maps.MapControl.Location(5, 5), new Microsoft.Maps.MapControl.Location(10, 10)));
		}

		private void scenario1_Click(object sender, RoutedEventArgs e)
		{
			automation.Process(@"<Root> <ForEachRecordInLayer layerid='Layer 2'>  <ZoomToRecord /> <Pause time='500' /> <ZoomToLevel level='1'/> <Pause time='1000' /> </ForEachRecordInLayer>  <ZoomToLevel level='1'/> </Root>");
		}

		private void scenario2_Click(object sender, RoutedEventArgs e)
		{
			automation.Process(@"<Root> <ForEachRecordInLayer layerid='Layer 2'> <ZoomToRecord />  <Pause time='1000' /> <ZoomToLevel level='10'/> <Pause time='1000' /> </ForEachRecordInLayer> </Root>");
		}

		private void scenario3_Click(object sender, RoutedEventArgs e)
		{
			automation.Process(@"<Root> <ZoomToLevel level='2'/> </Root>");
		}

		private void scenario4_Click(object sender, RoutedEventArgs e)
		{
			automation.Process(@"<Root> <ZoomToXY value='60,60'/> </Root>");
		}
	}
}
