// Deep Earth is a community project available under the Microsoft Public License (Ms-PL)
// Code is provided as is and with no warrenty – Use at your own risk
// View the project and the latest code at http://DeepEarth.codeplex.com/

using System.Windows;
using System.Windows.Controls;
using DeepEarth.BingMapsToolkit.Client.Controls;

namespace ExampleControlBing.ControlDemos
{
    public partial class FloatingPanelExample
    {
        public FloatingPanelExample()
        {
            InitializeComponent();
            fpanel1.Closed += (o, e) => show.Visibility = Visibility.Visible;
            fpanel1.SnapPositionChanged += (o, e) =>
                                               {
                                                   switch (fpanel1.SnapPosition)
                                                   {
                                                       case SnapPosition.Top:
                                                           optionTop.IsChecked = true;
                                                           break;

                                                       case SnapPosition.Left:
                                                           optionLeft.IsChecked = true;
                                                           break;

                                                       case SnapPosition.Right:
                                                           optionRight.IsChecked = true;
                                                           break;

                                                       case SnapPosition.Bottom:
                                                           optionBottom.IsChecked = true;
                                                           break;

                                                       case SnapPosition.TopLeft:
                                                           optionTopLeft.IsChecked = true;
                                                           break;

                                                       case SnapPosition.TopRight:
                                                           optionTopRight.IsChecked = true;
                                                           break;

                                                       case SnapPosition.BottomLeft:
                                                           optionBottomLeft.IsChecked = true;
                                                           break;

                                                       case SnapPosition.BottomRight:
                                                           optionBottomRight.IsChecked = true;
                                                           break;
                                                       default:
                                                           optionTop.IsChecked = false;
                                                           optionLeft.IsChecked = false;
                                                           optionRight.IsChecked = false;
                                                           optionBottom.IsChecked = false;
                                                           optionTopLeft.IsChecked = false;
                                                           optionTopRight.IsChecked = false;
                                                           optionBottomLeft.IsChecked = false;
                                                           optionBottomRight.IsChecked = false;
                                                           break;
                                                   }
                                               };
        }

        private void show_Click(object sender, RoutedEventArgs e)
        {
            fpanel1.Open();
            show.Visibility = Visibility.Collapsed;
        }

        private void move_Click(object sender, RoutedEventArgs e)
        {
            fpanel1.WindowPosition = new Point(0, 0);
        }

        private void option_Checked(object sender, RoutedEventArgs e)
        {
            if (optionTop.IsChecked.GetValueOrDefault())
            {
                fpanel1.SnapPosition = SnapPosition.Top;
                return;
            }
            if (optionLeft.IsChecked.GetValueOrDefault())
            {
                fpanel1.SnapPosition = SnapPosition.Left;
                return;
            }
            if (optionBottom.IsChecked.GetValueOrDefault())
            {
                fpanel1.SnapPosition = SnapPosition.Bottom;
                return;
            }
            if (optionRight.IsChecked.GetValueOrDefault())
            {
                fpanel1.SnapPosition = SnapPosition.Right;
                return;
            }
            if (optionTopLeft.IsChecked.GetValueOrDefault())
            {
                fpanel1.SnapPosition = SnapPosition.TopLeft;
                return;
            }
            if (optionTopRight.IsChecked.GetValueOrDefault())
            {
                fpanel1.SnapPosition = SnapPosition.TopRight;
                return;
            }
            if (optionBottomLeft.IsChecked.GetValueOrDefault())
            {
                fpanel1.SnapPosition = SnapPosition.BottomLeft;
                return;
            }
            if (optionBottomRight.IsChecked.GetValueOrDefault())
            {
                fpanel1.SnapPosition = SnapPosition.BottomRight;
                return;
            }
        }

        private void addcontent_Click(object sender, RoutedEventArgs e)
        {
            var label = new TextBlock
                            {
                                Text = "Some filler content"
                            };
            internalPanel.Children.Add(label);
        }
    }
}