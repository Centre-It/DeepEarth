using System;
using System.Windows.Media;

namespace ExampleControl.Controls.ColourPicker
{
    public class ColorChangedEventArgs : EventArgs
    {
        public SolidColorBrush newColor;
        public SolidColorBrush oldColor;

        public ColorChangedEventArgs(SolidColorBrush oldColor, SolidColorBrush newColor)
        {
            this.oldColor = oldColor;
            this.newColor = newColor;
        }
    }
}