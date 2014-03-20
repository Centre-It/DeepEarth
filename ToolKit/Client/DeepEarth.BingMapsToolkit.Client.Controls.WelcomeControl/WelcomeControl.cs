// Deep Earth is a community project available under the Microsoft Public License (Ms-PL)
// Code is provided as is and with no warrenty – Use at your own risk
// View the project and the latest code at http://DeepEarth.codeplex.com/

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using DeepEarth.BingMapsToolkit.Client.Common;

namespace DeepEarth.BingMapsToolkit.Client.Controls
{
    public class WelcomeControl : Control
    {
        private const string PersistedTimeStampKey = "PersistedTimeStampKey";

        public string Title { get; set; }

        public event EventHandler Closed;

        protected void onClosed()
        {
            if (Closed != null)
            {
                Closed(this, new EventArgs());
            }
        }

        public void ShowWelcomeDialog(string xamlContent, DateTime contentCreationTimeStamp)
        {
            ShowWelcomeDialog(xamlContent, contentCreationTimeStamp, false);
        }

        public void ShowWelcomeDialog(string xamlContent, DateTime contentCreationTimeStamp, bool forceShow)
        {
            var settings = IsolatedStorage.LoadData<TimeStampSettings>(PersistedTimeStampKey);
            if (forceShow || settings == null || settings.TimeStamp < contentCreationTimeStamp)
            {
                var dialog = new WelcomeDialog{Title = Title};
                try
                {
                    var element = (UIElement) XamlReader.Load(xamlContent);
                    dialog.WelcomeContent = element;
                    dialog.Closed += (o, e) => onClosed();
                    dialog.Show();
                }catch(Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
                IsolatedStorage.SaveData(new TimeStampSettings { TimeStamp = contentCreationTimeStamp }, PersistedTimeStampKey);
            }
        }
    }
}