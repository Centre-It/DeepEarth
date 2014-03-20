using System;
using DeepEarth.BingMapsToolkit.Client.MapGeometry.Drawing;
using Microsoft.Maps.MapControl.Core;

namespace DeepEarth.BingMapsToolkit.Client.MapGeometry
{
    public class MeasureGeometry : EditGeometry
    {

        const int MouseDoubleClickSpeed = 3000000;
        long lastClick = 0;

        public MeasureGeometry() { }

        public MeasureGeometry(MapCore map)
        {
            base.MapInstance = map;
        }

        internal void RaiseDoubleClick(object sender, EventArgs args)
        {
            //try
            //{
                drawingCompleted(this, null);
            //}
            //catch { }
        }

        protected override void OnClicked(object sender, EventArgs args)
        {
            
            if (((DateTime.Now.Ticks - lastClick) < MouseDoubleClickSpeed) && ((DateTime.Now.Ticks - lastClick) > MouseDoubleClickSpeed/5))
            {
                RaiseDoubleClick(sender, args);                
            }
            lastClick = DateTime.Now.Ticks;
        }

        protected override void drawingCompleted(object sender, DrawingEventArgs args)
        {
            base.drawingCompleted(sender, args);
        }


    }
}
