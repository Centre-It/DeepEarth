// Deep Earth is a community project available under the Microsoft Public License (Ms-PL)
// Code is provided as is and with no warrenty – Use at your own risk
// View the project and the latest code at http://DeepEarth.codeplex.com/

using System;
using System.Windows.Input;
using DeepEarth.BingMapsToolkit.Client.Common;
using DeepEarth.BingMapsToolkit.Client.Common.Commanding;
using DeepEarth.BingMapsToolkit.Client.Common.Entities;
using Microsoft.Maps.MapControl;
using Microsoft.Maps.MapControl.Core;

namespace DeepEarth.BingMapsToolkit.Client.MapGeometry.Drawing
{
    public class EditPoint : EditObjectBase
    {
        private readonly MapLayer layer;
        private readonly MapCore map;
        private bool isdrawing;

        private LocationCollection locations;
        private DrawPointCustom point;

        public EditPoint(MapCore mapInstance)
        {
            //default is to draw a new polygon
            isdrawing = true;
            Commands.StopDrawingCommand.Executed += StopDrawingCommand_Executed;
            map = mapInstance;

            layer = new MapLayer();
            map.Children.Add(layer);
            locations = new LocationCollection();

            //bind to map events for editing
            //Create event handlers for standard MouseControl behavior.
            map.MouseLeftButtonDown += map_MouseLeftButtonDown;
        }

        private StyleSpecification geometryStyle;
        public override StyleSpecification GeometryStyle
        {
            get { return geometryStyle; }
            set
            {
                geometryStyle = value;
                if (point != null)
                {
                    point.GeometryStyle = geometryStyle;
                }
            }
        }

        private double scale;
        public double Scale
        {
            get { return scale; }
            set
            {
                scale = value;
                if (point != null)
                {
                    point.Scale = scale;
                }
            }
        }

        public override LocationCollection Locations
        {
            get { return locations; }
            set
            {
                locations = value;
                //we are in edit mode
                exitDrawMode();

                //create / update point
                if (point == null)
                {
                    point = new DrawPointCustom(map) {GeometryStyle = GeometryStyle, Scale = Scale};
                    point.DrawPointMoved += point_DrawPointMoved;
                    point.DrawPointClick += point_DrawPointClick;
                    layer.AddChild(point, locations[0], PositionOrigin.Center);
                }
                else
                {
                    point.SetValue(MapLayer.PositionProperty, locations[0]);
                }
            }
        }

        private void StopDrawingCommand_Executed(object sender, ExecutedEventArgs e)
        {
            exitDrawMode();
        }

        private void exitDrawMode()
        {
            if (isdrawing)
            {
                isdrawing = false;
                map.MouseLeftButtonDown -= map_MouseLeftButtonDown;
            }
        }

        private void point_DrawPointClick(object sender, DrawPointMovedEventArgs args)
        {
            OnClick(new EventArgs());
            //if drawing need to stop, now in edit mode.
            exitDrawMode();
            OnDrawingCompleted(new DrawingEventArgs
                                   {
                                       Locations = Locations
                                   });
        }

        private void point_DrawPointMoved(object sender, DrawPointMovedEventArgs args)
        {
            Locations[0] = args.Location;
            OnDrawingChanged(new DrawingEventArgs{Locations = Locations});
            //may need to autopan map;
            Utilities.AutoPan(args.Point, map);
        }

        private void map_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (isdrawing)
            {
                Locations = new LocationCollection {map.ViewportPointToLocation(e.GetPosition(map))};
                OnDrawingCompleted(new DrawingEventArgs { Locations = Locations });
            }
        }

        public override void Dispose()
        {
            exitDrawMode();
            layer.Children.Clear();
            map.Children.Remove(layer);
        }
    }
}