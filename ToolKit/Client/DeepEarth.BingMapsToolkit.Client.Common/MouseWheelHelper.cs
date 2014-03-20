// Deep Earth is a community project available under the Microsoft Public License (Ms-PL)
// Code is provided as is and with no warrenty – Use at your own risk
// View the project and the latest code at http://DeepEarth.codeplex.com/

using System;
using System.Windows;
using System.Windows.Browser;

namespace DeepEarth.BingMapsToolkit.Client.Common
{
    public class MouseWheelEventArgs : EventArgs
    {
        private readonly double delta;

        public MouseWheelEventArgs(double delta)
        {
            this.delta = delta;
        }

        public double Delta
        {
            get { return delta; }
        }

        // Use handled to prevent the default browser behavior!
        public bool Handled { get; set; }
    }

    public class MouseWheelHelper
    {
        private static Worker worker;
        private bool isMouseOver;

        public MouseWheelHelper(UIElement element)
        {
            if (worker == null)
                worker = new Worker();

            worker.Moved += HandleMouseWheel;

            element.MouseEnter += HandleMouseEnter;
            element.MouseLeave += HandleMouseLeave;
            element.MouseMove += HandleMouseMove;
        }

        public event EventHandler<MouseWheelEventArgs> Moved;

        private void HandleMouseWheel(object sender, MouseWheelEventArgs args)
        {
            if (isMouseOver)
                Moved(this, args);
        }

        private void HandleMouseEnter(object sender, EventArgs e)
        {
            isMouseOver = true;
        }

        private void HandleMouseLeave(object sender, EventArgs e)
        {
            isMouseOver = false;
        }

        private void HandleMouseMove(object sender, EventArgs e)
        {
            isMouseOver = true;
        }

        #region Nested type: Worker

        private class Worker
        {
            public Worker()
            {
                if (HtmlPage.IsEnabled)
                {
                    HtmlPage.Window.AttachEvent("DOMMouseScroll", HandleMouseWheel);
                    HtmlPage.Window.AttachEvent("onmousewheel", HandleMouseWheel);
                    HtmlPage.Document.AttachEvent("onmousewheel", HandleMouseWheel);
                }
            }

            public event EventHandler<MouseWheelEventArgs> Moved;

            private void HandleMouseWheel(object sender, HtmlEventArgs args)
            {
                double delta = 0;

                ScriptObject eventObj = args.EventObject;

                if (eventObj.GetProperty("wheelDelta") != null)
                {
                    delta = ((double) eventObj.GetProperty("wheelDelta"))/120;


                    if (HtmlPage.Window.GetProperty("opera") != null)
                        delta = -delta;
                }
                else if (eventObj.GetProperty("detail") != null)
                {
                    delta = -((double) eventObj.GetProperty("detail"))/3;

                    if (HtmlPage.BrowserInformation.UserAgent.IndexOf("Macintosh") != -1)
                        delta = delta*3;
                }

                if (delta != 0 && Moved != null)
                {
                    var wheelArgs = new MouseWheelEventArgs(delta);
                    Moved(this, wheelArgs);

                    if (wheelArgs.Handled)
                        args.PreventDefault();
                }
            }
        }

        #endregion
    }
}