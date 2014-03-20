// Deep Earth is a community project available under the Microsoft Public License (Ms-PL)
// Code is provided as is and with no warrenty – Use at your own risk
// View the project and the latest code at http://DeepEarth.codeplex.com/

using System;
using System.Windows.Media;

namespace DeepEarth.BingMapsToolkit.Client.Controls
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