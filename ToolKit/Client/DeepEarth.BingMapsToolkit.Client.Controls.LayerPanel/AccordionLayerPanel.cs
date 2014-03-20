// Deep Earth is a community project available under the Microsoft Public License (Ms-PL)
// Code is provided as is and with no warrenty – Use at your own risk
// View the project and the latest code at http://DeepEarth.codeplex.com/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Linq;
using System.Windows.Media;

using GisSharpBlog.NetTopologySuite.IO;
using GisSharpBlog.NetTopologySuite.Geometries;

using DeepEarth.BingMapsToolkit.Client.Common.Commanding;
using DeepEarth.BingMapsToolkit.Common.Entities;
using DeepEarth.BingMapsToolkit.Client.Common;
using Microsoft.Maps.MapControl;
using DeepEarth.BingMapsToolkit.Client.Common.Converters;
using DeepEarth.BingMapsToolkit.Client.Controls.Dialogs;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Json;

namespace DeepEarth.BingMapsToolkit.Client.Controls
{
    [TemplatePart(Name = PART_LayerAccordion, Type = typeof(TreeView))]
    [TemplatePart(Name = PART_ToggleLayers, Type = typeof(Button))]
    [TemplatePart(Name = PART_ToggleLabels, Type = typeof(Button))]
    [TemplateVisualState(Name = VSM_Normal, GroupName = VSM_CommonStates)]
    [TemplateVisualState(Name = VSM_MouseOver, GroupName = VSM_CommonStates)]
    public class AccordionLayerPanel : LayerPanel
    {
        private const string PART_LayerAccordion = "PART_LayerAccordion";
        private const string PART_ToggleLayers = "PART_ToggleLayers";
        private const string PART_ToggleLabels = "PART_ToggleLabels";
        private const string VSM_CommonStates = "CommonStates";
        private const string VSM_MouseOver = "MouseOver";
        private const string VSM_Normal = "Normal";

        private const string LayerPanel_GotoButton = "LayerPanel_GotoButton";
        private const string LayerPanel_VectorDataViewButton = "LayerPanel_VectorDataViewButton";

        private const string ISOKey = "AccordionLayerPanel.SelectedLayers";

        private ObservableCollection<LayerDefinition> layers;

        private Accordion layerAccordion;
        private Button toggleLayers;
        private Button toggleLabels;

        private string persistedSetting;

        private List<Button> allGotoButtons = new List<Button>();
        private List<Button> allVectorDataButtons = new List<Button>();

        private Dictionary<string, bool> layersToBeSetup;

        public event EventHandler<AccordionLayerPanelGotoEventArgs> OnGoToLayer = (s, e) => { ;};
        public event EventHandler<AccordionLayerPanelGotoFeatureEventArgs> OnGoToFeature = (s, e) => { ;};
        public event EventHandler<VectorDataRequestEventArgs> OnVectorDataRequest = (s, e) => { ;};
        
        public AccordionLayerPanel()
        {
            layers = new ObservableCollection<LayerDefinition>();
            DefaultStyleKey = typeof(AccordionLayerPanel);
            Loaded += LayerPanel_Loaded;
            PersistedCommands.PersistedStateLoadedCommand.Executed += PersistedStateLoadedCommand_Executed;
        }

        public override ObservableCollection<LayerDefinition> Layers
        {
            get { return layers; }
            set
            {
                layers = value;
                //If we have a set of layer setup info then we want to ignore the database defaults and set manually
                bool useSelected = (layersToBeSetup == null);
                if (layerAccordion != null)
                {
                    layerAccordion.ItemsSource = processLayersIntoHierarchy(layers, useSelected);
                    foreach (var def in layers)
                    {
                        def.PropertyChanged += definition_PropertyChanged;
                        def.Selected = def.Selected;
                        def.LabelOn = def.LabelOn;
                    }
                    if (!useSelected)
                    {
                        //set layer and label visibility.
                        setLayerVisibilities(layersToBeSetup);
                    }
                }
            }
        }

        private void LayerPanel_Loaded(object sender, RoutedEventArgs e)
        {
            IsEnabled = true;
            ApplyTemplate();

            PersistedCommands.PersistedStateSaveCommand.Executed += PersistedStateSaveCommand_Executed;
        }

        private void PersistedStateSaveCommand_Executed(object sender, ExecutedEventArgs e)
        {
            ((PersistedSettings)e.Parameter).CustomSettings[ISOKey] = GetSelectedLayerIDs(Layers);
        }

        private void PersistedStateLoadedCommand_Executed(object sender, ExecutedEventArgs e)
        {
            if (((PersistedSettings)e.Parameter).CustomSettings.ContainsKey(ISOKey))
            {
                persistedSetting = ((PersistedSettings)e.Parameter).CustomSettings[ISOKey];
                applyPersistedState();
            }
        }

        private void applyPersistedState()
        {
            if (persistedSetting != null)
            {
                Dictionary<string, bool> dict = new Dictionary<string, bool>();

                DataContractJsonSerializer contract = new System.Runtime.Serialization.Json.DataContractJsonSerializer(dict.GetType());
                MemoryStream stream = new MemoryStream();

                foreach (var item in Encoding.Unicode.GetBytes(persistedSetting))
                {
                    stream.WriteByte(item);
                }
                stream.Position = 0;

                dict = (Dictionary<string, bool>)contract.ReadObject(stream);

                //If the treeview is setup then we can configure, else have to wait till it is ready.
                if (layerAccordion != null && layerAccordion.ItemsSource != null)
                {
                    //set layer and label visibility.
                    setLayerVisibilities(dict);
                }
                else
                {
                    //save for later
                    layersToBeSetup = dict;
                }
            }
        }

        public static string GetSelectedLayerIDs(IEnumerable<LayerDefinition> layerDefinitions)
        {
            Dictionary<string, bool> visibleLayers = new Dictionary<string, bool>();
            string result = "";
            if (layerDefinitions != null)
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    DataContractJsonSerializer contract = new System.Runtime.Serialization.Json.DataContractJsonSerializer(visibleLayers.GetType());

                    foreach (var layerDefinition in layerDefinitions)
                    {
                        if (layerDefinition.Selected)
                        {
                            if (visibleLayers.ContainsKey(layerDefinition.LayerID))
                            {
                                if ((visibleLayers[layerDefinition.LayerID] == false) && (layerDefinition.LabelOn == true))
                                {
                                    visibleLayers[layerDefinition.LayerID] = true;
                                }
                            }
                            else
                            {
                                visibleLayers.Add(layerDefinition.LayerID, layerDefinition.LabelOn);
                            }
                        }
                    }

                    contract.WriteObject(stream, visibleLayers);

                    stream.Position = 0;

                    using (StreamReader streamReader = new StreamReader(stream))
                    {
                        result = streamReader.ReadToEnd();
                    }
                }
            }

            return result;
        }

        private void setLayerVisibilities(Dictionary<string, bool> dict)
        {
            //reset all
            if ((layerAccordion.ItemsSource != null) && (layers != null))
            {


                foreach (LayerAccordionItemGroup accordionItem in layerAccordion.ItemsSource)
                {
                    foreach (LayerDefinition layerDefinition in accordionItem.Layers)
                    {
                        layerDefinition.Selected = false;
                        layerDefinition.LabelOn = false;
                    }
                }

                foreach (KeyValuePair<string, bool> item in dict)
                {
                    bool labelVisible = item.Value;
                    string id = item.Key;
                    foreach (LayerAccordionItemGroup accordionItem in layerAccordion.ItemsSource)
                    {
                        foreach (LayerDefinition layerDefinition in accordionItem.Layers)
                        {
                            if (layerDefinition.LayerID == id)
                            {
                                layerDefinition.Selected = true;
                                layerDefinition.LabelOn = labelVisible;
                                break;
                            }
                        }
                    }
                }
            }
        }

        private List<LayerAccordionItemGroup> processLayersIntoHierarchy(IEnumerable<LayerDefinition> LayerDefinitions, bool useSelected)
        {
            var Tags = LayerDefinitions.Select(u => u.Tags.Split(new char[] { ',' })).SelectMany(u => u).Distinct();

            List<LayerAccordionItemGroup> layerAccordionGroup = new List<LayerAccordionItemGroup>();

            foreach (string tag in Tags)
            {
                LayerAccordionItemGroup accordionGroup = new LayerAccordionItemGroup();
                accordionGroup.Layers = LayerDefinitions.Where(u => u.Tags.Split(new char[] { ',' }).Any(s => s.Equals(tag))).ToList();
                accordionGroup.Name = tag;
                accordionGroup.Selected = false;

                if (!string.IsNullOrEmpty(accordionGroup.Name))
                {
                    accordionGroup.PropertyChanged += new PropertyChangedEventHandler(accordionGroup_PropertyChanged);
                }

                layerAccordionGroup.Add(accordionGroup);
            }

            foreach (var def in LayerDefinitions)
            {
                def.PropertyChanged += definition_PropertyChanged;
            }

            return layerAccordionGroup;
        }

        void accordionGroup_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var layerAccordionItemGroup = (LayerAccordionItemGroup)sender;
            if (e.PropertyName.Equals("Selected"))
            {
                if (layerAccordionItemGroup.Selected == false)
                {
                    foreach (var layer in layerAccordionItemGroup.Layers)
                    {
                        layer.LabelOn = false;
                        layer.Selected = false;
                    }
                }
                else
                {
                    foreach (var layer in layerAccordionItemGroup.Layers)
                    {
                        layer.LabelOn = true;
                        layer.Selected = true;
                    }
                }
            }
        }

        private void definition_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var layerDefinition = (LayerDefinition)sender;
            switch (e.PropertyName)
            {
                case "Selected":
                    toggleLayer(layerDefinition);
                    if (layerDefinition.Selected == true)
                    {
                        var list = ((List<LayerAccordionItemGroup>)layerAccordion.ItemsSource).Where(u => u.Layers.Contains(layerDefinition));
                        foreach (var l in list)
                        {
                            l.PropertyChanged -= accordionGroup_PropertyChanged;
                            l.Selected = true;
                            l.PropertyChanged += accordionGroup_PropertyChanged;
                        }
                    }
                    else
                    {
                        var list = ((List<LayerAccordionItemGroup>)layerAccordion.ItemsSource).
                            Where(u => (u.Layers.Contains(layerDefinition))).
                            Where(l => l.Layers.All(s => s.Selected == false)).
                            Where(o => o.Selected == true);
                        foreach (var l in list)
                        {
                            if (l.Selected == true)
                                l.Selected = false;
                        }
                    }

                    //        RefreshPolling.resetPollingWorker(
                    //            (ObservableCollection<LayerTreeViewItem>)layerAccordion.ItemsSource);
                    break;
                case "LabelOn":
                    if ((layerDefinition.LabelOn == true) && (layerDefinition.Selected == false))
                        layerDefinition.Selected = true;
                    toggleLabel(layerDefinition);
                    break;
            }

            new SolidColorBrush();
        }

        private void toggleLabel(LayerDefinition layerDefinition)
        {
            switch (layerDefinition.LayerType)
            {
                case 1: //Vector Layer
                    Vector.SetLabelVisibility(layerDefinition, MapInstance);
                    break;
            }
        }

        private void toggleLayer(LayerDefinition layerDefinition)
        {
            if (layerDefinition.Selected)
            {
                //load layer
                LoadLayer(layerDefinition);

            }
            else
            {
                //hide layer if loaded
                UnloadLayer(layerDefinition);
            }
        }


        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            layerAccordion = (Accordion)GetTemplateChild(PART_LayerAccordion);
            toggleLayers = (Button)GetTemplateChild(PART_ToggleLayers);
            toggleLabels = (Button)GetTemplateChild(PART_ToggleLabels);

            if (toggleLayers != null)
            {
                toggleLayers.Click += toggleLayers_Click;
            }

            if (toggleLabels != null)
            {
                toggleLabels.Click += toggleLabels_Click;
            }
            if (layerAccordion != null)
            {
                layerAccordion.MouseEnter += new MouseEventHandler(layerAccordion_MouseEnter);
            }

            IsEnabled = true;
            applyPersistedState();
        }

        //lazy event attachment
        void layerAccordion_MouseEnter(object sender, MouseEventArgs e)
        {
            List<Button> gotoButtons = Utilities.FindAllVisualChildByName<Button>(layerAccordion, LayerPanel_GotoButton);
            foreach (Button button in gotoButtons)
            {
                if (!allGotoButtons.Contains(button))
                {
                    allGotoButtons.Add(button);
                    button.Click += GoToButton_Click;
                }
            }

            List<Button> vectorDataButtons = Utilities.FindAllVisualChildByName<Button>(layerAccordion, LayerPanel_VectorDataViewButton);
            foreach (Button button in vectorDataButtons)
            {
                if (!allVectorDataButtons.Contains(button))
                {
                    allVectorDataButtons.Add(button);
                    button.Click += VectorData_Click;
                }
            }
        }

        void VectorData_Click(object sender, RoutedEventArgs e)
        {
            var lri = ((LayerDefinition)((Button)sender).DataContext);

            VectorDataRequestEventArgs args = new VectorDataRequestEventArgs();

            args.LayerID = lri.LayerID;
            args.OnVectorDataRequestEnd = (u) =>
                {
                    ShowVectorData(u, lri.LayerID);
                };

            OnVectorDataRequest(this, args);
        }

        void ShowVectorData(IEnumerable<GeoVectorData> vectorData, string layerID)
        {
            LayerVectorDataDlg a = new LayerVectorDataDlg(OnGoToFeature, layerID);
            a.SetVectorData(vectorData);
            a.Show();
        }

        void GoToButton_Click(object sender, RoutedEventArgs e)
        {
            var lri = ((LayerDefinition)((Button)sender).DataContext);
            if (lri.Selected == false)
                lri.Selected = true;
            OnGoToLayer(null, new AccordionLayerPanelGotoEventArgs() { LayerID = lri.LayerID });
        }

        private void toggleLayers_Click(object sender, RoutedEventArgs e)
        {
            if (layerAccordion.ItemsSource != null)
            {
                var LayerItems = (ObservableCollection<LayerAccordionItemGroup>)layerAccordion.ItemsSource;
                if (LayerItems.Count > 0)
                {
                    var visible = !LayerItems[0].Selected;
                    foreach (var item in LayerItems)
                    {
                        foreach (var layer in item.Layers)
                        {
                            layer.Selected = visible;
                        }
                    }
                }
            }
        }

        private void toggleLabels_Click(object sender, RoutedEventArgs e)
        {
            if (layerAccordion.ItemsSource != null)
            {
                var LayerItems = (ObservableCollection<LayerAccordionItemGroup>)layerAccordion.ItemsSource;
                if (LayerItems.Count > 0)
                {
                    bool visible = !LayerItems[0].Selected;
                    foreach (LayerAccordionItemGroup item in LayerItems)
                    {
                        foreach (LayerDefinition layer in item.Layers)
                        {
                            layer.LabelOn = visible;
                        }
                    }
                }
            }
        }

        public new void Dispose()
        {
            if (toggleLayers != null)
            {
                toggleLayers.Click -= toggleLayers_Click;
            }

            if (toggleLabels != null)
            {
                toggleLabels.Click -= toggleLabels_Click;
            }
            base.Dispose();
        }
    }
}