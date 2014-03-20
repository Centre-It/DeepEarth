// Deep Earth is a community project available under the Microsoft Public License (Ms-PL)
// Code is provided as is and with no warrenty – Use at your own risk
// View the project and the latest code at http://DeepEarth.codeplex.com/

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Resources;
using ExampleControlBing.AppDemos;
using ExampleControlBing.ControlDemos;
using ExampleControlBing.DataDemos;

namespace ExampleControlBing
{
    /// <summary>
    /// This example silverlight application will grow to provide an example of all the 
    /// controls that can be used with the Bing Maps Silverlight control.
    /// REMEMEBER TO SET YOUR BING MAPS KEY IN THE APP.XAML
    /// </summary>
    public partial class Page
    {
        public Page()
        {
            InitializeComponent();
            Loaded += Page_Loaded;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //todo: load the examples up from an xml file?

            //Load control examples
            ExampleControlList.ItemsSource = new List<Example>
                               {
								   //Automation
                                   new Example
                                       {
                                           ControlType = typeof (AutomationExample),
                                           Source = "ControlDemos/AutomationExample.txt",
                                           Title = "Automation"
                                       },
                                   //MiniMap
                                   new Example
                                       {
                                           ControlType = typeof (MiniMapExample),
                                           Source = "ControlDemos/MiniMapExample.txt",
                                           Title = "Mini Map"
                                       },
                                   //Logo
                                   new Example
                                       {
                                           ControlType = typeof (LogoExample),
                                           Source = "ControlDemos/LogoExample.txt",
                                           Title = "DeepEarth Logo"
                                       },
                                  //Color Picker
                                   new Example
                                       {
                                           ControlType = typeof (ColorPickerExample),
                                           Source = "ControlDemos/ColorPickerExample.txt",
                                           Title = "Color Picker"
                                       },
                                  //Coordinate Panel
                                   new Example
                                       {
                                           ControlType = typeof (CoordinatePanelExample),
                                           Source = "ControlDemos/CoordinatePanelExample.txt",
                                           Title = "Coordinate Panel"
                                       },
                                  //DateRange Slider
                                   new Example
                                       {
                                           ControlType = typeof (DateRangeSliderExample),
                                           Source = "ControlDemos/DateRangeSliderExample.txt",
                                           Title = "Date Range Slider"
                                       },
                                  //Digitizer
                                   new Example
                                       {
                                           ControlType = typeof (DigitizerExample),
                                           Source = "ControlDemos/DigitizerExample.txt",
                                           Title = "DigitizerExample"
                                       },
                                  //Direction Panel
                                   new Example
                                       {
                                           ControlType = typeof (DirectionPanelExample),
                                           Source = "ControlDemos/DirectionPanelExample.txt",
                                           Title = "Direction Panel"
                                       },
                                  //Find
                                   new Example
                                       {
                                           ControlType = typeof (FindExample),
                                           Source = "ControlDemos/FindExample.txt",
                                           Title = "Find"
                                       },
                                  //Geometry Style Picker
                                   new Example
                                       {
                                           ControlType = typeof (GeometryStylePickerExample),
                                           Source = "ControlDemos/GeometryStylePickerExample.txt",
                                           Title = "Geometry Style Picker"
                                       },
                                  //Image Sprite 
                                   new Example
                                       {
                                           ControlType = typeof (ImageSpriteExample),
                                           Source = "ControlDemos/ImageSpriteExample.txt",
                                           Title = "Image Sprite"
                                       },
                                  //Layer Panel (TreeView)
                                   new Example
                                       {
                                           ControlType = typeof (LayerPanelTreeViewExample),
                                           Source = "ControlDemos/LayerPanelTreeViewExample.txt",
                                           Title = "Layer Panel (TreeView)"
                                       },
                                  //Layer Panel (Accordion)
                                   new Example
                                       {
                                           ControlType = typeof (LayerPanelAccordionExample),
                                           Source = "ControlDemos/LayerPanelAccordionExample.txt",
                                           Title = "Layer Panel (Accordion)"
                                       },
                                  //Magnifier
                                   new Example
                                       {
                                           ControlType = typeof (MagnifierExample),
                                           Source = "ControlDemos/MagnifierExample.txt",
                                           Title = "Magnifier"
                                       },
								   //MapRobot
                                   new Example
                                       {
                                           ControlType = typeof (MapRobotExample),
                                           Source = "ControlDemos/MapRobotExample.txt",
                                           Title = "Map Robot"
                                       },
                                  //Measure Tools
                                   new Example
                                       {
                                           ControlType = typeof (MeasureToolsExample),
                                           Source = "ControlDemos/MeasureToolsExample.txt",
                                           Title = "Measure Tools"
                                       },
                                  //Navigation Panel
                                   new Example
                                       {
                                           ControlType = typeof (NavigationPanelExample),
                                           Source = "ControlDemos/NavigationPanelExample.txt",
                                           Title = "Navigation Panel"
                                       },
                                  //Persisted State
                                   new Example
                                       {
                                           ControlType = typeof (PersistedStateExample),
                                           Source = "ControlDemos/PersistedStateExample.txt",
                                           Title = "Persisted State"
                                       },
                                    //Persisted State Custom Storage Example
                                    new Example
                                        {
                                            ControlType = typeof (SnapshotExample),
                                            Source = "ControlDemos/SnapshotExample.txt",
                                            Title = "Snapshot"
                                        },
                                  //Scale Panel
                                   new Example
                                       {
                                           ControlType = typeof (ScalePanelExample),
                                           Source = "ControlDemos/ScalePanelExample.txt",
                                           Title = "Scale Panel"
                                       },
								  //Status Panel
                                   new Example
                                       {
                                           ControlType = typeof (StatusPanelExample),
                                           Source = "ControlDemos/StatusPanelExample.txt",
                                           Title = "Status Panel"
                                       },
                                  //Tool Panel
                                   new Example
                                       {
                                           ControlType = typeof (ToolPanelExample),
                                           Source = "ControlDemos/ToolPanelExample.txt",
                                           Title = "Tool Panel"
                                       },
                                  //DefaultControlsStyleExample
                                   new Example
                                       {
                                           ControlType = typeof (DefaultControlsStyleExample),
                                           Source = "ControlDemos/DefaultControlsStyleExample.txt",
                                           Title = "Default Controls Styles"
                                       },
                                  ////
                                  //Welcome Control
                                   new Example
                                       {
                                           ControlType = typeof (WelcomeControlExample),
                                           Source = "ControlDemos/WelcomeControlExample.txt",
                                           Title = "Welcome Control"
                                       },
								  //FloatingPanelExample   
                                   new Example
                                       {
                                           ControlType = typeof (FloatingPanelExample),
                                           Source = "ControlDemos/FloatingPanelExample.txt",
                                           Title = "Floating Panel"
                                       },
                                  //LaunchPanelExample
                                   new Example
                                       {
                                           ControlType = typeof (LaunchPanelExample),
                                           Source = "ControlDemos/LaunchPanelExample.txt",
                                           Title = "Launch Panel"
                                       },
								   //WMSInfoToolExample
                                   new Example
                                       {
                                           ControlType = typeof (WMSInfoToolExample),
                                           Source = "ControlDemos/WMSInfoToolExample.txt",
                                           Title = "WMS Info Tool"
                                       }
                                  ////
                                  // new Example
                                  //     {
                                  //         ControlType = typeof (),
                                  //         Source = "ControlDemos/.txt",
                                  //         Title = ""
                                  //     },

                               };

            ExampleDataList.ItemsSource = new List<Example>
                               {
                                    //Single Point
                                    new Example
                                        {
                                            ControlType = typeof (SinglePoint),
                                            Source = "DataDemos/SinglePoint.txt",
                                            Title = "Single Point"
                                        },
                                   //Cluster Example
                                    new Example
                                        {
                                            ControlType = typeof (ClusteredPointsExample),
                                            Source = "DataDemos/ClusteredPointsExample.txt",
                                            Title = "Clustered Points Example"
                                        },
                                   //Complex Polygon Example
                                    new Example
                                        {
                                            ControlType = typeof (ComplexPolygonsExample),
                                            Source = "DataDemos/ComplexPolygonsExample.txt",
                                            Title = "Complex Polygons Example"
                                        },
                                   //Temporal Data Example
                                    new Example
                                        {
                                            ControlType = typeof (TemporalDataExample),
                                            Source = "DataDemos/TemporalDataExample.txt",
                                            Title = "Temporal Data Example"
                                        },
                                    //Balloon Xaml Example
                                    new Example
                                        {
                                            ControlType = typeof (BalloonXamlExample),
                                            Source = "DataDemos/BalloonXamlExample.txt",
                                            Title = "Balloon Xaml Example"
                                        }
                                   ////
                                   // new Example
                                   //     {
                                   //         ControlType = typeof (),
                                   //         Source = "DataDemos/.txt",
                                   //         Title = ""
                                   //     },

                                   
                               };

            ExampleAppList.ItemsSource = new List<Example>
                                {
                                    //Floating Panels
                                     new Example
                                         {
                                             ControlType = typeof (FloatingPanelsExample),
                                             Source = "AppDemos/FloatingPanelsExample.txt",
                                             Title = "Floating Panels"
                                         },
									 new Example
                                         {
                                             ControlType = typeof (JavascriptCallingExample),
                                             Source = "AppDemos/JavascriptCallingExample.txt",
                                             Title = "Javascript Calling"
                                         },
									 new Example
                                         {
                                             ControlType = typeof (StoreLocatorExample),
                                             Source = "AppDemos/StoreLocatorExample.txt",
                                             Title = "Store Locator"
                                         }
										 ,
									 new Example
                                         {
                                             ControlType = typeof (KMLExample),
                                             Source = "AppDemos/KMLExample.txt",
                                             Title = "KML Example"
                                         }

										 
                                    ////
                                    // new Example
                                    //     {
                                    //         ControlType = typeof (),
                                    //         Source = "AppDemos/.txt",
                                    //         Title = ""
                                    //     },
                                };

            //listen for when examples change
            ExampleControlList.SelectionChanged += ExampleList_SelectionChanged;
            ExampleDataList.SelectionChanged += ExampleList_SelectionChanged;
            ExampleAppList.SelectionChanged += ExampleList_SelectionChanged;

            //set to load 1st one
            ExampleControlList.SelectedIndex = 23;
        }

        private void ExampleList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                //unload previous controls
                while (ExamplePanel.Children.Count > 0)
                {
                    //TODO: dispose?
                    ExamplePanel.Children.RemoveAt(0);
                }

                //reset other lists
                if (sender != ExampleControlList)
                {
                    ExampleControlList.SelectedIndex = -1;
                }
                if (sender != ExampleDataList)
                {
                    ExampleDataList.SelectedIndex = -1;
                }
                if (sender != ExampleAppList)
                {
                    ExampleAppList.SelectedIndex = -1;
                }

                //load new control
                var item = (Example) e.AddedItems[0];
                var newUIElement = Activator.CreateInstance(item.ControlType) as UIElement;

                ExamplePanel.Children.Add(newUIElement);

                //todo: use nice animation, flip or flick off?

                //update source

                StreamResourceInfo sri =
                    Application.GetResourceStream(new Uri("/ExampleControlBing;component/" + item.Source,
                                                          UriKind.Relative));

                //Did you remember to set the Build Action in the properties window of the text file to "Resource" ???

                using (TextReader tr = new StreamReader(sri.Stream))
                {
                    ExampleSource.Text = tr.ReadToEnd();
                }
            }
        }
    }
}