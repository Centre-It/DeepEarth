// Deep Earth is a community project available under the Microsoft Public License (Ms-PL)
// Code is provided as is and with no warrenty – Use at your own risk
// View the project and the latest code at http://DeepEarth.codeplex.com/

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
	public class GetLayerLocationsEventArgs : EventArgs
	{
		public string LayerID { get; set; }
		public Action<List<string>> OnGetLayerLocationsComplete;

		public GetLayerLocationsEventArgs()
		{
			LayerID = "";
			OnGetLayerLocationsComplete = u => { ;};
		}

		public GetLayerLocationsEventArgs(string layerID, Action<List<string>> onGetLayerLocationsComplete)
		{
			LayerID = layerID;
			OnGetLayerLocationsComplete = onGetLayerLocationsComplete;
		}
	}
}
