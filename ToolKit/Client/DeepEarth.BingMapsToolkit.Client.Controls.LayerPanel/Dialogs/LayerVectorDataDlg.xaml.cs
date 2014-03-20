using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using DeepEarth.BingMapsToolkit.Common.Entities;

namespace DeepEarth.BingMapsToolkit.Client.Controls.Dialogs
{
    public partial class LayerVectorDataDlg : ChildWindow
    {
        private event EventHandler<AccordionLayerPanelGotoFeatureEventArgs> onGoToFeature;
        private string layerID = "";

        public LayerVectorDataDlg(EventHandler<AccordionLayerPanelGotoFeatureEventArgs> OnGoToFeature, string LayerID)
        {
            onGoToFeature += OnGoToFeature;
            layerID = LayerID;
            InitializeComponent();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        public void SetVectorData(IEnumerable<GeoVectorData> vectorData)
        {
            VectorData.ItemsSource = vectorData;
        }

        private void GotoButton_Click(object sender, RoutedEventArgs e)
        {
            var data = (GeoVectorData)((Button)sender).DataContext;
            AccordionLayerPanelGotoFeatureEventArgs args = new AccordionLayerPanelGotoFeatureEventArgs();
            
            args.LayerID = layerID;
            args.ItemID = data.ID;

            onGoToFeature(this, args);
        }
    }
}

