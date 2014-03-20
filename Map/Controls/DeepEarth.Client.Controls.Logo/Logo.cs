using System;
using System.Windows.Controls;

namespace DeepEarth.Client.Controls
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