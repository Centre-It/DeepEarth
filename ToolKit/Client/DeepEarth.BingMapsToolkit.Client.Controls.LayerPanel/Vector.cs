// Deep Earth is a community project available under the Microsoft Public License (Ms-PL)
// Code is provided as is and with no warrenty – Use at your own risk
// View the project and the latest code at http://DeepEarth.codeplex.com/

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using DeepEarth.BingMapsToolkit.Client.Common.Entities;
using DeepEarth.BingMapsToolkit.Client.MapGeometry;
using DeepEarth.BingMapsToolkit.Common.Entities;
using Microsoft.Maps.MapControl.Core;

namespace DeepEarth.BingMapsToolkit.Client.Controls
{
    public static class Vector
    {
        public static void CreateOverlay(ObservableCollection<VectorLayerData> layerData, LayerDefinition layerDefinition,
                                         MapCore map, Dictionary<string, StyleSpecification> styles,
                                         bool enableBalloon, bool layersSelectable, DateRange dateRangeDisplay)
        {
            //make sure it hasn't already been added, remove = refresh
            RemoveOverlay(layerDefinition, map);

            var vectorLayer = new EnhancedMapLayer(map)
                                  {
                                      ID = layerDefinition.LayerID,
                                      ZIndex = layerDefinition.ZIndex,
                                      Styles = styles,
                                      LayerDefinition = layerDefinition,
                                      MinZoomLevel = layerDefinition.MinDisplayLevel,
                                      MaxZoomLevel = layerDefinition.MaxDisplayLevel,
                                      EnableBalloon = enableBalloon,
                                      EnableSelection = layersSelectable,
                                      DateRangeDisplay = dateRangeDisplay,
                                  };
            map.Children.Add(vectorLayer);

            if (!layersSelectable)
            {
                vectorLayer.Opacity = 0.5;
                vectorLayer.EnableBalloon = false;
                vectorLayer.EnableSelection = false;
            }

            vectorLayer.Add(layerData);
        }

        public static void HideOverlays(MapCore map)
        {
            //remove the layer by ID
            foreach (UIElement layer in map.Children)
            {
                if (layer is EnhancedMapLayer)
                {
                    var geomLayer = (EnhancedMapLayer) layer;
                    geomLayer.Opacity = 0.5;
                    geomLayer.EnableBalloon = false;
                    geomLayer.EnableSelection = false;
                }
            }
        }

        public static void ShowOverlays(MapCore map, bool enableBalloon, bool enableSelection)
        {
            //remove the layer by ID
            foreach (UIElement layer in map.Children)
            {
                if (layer is EnhancedMapLayer)
                {
                    var geomLayer = (EnhancedMapLayer) layer;
                    geomLayer.Opacity = 1;
                    geomLayer.EnableBalloon = enableBalloon;
                    geomLayer.EnableSelection = enableSelection;
                }
            }
        }

        public static void RemoveOverlay(LayerDefinition layerDefinition, MapCore map)
        {
            //remove the layer by ID
            var layer = GetLayerByID(layerDefinition.LayerID, map);
            if (layer != null)
            {
                layer.Dispose();
                map.Children.Remove(layer);
            }
        }

        public static EnhancedMapLayer GetLayerByID(string layerID, MapCore map)
        {
            foreach (UIElement layer in map.Children)
            {
                if (layer is EnhancedMapLayer)
                {
                    var geomLayer = (EnhancedMapLayer)layer;
                    if (geomLayer.ID == layerID)
                    {
                        return geomLayer;
                    }
                }
            }
            return null;
        }

        public static void SetLabelVisibility(LayerDefinition layerDefinition, MapCore map)
        {
            var geomLayer = GetLayerByID(layerDefinition.LayerID, map);
            if (geomLayer != null)
            {
                geomLayer.LabelVisibility = (layerDefinition.LabelOn) ? Visibility.Visible : Visibility.Collapsed;
            }
        }
    }
}