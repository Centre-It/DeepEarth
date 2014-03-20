// Deep Earth is a community project available under the Microsoft Public License (Ms-PL)
// Code is provided as is and with no warrenty – Use at your own risk
// View the project and the latest code at http://DeepEarth.codeplex.com/

using System;
using DeepEarth.BingMapsToolkit.Client.Common.Entities;
using Microsoft.Maps.MapControl;

namespace DeepEarth.BingMapsToolkit.Client.MapGeometry.Drawing
{
    public delegate void DrawingCompletedEventHandler(object sender, DrawingEventArgs args);
    public delegate void DrawingChangedEventHandler(object sender, DrawingEventArgs args);

    public abstract class EditObjectBase : IDisposable
    {
        public Guid ID { get; set; }
        public virtual StyleSpecification GeometryStyle { get; set; }
        public virtual LocationCollection Locations { get; set; }

        #region IDisposable Members

        public virtual void Dispose() {}

        #endregion

        public event DrawingCompletedEventHandler DrawingCompleted;
        public event DrawingChangedEventHandler DrawingChanged;
        public event EventHandler Click;

        protected virtual void OnDrawingCompleted(DrawingEventArgs e)
        {
            if (DrawingCompleted != null)
                DrawingCompleted(this, e);
        }
        protected virtual void OnDrawingChanged(DrawingEventArgs e)
        {
            if (DrawingChanged != null)
                DrawingChanged(this, e);
        }
        protected virtual void OnClick(EventArgs e)
        {
            if (Click != null)
                Click(this, e);
        }

    }

    public class DrawingEventArgs : EventArgs
    {
        public LocationCollection Locations { get; set; }
    }
}