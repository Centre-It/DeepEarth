// Deep Earth is a community project available under the Microsoft Public License (Ms-PL)
// Code is provided as is and with no warrenty – Use at your own risk
// View the project and the latest code at http://DeepEarth.codeplex.com/

using System.Windows;
using System.Windows.Input;
using DeepEarth.BingMapsToolkit.Client.Common;
using DeepEarth.BingMapsToolkit.Client.Common.Commanding;
using DeepEarth.BingMapsToolkit.Client.Common.Entities;
using Microsoft.Maps.MapControl;
using Microsoft.Maps.MapControl.Core;

namespace DeepEarth.BingMapsToolkit.Client.MapGeometry.Drawing
{
    public class FreeDraw : EditObjectBase
    {
        private readonly MapLayer layer;
        private readonly MapCore map;
        private readonly EnhancedMapPolyline strokePolyline;
        private StyleSpecification geometryStyle;

        private bool isDrawing;

        public FreeDraw(MapCore mapInstance)
        {
            //default is to draw a new polygon
            Commands.StopDrawingCommand.Executed += StopDrawingCommand_Executed;

            map = mapInstance;

            layer = new MapLayer();
            map.Children.Add(layer);

            strokePolyline = new EnhancedMapPolyline();
            strokePolyline.Locations = new LocationCollection();
            layer.Children.Add(strokePolyline);

            //bind to map events for editing
            map.MouseDoubleClick += map_MouseDoubleClick;
            map.MouseLeftButtonDown += map_MouseLeftButtonDown;
            map.MouseMove += map_MouseMove;
            map.MouseLeftButtonUp += map_MouseLeftButtonUp;
            map.MousePan += map_MousePan;
        }

        void map_MousePan(object sender, MapMouseDragEventArgs e)
        {
            e.Handled = true;
        }

        public override StyleSpecification GeometryStyle
        {
            get { return geometryStyle; }
            set
            {
                geometryStyle = value;
                strokePolyline.GeometryStyle = geometryStyle;
            }
        }

        public override LocationCollection Locations
        {
            get { return strokePolyline.Locations; }
            set { strokePolyline.Locations = value;}
        }

        private void map_MouseMove(object sender, MouseEventArgs e)
        {
            Point pos = e.GetPosition(map);
            if (isDrawing)
            {
                strokePolyline.Locations.Add(map.ViewportPointToLocation(pos));
            }
            else
            {
                //if we are need the edge of the map then we pan
                Utilities.AutoPan(pos, map);
            }
        }

        private void map_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            strokePolyline.Locations.Add(map.ViewportPointToLocation(e.GetPosition(map)));
            isDrawing = true;
        }

        void map_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            completeDrawing();
        }

        private void StopDrawingCommand_Executed(object sender, ExecutedEventArgs e)
        {
            completeDrawing();
        }

        private void completeDrawing()
        {
            //end drawing
            OnDrawingCompleted(new DrawingEventArgs
            {
                Locations = Locations
            });
            Dispose();
        }

        private static void map_MouseDoubleClick(object sender, MapMouseEventArgs e)
        {
            //prevent zoom
            e.Handled = true;
        }

        public override void Dispose()
        {
            map.MouseDoubleClick -= map_MouseDoubleClick;
            map.MouseLeftButtonDown -= map_MouseLeftButtonDown;
            map.MouseMove -= map_MouseMove;
            map.MouseLeftButtonUp -= map_MouseLeftButtonUp;
            map.MousePan -= map_MousePan;

            layer.Children.Clear();
            map.Children.Remove(layer);
        }
    }
}