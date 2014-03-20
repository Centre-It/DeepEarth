// Deep Earth is a community project available under the Microsoft Public License (Ms-PL)
// Code is provided as is and with no warrenty – Use at your own risk
// View the project and the latest code at http://DeepEarth.codeplex.com/

using System;
using System.Collections.ObjectModel;
using DeepEarth.BingMapsToolkit.Client.MapGeometry;
using DeepEarth.BingMapsToolkit.Common.Entities;

namespace DeepEarth.BingMapsToolkit.Client.Controls
{
    public delegate void BalloonLaunchEventHandler(object sender, BalloonEventArgs args);

    public delegate void LoadLayerEventHandler(object sender, LoadLayerEventArgs args, Action<ObservableCollection<VectorLayerData>, LayerDefinition> callback);

    public class LoadLayerEventArgs : EventArgs
    {
        internal LoadLayerEventArgs(LayerDefinition layer, DateRange dateRange)
        {
            Layer = layer;
            DateRange = dateRange;
        }

        public LayerDefinition Layer { get; set; }
        public DateRange DateRange { get; set; }
    }
}
