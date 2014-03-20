using System;
using System.Windows.Controls;

namespace DeepEarth.BingMapsToolkit.Client.Controls
{
    public class Logo : Control, IDisposable
    {
        public Logo()
        {
            DefaultStyleKey = typeof (Logo);
        }

        #region IDisposable Members

        public void Dispose()
        {
        }

        #endregion
    }
}