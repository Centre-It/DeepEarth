// Deep Earth is a community project available under the Microsoft Public License (Ms-PL)
// Code is provided as is and with no warrenty – Use at your own risk
// View the project and the latest code at http://DeepEarth.codeplex.com/

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Maps.MapControl;
using Microsoft.Maps.MapControl.Core;

namespace DeepEarth.BingMapsToolkit.Client.MapGeometry.Drawing
{
    public delegate void DrawPointMovedEventHandler(object sender, DrawPointMovedEventArgs args);

    public delegate void DrawPointMoveCompletedEventHandler(object sender, DrawPointMovedEventArgs args);

    public delegate void DrawPointMoveStartedEventHandler(object sender, DrawPointMovedEventArgs args);

    public delegate void DrawPointClickEventHandler(object sender, DrawPointMovedEventArgs args);

    public class DrawPoint : Control, IDisposable
    {
        private readonly MapCore map;
        private bool moved;

        public DrawPoint(MapCore mapInstance)
        {
            map = mapInstance;
            DefaultStyleKey = typeof (DrawPoint);
            MouseLeftButtonDown += DrawPoint_MouseLeftButtonDown;
        }

        public int Index { get; set; }

        public event DrawPointMovedEventHandler DrawPointMoved;
        public event DrawPointMoveCompletedEventHandler DrawPointMoveCompleted;
        public event DrawPointMoveStartedEventHandler DrawPointMoveStarted;
        public event DrawPointClickEventHandler DrawPointClick;

        protected virtual void OnDrawPointMoved(DrawPointMovedEventArgs e)
        {
            if (DrawPointMoved != null)
                DrawPointMoved(this, e);
        }

        protected virtual void OnDrawPointMoveStarted(DrawPointMovedEventArgs e)
        {
            if (DrawPointMoveStarted != null)
                DrawPointMoveStarted(this, e);
        }

        protected virtual void OnDrawPointMoveCompleted(DrawPointMovedEventArgs e)
        {
            if (DrawPointMoveCompleted != null)
                DrawPointMoveCompleted(this, e);
        }

        protected virtual void OnDrawPointClick(DrawPointMovedEventArgs e)
        {
            if (DrawPointClick != null)
                DrawPointClick(this, e);
        }

        private void map_MouseMove(object sender, MouseEventArgs e)
        {
            moved = true;
            var position = e.GetPosition(map);
            Location currentMousePoint = map.ViewportPointToLocation(position);
            SetValue(MapLayer.PositionProperty, currentMousePoint);
            OnDrawPointMoved(new DrawPointMovedEventArgs { Location = currentMousePoint, Index = Index, Point = position });
        }

        private void DrawPoint_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            moved = false;
            map.MouseMove += map_MouseMove;
            map.MouseLeftButtonUp += map_MouseLeftButtonUp;
            e.Handled = true;
            var position = e.GetPosition(map);
            Location currentMousePoint = map.ViewportPointToLocation(position);
            OnDrawPointMoveStarted(new DrawPointMovedEventArgs { Location = currentMousePoint, Index = Index, Point = position });
        }

        private void map_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            map.MouseMove -= map_MouseMove;
            map.MouseLeftButtonUp -= map_MouseLeftButtonUp;
            var position = e.GetPosition(map);
            Location currentMousePoint = map.ViewportPointToLocation(position);
            OnDrawPointMoveCompleted(new DrawPointMovedEventArgs { Location = currentMousePoint, Index = Index, Point = position });
            if (!moved)
            {
                OnDrawPointClick(new DrawPointMovedEventArgs { Location = currentMousePoint, Index = Index, Point = position });
            }
        }

        public void Dispose()
        {
            MouseLeftButtonDown -= DrawPoint_MouseLeftButtonDown;
        }
    }

    public class DrawPointMovedEventArgs : EventArgs
    {
        public Location Location { get; set; }
        public int Index { get; set; }
        public Point Point { get; set; }
    }
}