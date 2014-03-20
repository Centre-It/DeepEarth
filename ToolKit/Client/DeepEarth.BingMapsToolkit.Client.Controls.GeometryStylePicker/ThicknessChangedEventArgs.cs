using System;

namespace DeepEarth.Client.Controls.GeometryStylePicker
{
    public class ThicknessChangedEventArgs: EventArgs
    {
        public double oldThickness;
        public double newThickness;

        public ThicknessChangedEventArgs(double oldThickness, double newThickness)
        {
            this.oldThickness = oldThickness;
            this.newThickness = newThickness;
        }
    }
}
