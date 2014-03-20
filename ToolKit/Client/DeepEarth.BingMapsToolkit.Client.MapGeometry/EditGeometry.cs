// Deep Earth is a community project available under the Microsoft Public License (Ms-PL)
// Code is provided as is and with no warrenty – Use at your own risk
// View the project and the latest code at http://DeepEarth.codeplex.com/

using System;
using System.Collections.Generic;
using System.Windows.Input;
using DeepEarth.BingMapsToolkit.Client.Common;
using DeepEarth.BingMapsToolkit.Client.Common.Converters;
using DeepEarth.BingMapsToolkit.Client.Common.Entities;
using DeepEarth.BingMapsToolkit.Client.MapGeometry.Drawing;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using Microsoft.Maps.MapControl;
using Microsoft.Maps.MapControl.Core;
using Location=Microsoft.Maps.MapControl.Location;
using Point=GisSharpBlog.NetTopologySuite.Geometries.Point;

namespace DeepEarth.BingMapsToolkit.Client.MapGeometry
{
    public delegate void EditCompletedEventHandler(object sender, EditGeometryChangedEventArgs args);

    public class EditGeometry : IDisposable
    {
        private double currentZoomLevel;
        private IGeometry geometry;
        private bool isDirty;


        public EditGeometry()
        {
            isDirty = true;
            drawObjects = new List<EditObjectBase>();
            StyleSpecification = new StyleSpecification();
        }

        internal List<EditObjectBase> drawObjects { get; set; }

        private MapCore mapInstance;
        public MapCore MapInstance
        {
            get { return mapInstance; }
            set 
            {
                if (mapInstance != null)
                {
                    mapInstance.ViewChangeOnFrame -= MapInstance_ViewChangeOnFrame;
                }
                mapInstance = value;
                mapInstance.ViewChangeOnFrame += MapInstance_ViewChangeOnFrame;
                applyScaling();
            }
        }

        public string ItemID { get; set; }

        private StyleSpecification styleSpecification;
        public StyleSpecification StyleSpecification
        {
            get
            {
                return styleSpecification;
            }
            set
            {
                if (styleSpecification != null)
                {
                    styleSpecification.StyleChanged -= GeometryStyle_StyleChanged;
                }
                styleSpecification = value;
                if (styleSpecification != null)
                {
                    styleSpecification.StyleChanged += GeometryStyle_StyleChanged;
                    foreach (var o in drawObjects)
                    {
                        o.GeometryStyle = styleSpecification;
                    }
                }
            }
        }

        public IGeometry Geometry
        {
            get
            {
                if (isDirty)
                {
                    var points = new List<IPoint>();
                    var lines = new List<ILineString>();
                    var polys = new List<IPolygon>();
                    //parse internal shapes into new IGeometry
                    GeometryType typemask = GeometryType.None;
                    foreach (EditObjectBase o in drawObjects)
                    {
                        //only valid objects (cancelled objects can have 0 locations)
                        if (o.Locations.Count > 0)
                        {
                            //get each IGeometry based on type
                            if (o is EditPoint)
                            {
                                typemask = Utilities.GetTypemask(typemask, GeometryType.Point, GeometryType.MultiPoint);
                                points.Add(new Point(CoordinateConvertor.Convert(o.Locations[0])));
                            }

                            if (o is EditLineString) 
                            {
                                
                                typemask = Utilities.GetTypemask(typemask, GeometryType.LineString,
                                                                 GeometryType.MultiLineString);
                                if ((o.Locations.Count > 2) || (o.Locations.Count == 0))
                                {
                                    lines.Add(
                                        new LineString(CoordinateConvertor.LocationCollectionToCoordinates(o.Locations)));
                                }
                                else
                                {
                                    lines.Add(new LinearRing(((new List<ICoordinate>()).ToArray())));
                                }
                            }

                            if (o is EditPolygon) 
                            {
                                
                                typemask = Utilities.GetTypemask(typemask, GeometryType.Polygon,
                                                                 GeometryType.MultiPolygon);
                                //polygon needs its start added again as an end point
                                if ((o.Locations.Count > 3) || (o.Locations.Count == 0))
                                {
                                    var locs = new LocationCollection();
                                    foreach (Location location in o.Locations)
                                    {
                                        locs.Add(location);
                                    }
                                    locs.Add(o.Locations[0]);
                                    polys.Add(
                                        new Polygon(new LinearRing(CoordinateConvertor.LocationCollectionToCoordinates(locs))));
                                }
                                else
                                {
                                    polys.Add(
                                        new Polygon(new LinearRing(((new List<ICoordinate>()).ToArray()))));
                                }
                            }
                        }
                    }
                    //determine what overall type it is
                    if ((typemask & GeometryType.GeometryCollection) > 0)
                    {
                        var geoms = new List<IGeometry>();
                        foreach (IPoint point in points)
                        {
                            geoms.Add(point);
                        }
                        foreach (ILineString line in lines)
                        {
                            geoms.Add(line);
                        }
                        foreach (IPolygon poly in polys)
                        {
                            geoms.Add(poly);
                        }
                        geometry = new GeometryCollection(geoms.ToArray());
                        return geometry;
                    }
                    if ((typemask & GeometryType.MultiPolygon) > 0)
                    {
                        geometry = new MultiPolygon(polys.ToArray());
                        return geometry;
                    }
                    if ((typemask & GeometryType.MultiLineString) > 0)
                    {
                        geometry = new MultiLineString(lines.ToArray());
                        return geometry;
                    }
                    if ((typemask & GeometryType.MultiPoint) > 0)
                    {
                        geometry = new MultiPoint(points.ToArray());
                        return geometry;
                    }
                    if ((typemask & GeometryType.Polygon) > 0)
                    {
                        geometry = polys[0];
                        return geometry;
                    }
                    if ((typemask & GeometryType.LineString) > 0)
                    {
                        geometry = lines[0];
                        return geometry;
                    }
                    if ((typemask & GeometryType.Point) > 0)
                    {
                        geometry = points[0];
                        return geometry;
                    }
                    //empty
                    geometry = null;
                    return geometry;
                }
                return geometry;
            }
            set
            {
                if (geometry != null)
                {
                    //dispose
                    removeGeometry();
                }
                geometry = value;
                if (geometry != null)
                {
                    //create new
                    isDirty = false;
                    drawObjects = new List<EditObjectBase>();
                    createGeometry(geometry);
                    applyScaling();
                }
            }
        }

        void GeometryStyle_StyleChanged(object sender, StyleChangedEventArgs e)
        {
            foreach (var o in drawObjects)
            {
                o.GeometryStyle = e.newStyle;
            }
        }

        public void AddNew(GeometryType type)
        {
            switch (type)
            {
                case GeometryType.Point:
                    var editPoint = new EditPoint(MapInstance)
                    {
                        GeometryStyle = StyleSpecification
                    };
                    editPoint.DrawingCompleted += drawingCompleted;
                    editPoint.DrawingChanged += drawingChanged;
                    drawObjects.Add(editPoint);
                    break;

                case GeometryType.LineString:
                    var editLine= new EditLineString(MapInstance)
                    {
                        GeometryStyle = StyleSpecification
                    };
                    editLine.DrawingCompleted += drawingCompleted;
                    editLine.DrawingChanged += drawingChanged;
                    editLine.Click += OnClicked;
                    drawObjects.Add(editLine);
                    break;

                case GeometryType.Polygon:
                    var editPoly = new EditPolygon(MapInstance)
                    {
                        GeometryStyle = StyleSpecification
                    };
                    editPoly.DrawingCompleted += drawingCompleted;
                    editPoly.DrawingChanged += drawingChanged;
                    editPoly.Click += OnClicked;
                    drawObjects.Add(editPoly);
                    break;
                case GeometryType.None:
                    //free draw
                    var freeDraw = new FreeDraw(MapInstance)
                    {
                        GeometryStyle = StyleSpecification
                    };
                    freeDraw.DrawingCompleted += freeDrawingCompleted;
                    drawObjects.Add(freeDraw);
                    break;
            }
            applyScaling();
        }

        private Cursor previousCursor;
        private bool eraseMode;
        public bool EraseMode
        {
            get { return eraseMode; }
            set
            {
                eraseMode = value;
                if (eraseMode)
                {
                    previousCursor = MapInstance.Cursor;
                    MapInstance.Cursor = Cursors.Eraser;
                    //listen to click event on drawobjects, erase them.
                    foreach (var o in drawObjects)
                    {
                        o.Click += eraseObject_Click;
                    }
                }else
                {
                    MapInstance.Cursor = previousCursor;
                    foreach (var o in drawObjects)
                    {
                        o.Click -= eraseObject_Click;
                    }
                }
            }
        }

        void eraseObject_Click(object sender, EventArgs e)
        {
            //remove the object.
            var o = (EditObjectBase) sender;
            drawObjects.Remove(o);
            o.Dispose();
            OnEditCompleted();
        }



        public ICoordinate[] LastPoints
        {
            get
            {
                GeometryType typemask = GeometryType.None;
                var points = new List<ICoordinate>();
                var lines = new List<ICoordinate[]>();
                var polys = new List<ICoordinate[]>();

                if (isDirty)
                {
                    //parse internal shapes into new IGeometry

                    foreach (EditObjectBase o in drawObjects)
                    {

                        if (o is EditLineString)
                        {
                            typemask = Utilities.GetTypemask(typemask, GeometryType.LineString,
                                                                 GeometryType.MultiLineString);
                            lines.Add(CoordinateConvertor.LocationCollectionToCoordinates(o.Locations));
                        }

                        if (o is EditPolygon)
                        {
                            typemask = Utilities.GetTypemask(typemask, GeometryType.Polygon,
                                                                GeometryType.MultiPolygon);

                            var locs = new LocationCollection();
                            foreach (Location location in o.Locations)
                            {
                                locs.Add(location);
                            }

                            polys.Add(CoordinateConvertor.LocationCollectionToCoordinates(locs));
                        }
                    }
                }

                if ((typemask & GeometryType.Polygon) > 0)
                {
                    return polys[0];
                }
                if ((typemask & GeometryType.LineString) > 0)
                {
                    return lines[0];
                }
                else
                    return null;
            }
        }


        #region IDisposable Members

        public void Dispose()
        {
            removeGeometry();
            MapInstance.ViewChangeOnFrame -= MapInstance_ViewChangeOnFrame;
        }

        #endregion

        public event EditCompletedEventHandler EditCompleted;
        public event EditCompletedEventHandler EditChange;

        protected void OnEditCompleted()
        {
            isDirty = true;
            if (EditCompleted != null)
                EditCompleted(this, new EditGeometryChangedEventArgs {Geometry = Geometry});
        }

        protected void OnEditChange()
        {
            isDirty = true;
            if (EditChange != null)
                EditChange(this, new EditGeometryChangedEventArgs {Geometry = Geometry});
        }

        private void MapInstance_ViewChangeOnFrame(object sender, MapEventArgs e)
        {
            if (currentZoomLevel != MapInstance.ZoomLevel)
            {
                applyScaling();
            }
        }

        private void applyScaling()
        {
            currentZoomLevel = MapInstance.ZoomLevel;

            double scale = Math.Pow(0.05*(currentZoomLevel + 1), 2) + 0.01;
            if (scale > 1) scale = 1;
            if (scale < 0.125) scale = 0.125;

            foreach (EditObjectBase drawObject in drawObjects)
            {
                if (drawObject is EditPoint)
                {
                    ((EditPoint) drawObject).Scale = scale;
                }
            }
        }

        private void removeGeometry()
        {
            foreach (EditObjectBase drawObject in drawObjects)
            {
                drawObject.Dispose();
            }
            drawObjects.Clear();
        }

        private void createGeometry(IGeometry geom)
        {
            if (geom is IPoint)
            {
                var point = (IPoint) geom;
                createPoint(point);
            }
            if (geom is ILineString)
            {
                var lineString = (ILineString) geom;
                createPolyLine(lineString);
            }
            if (geom is IPolygon)
            {
                var polygon = (IPolygon) geom;
                createPolygon(polygon);
            }
            if (geom is IGeometryCollection)
            {
                var geometryCollection = (IGeometryCollection) geom;
                foreach (IGeometry subgeometry in geometryCollection.Geometries)
                {
                    createGeometry(subgeometry);
                }
            }
        }

        private void createPoint(IPoint point)
        {
            var editPoint = new EditPoint(MapInstance)
                                {
                                    Locations = CoordinateConvertor.CoordinatesToLocationCollection(point.Coordinates),
                                    GeometryStyle = StyleSpecification,
                                };
            editPoint.DrawingCompleted += drawingCompleted;
            editPoint.DrawingChanged += drawingChanged;
            drawObjects.Add(editPoint);
        }

        private void createPolyLine(IGeometry lineString)
        {
            var line = new EditLineString(MapInstance)
                           {
                               Locations = CoordinateConvertor.CoordinatesToLocationCollection(lineString.Coordinates),
                               GeometryStyle = StyleSpecification,
                           };
            line.DrawingCompleted += drawingCompleted;
            line.DrawingChanged += drawingChanged;
            drawObjects.Add(line);
        }

        private void createPolygon(IPolygon polygon)
        {
            var poly = new EditPolygon(MapInstance)
                           {
                               Locations =
                                   CoordinateConvertor.CoordinatesToLocationCollection(polygon.ExteriorRing.Coordinates),
                               GeometryStyle = StyleSpecification,
                           };
            poly.DrawingCompleted += drawingCompleted;
            poly.DrawingChanged += drawingChanged;
            drawObjects.Add(poly);
        }

        private void drawingChanged(object sender, DrawingEventArgs args)
        {
            OnEditChange();
        }

        protected virtual void drawingCompleted(object sender, DrawingEventArgs args)
        {
            OnEditCompleted();
        }

        private void freeDrawingCompleted(object sender, DrawingEventArgs args)
        {
            drawObjects.Remove((EditObjectBase) sender);
            createPolyLine(new LineString(CoordinateConvertor.LocationCollectionToCoordinates(args.Locations)));
            OnEditCompleted();
        }

        protected virtual void OnClicked(object sender, EventArgs args) { }
    }

    public class EditGeometryChangedEventArgs : EventArgs
    {
        public IGeometry Geometry { get; set; }
    }
}