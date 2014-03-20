using System;

namespace DeepEarth.Client.Controls.GeometryStylePicker
{
    public class OpacityChangedEventArgs: EventArgs
    {
        public double oldOpacity;
        public double newOpacity;

        public OpacityChangedEventArgs(double oldOpacity, double newOpacity)
        {
            this.oldOpacity = oldOpacity;
            this.newOpacity = newOpacity;
        }

    }
}
