﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using DeepEarth.BingMapsToolkit.Client.Controls;

namespace ExampleControlBing.ControlDemos
{
	public partial class MapRobotExample : UserControl
	{
		public MapRobotExample()
		{
			InitializeComponent();

			Loaded += MapRobotExample_Loaded;
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
			if (e.LayerID.CompareTo("2") == 0)
			{
				if (e.ItemID.CompareTo("1") == 0)
					e.OnGetFeatureLocationComplete(new Microsoft.Maps.MapControl.LocationRect(new Microsoft.Maps.MapControl.Location(35, -70), new Microsoft.Maps.MapControl.Location(45, -80)));
				if (e.ItemID.CompareTo("2") == 0)
					e.OnGetFeatureLocationComplete(new Microsoft.Maps.MapControl.LocationRect(new Microsoft.Maps.MapControl.Location(5, 5), new Microsoft.Maps.MapControl.Location(10, 10)));
			}
			else if (e.LayerID.CompareTo("1") == 0)
			{
				if (e.ItemID.CompareTo("1") == 0)
					e.OnGetFeatureLocationComplete(new Microsoft.Maps.MapControl.LocationRect(new Microsoft.Maps.MapControl.Location(5, 5), new Microsoft.Maps.MapControl.Location(10, 10)));
				if (e.ItemID.CompareTo("2") == 0)
					e.OnGetFeatureLocationComplete(new Microsoft.Maps.MapControl.LocationRect(new Microsoft.Maps.MapControl.Location(35, -70), new Microsoft.Maps.MapControl.Location(45, -80)));
			}

		}

		void MapRobotExample_Loaded(object sender, RoutedEventArgs e)
		{
			List<Workflow> workflows = new List<Workflow>();

			workflows.Add(new Workflow()
			{
				Name = "From New York to Nigeria",
				Maml = @"<Root> <ForEachRecordInLayer layerid='2'> <ZoomToRecord /> <Pause time='500' /> <Pause time='1000' /> </ForEachRecordInLayer> </Root>"
			});
			workflows.Add(new Workflow()
			{
				Name = "From Nigeria to New York",
				Maml = @"<Root> <ForEachRecordInLayer layerid='1'> <ZoomToRecord /> <Pause time='1000' /> <ZoomToLevel level='9'/> <Pause time='1000' /> </ForEachRecordInLayer> </Root>"
			});
			workflows.Add(new Workflow()
			{
				Name = "Zoom level(2)",
				Maml = @"<Root> <ZoomToLevel level='2'/> </Root>"
			});
			workflows.Add(new Workflow()
			{
				Name = "Zoom to coordinates(60,60)",
				Maml = @"<Root> <ZoomToXY value='60,60'/> </Root>"
			});

			maprobot.Workflows = workflows;
		}
	}
}