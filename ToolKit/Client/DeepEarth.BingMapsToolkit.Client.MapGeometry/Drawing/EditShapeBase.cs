// Deep Earth is a community project available under the Microsoft Public License (Ms-PL)
// Code is provided as is and with no warrenty – Use at your own risk
// View the project and the latest code at http://DeepEarth.codeplex.com/

using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using DeepEarth.BingMapsToolkit.Client.Common;
using DeepEarth.BingMapsToolkit.Client.Common.Commanding;
using DeepEarth.BingMapsToolkit.Client.Common.Entities;
using Microsoft.Maps.MapControl;
using Microsoft.Maps.MapControl.Core;

namespace DeepEarth.BingMapsToolkit.Client.MapGeometry.Drawing
{
    public class EditShapeBase : EditObjectBase
    {
        private readonly MapLayer layer;
        private readonly MapCore map;
        private readonly MapLayer layerPoints;
        private readonly MapPolygon polygon;
        private readonly MapPolyline predictedPolyline;
        private readonly MapPolyline strokePolyline;
        private StyleSpecification geometryStyle;

        private bool isdrawing;
        private bool isStarted;

        private Location lastMousePoint;
        private LocationCollection locations;

        public EditShapeBase(MapCore mapInstance)
        {
            //default is to draw a new polygon
            isdrawing = true;
            Commands.StopDrawingCommand.Executed += StopDrawingCommand_Executed;
            isStarted = false;

            map = mapInstance;

            layer = new MapLayer();
            map.Children.Add(layer);
            locations = new LocationCollection();

            polygon = new MapPolygon();
            polygon.MouseLeftButtonDown += polygon_MouseLeftButtonDown;
            layer.Children.Add(polygon);

            strokePolyline = new MapPolyline();
            strokePolyline.MouseLeftButtonDown += strokePolyline_MouseLeftButtonDown;
            layer.Children.Add(strokePolyline);

            //TODO: apply dashes?
            predictedPolyline = new MapPolyline();
            layer.Children.Add(predictedPolyline);

            layerPoints = new MapLayer();
            map.Children.Add(layerPoints);

            //bind to map events for editing
            //Create event handlers for standard MouseControl behavior.
            map.MouseDoubleClick += map_MouseDoubleClick;
            map.MouseLeftButtonDown += map_MouseLeftButtonDown;
            map.MouseMove += map_MouseMove;
        }

        void strokePolyline_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OnClick(new EventArgs());

        }

        void polygon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OnClick(new EventArgs());
        }

        public bool IsClosed { get; set; }

        public override StyleSpecification GeometryStyle
        {
            get { return geometryStyle; }
            set
            {
                //TODO: need to support the styling of polylines as polygons when isclosed.
                geometryStyle = value;
                strokePolyline.Stroke = new SolidColorBrush(Utilities.ColorFromHexString(geometryStyle.PolygonLineColour));
                strokePolyline.StrokeThickness = geometryStyle.PolygonLineWidth;

                predictedPolyline.Stroke = new SolidColorBrush(Utilities.ColorFromHexString(geometryStyle.PolygonLineColour));
                predictedPolyline.StrokeThickness = geometryStyle.PolygonLineWidth;

                polygon.Fill = new SolidColorBrush(Utilities.ColorFromHexString(geometryStyle.PolyFillColour));
            }
        }

        public override LocationCollection Locations
        {
            get { return locations; }
            set
            {
                //If polygon then we have a repeated point to close the shape that is redundant
                if (IsClosed && value.Count > 1)
                {
                    value.RemoveAt(value.Count - 1);
                }
                locations = value;
                //we are in edit mode
                exitDrawMode();

                //update points, polygon etc
                refreshAllLocations();
            }
        }

        private void StopDrawingCommand_Executed(object sender, ExecutedEventArgs e)
        {
            exitDrawMode();

            //update points, polygon etc
            refreshAllLocations();
        }

        private void exitDrawMode()
        {
            if (isdrawing)
            {
                isdrawing = false;
                Commands.StopDrawingCommand.Executed -= StopDrawingCommand_Executed;
                //drop predicted line
                layer.Children.Remove(predictedPolyline);
                //allow double click
                map.MouseDoubleClick -= map_MouseDoubleClick;
                //validate shape
                if (locations.Count < 2 || (IsClosed && locations.Count < 3))
                {
                    //invalid, clear object.
                    Dispose();
                }
            }
        }

        private void refreshAllLocations()
        {
            //TODO: test if some sort of differential is faster.
            if (Locations.Count > 0)
            {
                lastMousePoint = Locations[Locations.Count - 1];
            }

            polygon.Locations = new LocationCollection();
            strokePolyline.Locations = new LocationCollection();

            foreach (Location location in Locations)
            {
                if (IsClosed) polygon.Locations.Add(location);
                strokePolyline.Locations.Add(location);
            }
            //if we are drawing then the polygon need an extra point for the predicted area on mousemove
            if (IsClosed && isdrawing)
            {
                polygon.Locations.Add(lastMousePoint);
            }

            //clear the layers
            layerPoints.Children.Clear();

            var prevLocation = new Location();
            int index = 0;
            foreach (Location location in Locations)
            {
                if (index > 0)
                {
                    var epoint = new DrawPoint(map) {Index = index};
                    epoint.DrawPointMoved += point_DrawPointMoved;
                    epoint.DrawPointClick += epoint_DrawPointClick;
                    layerPoints.AddChild(epoint, location, PositionOrigin.Center);

                    var mpoint = new DrawPointMid(map) {Index = index};
                    mpoint.DrawPointMoved += mpoint_DrawPointMoved;
                    mpoint.DrawPointMoveStarted += mpoint_DrawPointMoveStarted;
                    mpoint.DrawPointMoveCompleted += mpoint_DrawPointMoveCompleted;
                    layerPoints.AddChild(mpoint, Utilities.GetMidLocation(prevLocation, location), PositionOrigin.Center);
                }
                else
                {
                    var spoint = new DrawPointStart(map) {Index = index};
                    spoint.DrawPointMoved += point_DrawPointMoved;
                    spoint.DrawPointClick += spoint_DrawPointClick;
                    layerPoints.AddChild(spoint, location, PositionOrigin.Center);
                }
                index++;
                prevLocation = location;
            }

            //if not drawing we join the last bit together
            if (IsClosed && index > 0 && !isdrawing)
            {
                var mpoint = new DrawPointMid(map) {Index = index};
                mpoint.DrawPointMoved += mpoint_DrawPointMoved;
                mpoint.DrawPointMoveStarted += mpoint_DrawPointMoveStarted;
                mpoint.DrawPointMoveCompleted += mpoint_DrawPointMoveCompleted;
                layerPoints.AddChild(mpoint, Utilities.GetMidLocation(prevLocation, Locations[0]),
                                   PositionOrigin.Center);

                //close the linestring
                strokePolyline.Locations.Add(Locations[0]);
            }

            OnDrawingChanged(new DrawingEventArgs {Locations = Locations});
        }

        private void map_MouseMove(object sender, MouseEventArgs e)
        {
            if (isdrawing)
            {
                Point pos = e.GetPosition(map);
                if (isStarted)
                {
                    Location currentMousePoint = map.ViewportPointToLocation(pos);
                    //draw a line between the mouse cursor and the previous point
                    predictedPolyline.Locations = new LocationCollection {lastMousePoint, currentMousePoint};
                    if (IsClosed) polygon.Locations[polygon.Locations.Count - 1] = currentMousePoint;
                }
                //if we are need the edge of the map then we pan
                Utilities.AutoPan(pos, map);
            }
        }

        private void map_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (isdrawing)
            {
                lastMousePoint = map.ViewportPointToLocation(e.GetPosition(map));
                locations.Add(lastMousePoint);
                refreshAllLocations();
                isStarted = true;
                OnClick(new EventArgs());
            }
        }

        private void point_DrawPointMoved(object sender, DrawPointMovedEventArgs args)
        {
            if (locations.Count > args.Index)
            {
                //Find and update the location
                locations[args.Index] = args.Location;
            }
            //Need to update the polygon, polyline and mid points.
            refreshAllLocations();
            //may need to autopan map;
            Utilities.AutoPan(args.Point, map);
        }

        private void epoint_DrawPointClick(object sender, DrawPointMovedEventArgs args)
        {
            OnClick(new EventArgs());
            //validate min amount of points for shape.
            if (locations.Count > 3 || (!IsClosed && locations.Count > 2))
            {
                //delete the location
                locations.RemoveAt(args.Index);
                //Refresh
                refreshAllLocations();
            }
        }

        private void mpoint_DrawPointMoved(object sender, DrawPointMovedEventArgs args)
        {
            //move the point
            if (IsClosed) polygon.Locations[args.Index] = args.Location;
            strokePolyline.Locations[args.Index] = args.Location;
        }

        private void mpoint_DrawPointMoveCompleted(object sender, DrawPointMovedEventArgs args)
        {
            //add the point and refresh
            locations.Insert(args.Index, args.Location);
            refreshAllLocations();
        }

        private void mpoint_DrawPointMoveStarted(object sender, DrawPointMovedEventArgs args)
        {
            //add a new temp point to polys.
            if (IsClosed) polygon.Locations.Insert(args.Index, args.Location);
            strokePolyline.Locations.Insert(args.Index, args.Location);
        }

        private static void map_MouseDoubleClick(object sender, MapMouseEventArgs e)
        {
            //prevent zoom
            e.Handled = true;
        }

        private void spoint_DrawPointClick(object sender, DrawPointMovedEventArgs args)
        {
            OnClick(new EventArgs());
            if (locations.Count >= 3 || (!IsClosed && locations.Count >= 2))
            {
                //if we are drawing, exit now in edit mode.
                exitDrawMode();

                //update points, polygon etc
                refreshAllLocations();

                if (IsClosed)
                {
                    OnDrawingCompleted(new DrawingEventArgs
                                           {
                                               Locations = Locations
                                           });
                }
                else
                {
                    OnDrawingCompleted(new DrawingEventArgs
                                           {
                                               Locations = Locations
                                           });
                }
            }
        }

        public override void Dispose()
        {
            exitDrawMode();
            map.MouseLeftButtonDown -= map_MouseLeftButtonDown;
            map.MouseMove -= map_MouseMove;

            layer.Children.Clear();
            map.Children.Remove(layer);

            layerPoints.Children.Clear();
            map.Children.Remove(layerPoints);
        }
    }
}