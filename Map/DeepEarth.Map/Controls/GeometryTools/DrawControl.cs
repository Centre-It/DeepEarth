using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Browser;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DeepEarth.Client.MapControl.Events;
using DeepEarth.Client.MapControl.Geometry;
using DeepEarth.Client.MapControl.Layers;

#pragma warning disable 1591

namespace DeepEarth.Client.MapControl.Controls.GeometryTools
{
    public class DrawControl : MapControl
    {
        [DefaultValue(GeometryMode.FreeDraw)]
        public enum GeometryMode
        {
            FreeDraw,
            StringLine,
            Polygon,
            PointPin,
            Eraser
        }

        private readonly ObservableCollection<GeometryLayer> _DrawLayers;

        public GeometryMode DrawMode { get; set; }

        public Color DrawLineColor { get; set; }
        public Color DrawFillColor { get; set; }
        public double DrawLineThickness { get; set; }
        public double DrawOpacity { get; set; }

        private GeometryLayer _DrawLayer;
        private Grid _DrawLayerGrid;

        //private Point _OrigMousePoint;
        private Point _CurrentMousePoint;

        private DrawLineString _FreeDrawLine;
        private DrawLineString _StringLine;
        private DrawPolygon _Polygon;
        private DrawPinPoint _PinPoint;


        private DrawPolygon _LeaderPolygon;
        private DrawLineString _LeaderLine;
        private DrawPoint _LeaderPoint;

        //private DrawPoint _StartPoint;
        //private DrawPoint _EndPoint;

        private bool _IsDrawing;
        private bool _IsDoubleClick;

        public DrawControl() : this(Map.DefaultInstance) { }

        public DrawControl(Map map)
        {
            _Map = map;

            DefaultStyleKey = typeof(DrawControl);

            _DrawLayers = new ObservableCollection<GeometryLayer>();
            _DrawLayers.CollectionChanged += DrawLayers_CollectionChanged;
        }


        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // Test IsDesignTime, to help display control in blend correctly
            if(HtmlPage.IsEnabled)
            {
                if(_DrawLayer == null)
                {
                    _DrawLayerGrid = (Grid)GetTemplateChild("PART_DrawControlLayers");
                    _DrawLayer = new GeometryLayer(_Map) { UpdateMode = GeometryLayer.UpdateModes.TransformUpdate };
                    _DrawLayers.Add(_DrawLayer);
                }

                //Create event handlers for standard MouseControl behavior.

                //  MapInstance.Events.MapMouseEnter += Events_MapMouseEnter;
                MapInstance.Events.MapMouseLeave += Events_MapMouseLeave;
                MapInstance.Events.MapDoubleClick += Events_MapDoubleClick;
                MapInstance.Events.MapMouseDown += Events_MapMouseDown;
                MapInstance.Events.MapMouseMove += Events_MapMouseMove;
                MapInstance.Events.MapMouseDrag += Events_MapMouseDrag;
                MapInstance.Events.MapMouseUp += Events_MapMouseUp;
                MapInstance.Events.MapDragBehavourChanged += Events_MapDragBehavourChanged;
            }
        }

        private void Events_MapMouseLeave(Map map, MouseEventArgs args)
        {
            ClearDrawLeader();
        }

        private void Events_MapMouseDown(Map map, MouseButtonEventArgs args)
        {
            if(map.DragMode == Map.DragBehavior.Draw)
            {
                _CurrentMousePoint = map.CoordHelper.PixelToGeo(args.GetPosition(map));

                switch(DrawMode)
                {
                    case GeometryMode.FreeDraw:
                        _FreeDrawLine = new DrawLineString { LineColor = DrawLineColor, LineThickness = DrawLineThickness, Opacity = DrawOpacity };
                        _FreeDrawLine.Points.Add(_CurrentMousePoint);
                        _DrawLayer.Add(_FreeDrawLine);
                        break;

                    case GeometryMode.StringLine:
                        if(!_IsDoubleClick)
                        {
                            if(!_IsDrawing)
                            {
                                _StringLine = new DrawLineString { LineColor = DrawLineColor, LineThickness = DrawLineThickness, Opacity = DrawOpacity };
                                _StringLine.Points.Add(_CurrentMousePoint);
                                _DrawLayer.Add(_StringLine);
                                _DrawLayer.Add(new DrawPoint { Point = _CurrentMousePoint });
                                _IsDrawing = true;
                            }
                            else
                            {
                                _StringLine.Points.Add(_CurrentMousePoint);
                                _DrawLayer.Add(new DrawPoint { Point = _CurrentMousePoint });
                            }
                        }
                        else
                        {
                            _IsDoubleClick = false;
                        }
                        break;

                    case GeometryMode.Polygon:
                        if(!_IsDoubleClick)
                        {
                            if(!_IsDrawing)
                            {
                                _Polygon = new DrawPolygon { LineColor = DrawLineColor, LineThickness = DrawLineThickness, Opacity = DrawOpacity, Fill = new SolidColorBrush(DrawFillColor) };
                                _Polygon.Points.Add(_CurrentMousePoint);
                                _DrawLayer.Add(_Polygon);
                                _DrawLayer.Add(new DrawPoint { Point = _CurrentMousePoint });
                                _IsDrawing = true;
                            }
                            else
                            {
                                _Polygon.Points.Add(_CurrentMousePoint);
                                _DrawLayer.Add(new DrawPoint { Point = _CurrentMousePoint });
                            }
                        }
                        else
                        {
                            _IsDoubleClick = false;
                        }
                        break;

                    case GeometryMode.PointPin:
                        _PinPoint = new DrawPinPoint(); // { Image = "PinPoints/public.png" };
                      

                        _DrawLayer.Add(new DrawPinPoint());
                        break;

                    case GeometryMode.Eraser:
                        break;
                }
            }

        }

        private void Events_MapMouseDrag(Map map, MouseEventArgs args)
        {
            if(map.DragMode == Map.DragBehavior.Draw)
            {
                _CurrentMousePoint = map.CoordHelper.PixelToGeo(args.GetPosition(map));

                switch(DrawMode)
                {
                    case GeometryMode.FreeDraw:
                        _FreeDrawLine.Points.Add(_CurrentMousePoint);
                        break;

                    case GeometryMode.StringLine:
                        break;

                    case GeometryMode.Polygon:
                        break;

                    case GeometryMode.PointPin:
                        break;

                    case GeometryMode.Eraser:
                        break;
                }
            }

        }

        private void Events_MapMouseUp(Map map, MouseButtonEventArgs args)
        {
            if(map.DragMode == Map.DragBehavior.Draw)
            {
                switch(DrawMode)
                {
                    case GeometryMode.FreeDraw:
                        _FreeDrawLine = null;
                        break;

                    case GeometryMode.StringLine:
                        break;

                    case GeometryMode.Polygon:
                        break;

                    case GeometryMode.PointPin:
                        break;

                    case GeometryMode.Eraser:
                        break;
                }
            }

        }

        private void Events_MapMouseMove(Map map, MouseEventArgs args)
        {
            var _MousePoint = map.CoordHelper.PixelToGeo(args.GetPosition(map));

            if(map.DragMode == Map.DragBehavior.Draw)
            {
                switch(DrawMode)
                {
                    case GeometryMode.FreeDraw:
                        break;

                    case GeometryMode.StringLine:
                        if(_IsDrawing && _LeaderLine == null)
                        {
                            _LeaderLine = new DrawLineString { LineColor = DrawLineColor, LineThickness = DrawLineThickness, Opacity = DrawOpacity };
                            _LeaderLine.Points.Add(_CurrentMousePoint);
                            _LeaderLine.Points.Add(_MousePoint);

                            _LeaderPoint = new DrawPoint { Point = _MousePoint };

                            _DrawLayer.Add(_LeaderLine);
                            _DrawLayer.Add(_LeaderPoint);
                        }
                        else
                        {
                            if(_LeaderLine != null)
                            {
                                _LeaderLine.Points[0] = _CurrentMousePoint;
                                _LeaderLine.Points[1] = _MousePoint;
                                _LeaderPoint.Point = _MousePoint;
                            }
                        }

                        if(_LeaderLine != null && _LeaderPoint != null)
                        {
                            _DrawLayer.UpdateShape(_LeaderLine);
                            _DrawLayer.UpdateShape(_LeaderPoint);
                        }
                        break;

                    case GeometryMode.Polygon:
                      if(_IsDrawing && _LeaderPolygon == null)
                        {
                            _LeaderPolygon = new DrawPolygon { LineColor = DrawLineColor, LineThickness = DrawLineThickness, Opacity = DrawOpacity };
                            _LeaderPolygon.Points.Add(_Polygon.Points[0]);
                            _LeaderPolygon.Points.Add(_Polygon.Points[_Polygon.Points.Count-1]);
                            _LeaderPolygon.Points.Add(_MousePoint);

                            _LeaderPoint = new DrawPoint { Point = _MousePoint };

                            _DrawLayer.Add(_LeaderPolygon);
                            _DrawLayer.Add(_LeaderPoint);
                        }
                        else
                        {
                            if(_LeaderPolygon != null)
                            {
                                _LeaderPolygon.Points[0] = _Polygon.Points[0];
                                _LeaderPolygon.Points[1] = _Polygon.Points[_Polygon.Points.Count - 1];
                                _LeaderPolygon.Points[2] = _MousePoint;

                                _LeaderPoint.Point = _MousePoint;
                            }
                        }

                        if(_LeaderPolygon != null && _LeaderPoint != null)
                        {
                            _DrawLayer.UpdateShape(_LeaderPolygon);
                            _DrawLayer.UpdateShape(_LeaderPoint);
                        }


                        break;

                    case GeometryMode.PointPin:
                        break;

                    case GeometryMode.Eraser:
                        break;
                }
            }

        }

        private void Events_MapDoubleClick(Map map, MouseButtonEventArgs args)
        {
            if(map.DragMode == Map.DragBehavior.Draw)
            {
                _CurrentMousePoint = map.CoordHelper.PixelToGeo(args.GetPosition(map));

                switch(DrawMode)
                {
                    case GeometryMode.FreeDraw:
                        break;

                    case GeometryMode.StringLine:
                        _StringLine = null;
                        ClearDrawLeader();
                        break;

                    case GeometryMode.Polygon:
                        _Polygon = null;
                        ClearDrawLeader();
                        break;

                    case GeometryMode.PointPin:
                        break;

                    case GeometryMode.Eraser:
                        break;
                }

                _IsDrawing = false;
                _IsDoubleClick = true;
            }

        }


        private void Events_MapDragBehavourChanged(Map map, MapEventArgs args)
        {
            if(map.DragMode == Map.DragBehavior.Draw)
            {
                ClearShapes();

                map.Events.EnableMouseClicks = true;
                map.Events.EnableMouseWheel = true;
                map.Events.EnableMapZoom = true;
                map.Events.EnableKeyboard = false;
                map.Events.EnableMapZoomOnDoubleClick = false;
            }
        }

        private void DrawLayers_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch(e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        int index = e.NewStartingIndex;
                        foreach(object obj in e.NewItems)
                        {
                            if(obj is ILayer)
                            {
                                var element = e.NewItems[0] as ILayer;
                                if(element != null)
                                {
                                    element.MapInstance = _Map;
                                }
                                _DrawLayerGrid.Children.Insert(index, obj as UIElement);
                                index++;
                            }
                        }
                        break;
                    }
                case NotifyCollectionChangedAction.Remove:
                    {
                        foreach(object obj in e.OldItems)
                        {
                            if(obj is UIElement)
                            {
                                var removalItem = obj as UIElement;
                                if(_DrawLayerGrid.Children.Contains(removalItem))
                                {
                                    _DrawLayerGrid.Children.RemoveAt(e.OldStartingIndex);
                                }
                            }
                        }
                        break;
                    }
            }
        }

        private void ClearShapes()
        {
            _IsDrawing = false;
            _IsDoubleClick = false;

            _FreeDrawLine = null;
            _StringLine = null;
            _Polygon = null;
            _PinPoint = null;

            _LeaderLine = null;
            _LeaderPolygon = null;
        }

        private void ClearDrawLeader()
        {
            _DrawLayer.Remove(_LeaderLine);
            _DrawLayer.Remove(_LeaderPolygon);
            _DrawLayer.Remove(_LeaderPoint);

            _LeaderLine = null;
            _LeaderPoint = null;
            _LeaderPolygon = null;
        }
    }
}

#pragma warning restore 1591