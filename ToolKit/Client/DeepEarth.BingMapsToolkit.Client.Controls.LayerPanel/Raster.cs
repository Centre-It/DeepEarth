// Deep Earth is a community project available under the Microsoft Public License (Ms-PL)
// Code is provided as is and with no warrenty – Use at your own risk
// View the project and the latest code at http://DeepEarth.codeplex.com/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DeepEarth.BingMapsToolkit.Client.Common.Converters;
using DeepEarth.BingMapsToolkit.Client.Common.Entities;
using DeepEarth.BingMapsToolkit.Client.MapGeometry;
using DeepEarth.BingMapsToolkit.Common.Entities;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.IO;
using Microsoft.Maps.MapControl;
using Microsoft.Maps.MapControl.Core;

namespace DeepEarth.BingMapsToolkit.Client.Controls
{
    public static class Raster
    {
        private const string cDefaultStyleID = "defaultstyle";

        public static void RemoveOverlay(LayerDefinition layerDefinition, MapCore map)
        {
            //remove the paired vector layer
            Vector.RemoveOverlay(layerDefinition, map);

            //remove the overlay by ID
            foreach (UIElement overlay in map.Children)
            {
                if (overlay is IdentifiableTileLayer)
                {
                    if (((IdentifiableTileLayer) overlay).ID == layerDefinition.LayerID)
                    {
                        map.Children.Remove(overlay);
                        ((IdentifiableTileLayer) overlay).Dispose();
                        break;
                    }
                }
            }
        }

        public static void CreateOverlay(LayerDefinition layerDefinition, MapCore map, string tileServerPath,
                                         bool enableTileRefreshPolling)
        {
            //make sure it hasn't already been added, remove = refresh
            RemoveOverlay(layerDefinition, map);

            //add a layer and polygon to show where high res images are:
            var overlaymarkerLayer = new EnhancedMapLayer(map)
                                         {
                                             ID = layerDefinition.LayerID,
                                             IsHitTestVisible = false
                                         };
            //add overlay
            // Creates a new map layer to add the tile overlay to.
            var tileLayer = new IdentifiableTileLayer
                                {
                                    LayerDefinition = layerDefinition,
                                    ZIndex = layerDefinition.ZIndex,
                                    IsHitTestVisible = false,
                                    TileServerPath = tileServerPath,
                                    EnableTileRefreshPolling = enableTileRefreshPolling,
                                    MapInstance = map,
                                };

            tileLayer.TilePolling += (o, e) => Commands.PollTileLayerCommand.Execute(e, o);

            map.Children.Add(tileLayer);
            map.Children.Add(overlaymarkerLayer);

            tileLayer.Opacity = 1.0;

            var polygon = new MapPolygon
                              {
                                  Locations = CoordinateConvertor.LocationRectToLocationCollection(tileLayer.MBR),
                                  Stroke = new SolidColorBrush(Colors.Red)
                              };
            overlaymarkerLayer.Children.Add(polygon);
        }

        public static void RemoveImage(LayerDefinition layerDefinition, MapCore map)
        {
            //remove the overlay by ID
            Vector.RemoveOverlay(layerDefinition, map);
        }

        public static void CreateImage(LayerDefinition layerDefinition, MapCore map,
                                       Dictionary<string, StyleSpecification> styles, string tileServerPath)
        {
            //make sure it hasn't already been added, remove = refresh
            RemoveImage(layerDefinition, map);

            try
            {
                StyleSpecification cascadedstyle = styles[cDefaultStyleID];

                if (styles.ContainsKey(layerDefinition.LayerStyleName))
                {
                    cascadedstyle = styles[layerDefinition.LayerStyleName];
                }

                var wkbReader = new WKBReader(new GeometryFactory(new PrecisionModel(), 4326));
                IGeometry geom = wkbReader.Read(layerDefinition.MBR);
                var ev = (Envelope) geom.EnvelopeInternal;

                // The bounding rectangle that the tile overlay can be placed within.
                var boundingRect = new LocationRect(CoordinateConvertor.ConvertBack(ev.Centre), ev.Width, ev.Height);

                var layer = new EnhancedMapLayer(map)
                                {
                                    ID = layerDefinition.LayerID,
                                    ZIndex = layerDefinition.ZIndex,
                                    IsHitTestVisible = false,
                                };
                map.Children.Add(layer);
                var image = new ImageSprite
                                {
                                    ImageSource =
                                        new BitmapImage(
                                        new Uri(
                                            tileServerPath + "&layerid=" + layerDefinition.LayerID + "&timestamp=" +
                                            DateTime.Now.ToShortTimeString(),
                                            UriKind.Absolute)),
                                    Frames = cascadedstyle.Frames,
                                    MilliSecondsPerFrame = cascadedstyle.FrameInterval,
                                    SpriteWidth = cascadedstyle.Width,
                                    SpriteHeight = cascadedstyle.Height,
                                };
                layer.AddChild(image, boundingRect);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }
    }
}