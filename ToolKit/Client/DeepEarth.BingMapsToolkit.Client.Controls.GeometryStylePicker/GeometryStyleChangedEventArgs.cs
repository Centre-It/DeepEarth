using System;

namespace DeepEarth.Client.Controls.GeometryStylePicker
{
    public class GeometryStyleChangedEventArgs : EventArgs
    {
        public GeometryStyle oldStyle;
        public GeometryStyle newStyle;

        public GeometryStyleChangedEventArgs(GeometryStyle oldStyle, GeometryStyle newStyle)
        {
            this.oldStyle = oldStyle;
            this.newStyle = newStyle;
        }
    }
}
