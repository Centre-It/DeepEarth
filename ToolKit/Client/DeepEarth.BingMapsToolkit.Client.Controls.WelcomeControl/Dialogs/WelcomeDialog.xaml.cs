// Deep Earth is a community project available under the Microsoft Public License (Ms-PL)
// Code is provided as is and with no warrenty – Use at your own risk
// View the project and the latest code at http://DeepEarth.codeplex.com/

using System.Windows;
using System.Windows.Controls;

namespace DeepEarth.BingMapsToolkit.Client.Controls
{
    public partial class WelcomeDialog : ChildWindow
    {
        private UIElement content;

        public WelcomeDialog()
        {
            InitializeComponent();
        }

        public UIElement WelcomeContent
        {
            get { return content; }
            set
            {
                content = value;
                if (ContentGrid.Children != null)
                {
                    if (ContentGrid.Children.Count > 0)
                    {
                        ContentGrid.Children.Clear();
                    }
                    ContentGrid.Children.Add(content);
                }
            }
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}