using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Maps.MapControl;
using System.Collections.Generic;

namespace DeepEarth.BingMapsToolkit.Client.Controls
{
	public class FindLayerEventArgs : EventArgs
	{
		public string Term { get; set; }
		public LocationRect SearchZone { get; set; }
		public Action<IEnumerable<FoundLayer>> OnGetResultLayersSearchComplete { get; set; }

		public FindLayerEventArgs()
		{}

		public FindLayerEventArgs(string term, LocationRect searchZone, Action<IEnumerable<FoundLayer>> onGetResultLayersSearchComplete)
		{
			Term = term;
			SearchZone = searchZone;
			OnGetResultLayersSearchComplete = onGetResultLayersSearchComplete;
		}
	}
}
