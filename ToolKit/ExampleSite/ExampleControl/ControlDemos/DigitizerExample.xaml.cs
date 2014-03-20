// Deep Earth is a community project available under the Microsoft Public License (Ms-PL)
// Code is provided as is and with no warrenty – Use at your own risk
// View the project and the latest code at http://DeepEarth.codeplex.com/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using DeepEarth.BingMapsToolkit.Client.Common.Entities;
using DeepEarth.BingMapsToolkit.Common.Entities;

namespace ExampleControlBing.ControlDemos
{
    public partial class DigitizerExample
    {
        public DigitizerExample()
        {
            InitializeComponent();
            Loaded += DigitizerExample_Loaded;
        }

        void DigitizerExample_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            var styles = new Dictionary<string, StyleSpecification>();
            styles.Add("defaultstyle", new StyleSpecification
            {
                ID = "style 1",
                LineColour = "FF1B0AA5",
                LineWidth = 2,
                PolyFillColour = "88677E1E",
                ShowFill = true,
                ShowLine = true,
                IconURL = "http://soulsolutions.com.au/Images/pin.png",
                IconScale = 2
            });

            digitizer.Styles = styles;


            var layers = new ObservableCollection<LayerDefinition>();
            layers.Add(new LayerDefinition
                           {
                               IsEditable = true,
                               LayerAlias = "Scratch Layer"
                           });


            digitizer.Layers = layers;
        }
    }
}
