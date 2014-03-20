// Deep Earth is a community project available under the Microsoft Public License (Ms-PL)
// Code is provided as is and with no warrenty – Use at your own risk
// View the project and the latest code at http://DeepEarth.codeplex.com/

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DeepEarth.BingMapsToolkit.Client.Common.Commanding;
using DeepEarth.BingMapsToolkit.Common.Entities;

namespace DeepEarth.BingMapsToolkit.Client.Controls
{
    [TemplatePart(Name = PART_LayerTreeview, Type = typeof(TreeView))]
    [TemplatePart(Name = PART_ToggleLayers, Type = typeof(Button))]
    [TemplatePart(Name = PART_ToggleLabels, Type = typeof(Button))]
    [TemplateVisualState(Name = VSM_Normal, GroupName = VSM_CommonStates)]
    [TemplateVisualState(Name = VSM_MouseOver, GroupName = VSM_CommonStates)]
    public class TreeViewLayerPanel : LayerPanel
    {
        private const string PART_LayerTreeview = "PART_LayerTreeview";
        private const string PART_ToggleLayers = "PART_ToggleLayers";
        private const string PART_ToggleLabels = "PART_ToggleLabels";
        private const string VSM_CommonStates = "CommonStates";
        private const string VSM_MouseOver = "MouseOver";
        private const string VSM_Normal = "Normal";
        private const string ISOKey = "TreeViewLayerPanel.SelectedLayers";


        private bool isMouseOver;
        private ObservableCollection<LayerDefinition> layers;

        private TreeView layerTreeview;
        private Button toggleLayers;
        private Button toggleLabels;

        private string persistedSetting;
        private string[] layersToBeSetup;


        public TreeViewLayerPanel()
        {
            IsEnabled = false;
            layers = new ObservableCollection<LayerDefinition>();
            DefaultStyleKey = typeof(TreeViewLayerPanel);
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
                if (layerTreeview != null)
                {
                    layerTreeview.ItemsSource = processLayersIntoHierarchy(layers, useSelected);
                    if (!useSelected)
                    {
                        //set layer and label visibility.
                        setLayerVisibilities(layersToBeSetup);
                    }
                    //Setup background polling worker
                    RefreshPolling.resetPollingWorker(
                        (ObservableCollection<LayerTreeViewItem>)layerTreeview.ItemsSource);
                }
            }
        }

        private void LayerPanel_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyTemplate();

            MouseEnter += mouseEnter;
            MouseLeave += mouseLeave;
            GoToState(true);

            PersistedCommands.PersistedStateSaveCommand.Executed += PersistedStateSaveCommand_Executed;
        }


        private void PersistedStateSaveCommand_Executed(object sender, ExecutedEventArgs e)
        {
            ((PersistedSettings)e.Parameter).CustomSettings.Add(ISOKey, Utility.GetSelectedLayerIDs(Layers));
        }

        private void PersistedStateLoadedCommand_Executed(object sender, ExecutedEventArgs e)
        {
            persistedSetting = ((PersistedSettings)e.Parameter).CustomSettings[ISOKey];
            applyPersistedState();
        }

        private void applyPersistedState()
        {
            if (persistedSetting != null)
            {
                //split string
                var items = persistedSetting.Split(',');
                //If the treeview is setup then we can configure, else have to wait till it is ready.
                if (layerTreeview != null && layerTreeview.ItemsSource != null)
                {
                    //set layer and label visibility.
                    setLayerVisibilities(items);
                }
                else
                {
                    //save for later
                    layersToBeSetup = items;
                }
            }
        }

        private void setLayerVisibilities(IEnumerable<string> items)
        {
            //reset all
            if (layerTreeview.ItemsSource != null)
            {
                foreach (
                    LayerTreeViewItem treeViewItem in layerTreeview.ItemsSource)
                {
                    foreach (LayerDefinition layerDefinition in treeViewItem.Layers)
                    {
                        layerDefinition.Selected = false;
                        layerDefinition.LabelOn = false;
                    }
                    foreach (LayerTreeViewItem subtreeViewItem in treeViewItem.Children)
                    {
                        foreach (LayerDefinition layerDefinition in subtreeViewItem.Layers)
                        {
                            layerDefinition.Selected = false;
                            layerDefinition.LabelOn = false;
                        }
                    }
                }

                foreach (string item in items)
                {
                    bool labelVisible = (item.Contains("L"));
                    string id = (labelVisible) ? item.Remove(item.Length - 1) : item;
                    foreach (LayerTreeViewItem treeViewItem in layerTreeview.ItemsSource)
                    {
                        foreach (LayerDefinition layerDefinition in treeViewItem.Layers)
                        {
                            if (layerDefinition.LayerID == id)
                            {
                                layerDefinition.Selected = true;
                                layerDefinition.LabelOn = labelVisible;
                                break;
                            }
                        }
                        foreach (LayerTreeViewItem subtreeViewItem in treeViewItem.Children)
                        {
                            foreach (LayerDefinition layerDefinition in subtreeViewItem.Layers)
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
        }

        private ObservableCollection<LayerTreeViewItem> processLayersIntoHierarchy(
            IEnumerable<LayerDefinition> layerDefinitions, bool useSelected)
        {
            var tvdata = new ObservableCollection<LayerTreeViewItem>();
            var currentGroup = new LayerTreeViewItem();
            var currentSubGroup = new LayerTreeViewItem();
            //initially set all selected to false for item -1.
            bool selected = false;
            bool subSelected = false;
            //Layers are ordered by GroupName, break into simple Hierarchy
            //At this stage we support a single tag or two comma seperated.
            //TODO: support full recursive set of tags
            foreach (LayerDefinition layerDefinition in layerDefinitions)
            {
                var nextGroup = (layerDefinition.Tags.Contains(","))
                                    ? layerDefinition.Tags.Split(',')[0]
                                    : layerDefinition.Tags;

                var nextSubGroup = (layerDefinition.Tags.Contains(","))
                                    ? layerDefinition.Tags.Split(',')[1]
                                    : string.Empty;

                var forceNewSubGroup = false;

                //Do we need to add a new Group?
                if (currentGroup.LayerAlias != nextGroup)
                {
                    //if the previous group is all selected we need to set the parent to selected.
                    currentGroup.Selected = selected;
                    //then we setup the change event.
                    if (!string.IsNullOrEmpty(currentGroup.LayerAlias))
                    {
                        currentGroup.PropertyChanged += currentGroup_PropertyChanged;
                    }

                    currentGroup = new LayerTreeViewItem
                    {
                        LayerAlias = nextGroup
                    };
                    tvdata.Add(currentGroup);
                    selected = false;
                    forceNewSubGroup = true;
                }

                //Do we need to add a new SubGroup?
                if (forceNewSubGroup || currentSubGroup.LayerAlias != nextSubGroup)
                {
                    //if the previous group is all selected we need to set the parent to selected.
                    currentSubGroup.Selected = subSelected;
                    //then we setup the change event.
                    if (!string.IsNullOrEmpty(currentSubGroup.LayerAlias))
                    {
                        //TODO: check this is ok
                        currentSubGroup.PropertyChanged += currentGroup_PropertyChanged;
                    }

                    currentSubGroup = new LayerTreeViewItem
                    {
                        LayerAlias = nextSubGroup,
                        IconURI = layerDefinition.IconURI,
                    };

                    //only add this if it is populated
                    if (!string.IsNullOrEmpty(currentSubGroup.LayerAlias))
                    {
                        currentGroup.Children.Add(currentSubGroup);
                    }
                    subSelected = false;
                }

                //override visibility
                if (!useSelected)
                {
                    layerDefinition.LabelOn = false;
                    layerDefinition.Selected = false;
                }

                if (string.IsNullOrEmpty(currentSubGroup.LayerAlias))
                {
                    //add the layer to the group
                    currentGroup.Layers.Add(layerDefinition);
                }
                else
                {
                    //add the layer to the sub group
                    currentSubGroup.Layers.Add(layerDefinition);
                }
                layerDefinition.PropertyChanged += definition_PropertyChanged;
                toggleLayer(layerDefinition);
                if (layerDefinition.Selected)
                {
                    //Set to true unless we are ignoring selected (False)
                    selected = useSelected;
                    subSelected = useSelected;
                }
            }

            //set the last group and change event.

            if (!string.IsNullOrEmpty(currentGroup.LayerAlias))
            {
                currentGroup.Selected = selected;
                currentGroup.PropertyChanged += currentGroup_PropertyChanged;
            }

            if (!string.IsNullOrEmpty(currentSubGroup.LayerAlias))
            {
                currentSubGroup.Selected = subSelected;
                //TODO: check this is ok
                currentSubGroup.PropertyChanged += currentGroup_PropertyChanged;
            }

            return tvdata;
        }

        private static void currentGroup_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var layerTreeViewItem = (LayerTreeViewItem)sender;
            switch (e.PropertyName)
            {
                case "Selected":
                    foreach (var layer in layerTreeViewItem.Layers)
                    {
                        layer.Selected = layerTreeViewItem.Selected;
                    }
                    foreach (var item in layerTreeViewItem.Children)
                    {
                        item.Selected = layerTreeViewItem.Selected;
                    }
                    break;
                case "LabelOn":
                    foreach (var layer in layerTreeViewItem.Layers)
                    {
                        layer.LabelOn = layerTreeViewItem.LabelOn;
                    }
                    foreach (var item in layerTreeViewItem.Children)
                    {
                        item.LabelOn = layerTreeViewItem.LabelOn;
                    }
                    break;
            }
        }

        private void definition_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var layerDefinition = (LayerDefinition)sender;
            switch (e.PropertyName)
            {
                case "Selected":
                    toggleLayer(layerDefinition);
                    RefreshPolling.resetPollingWorker(
                        (ObservableCollection<LayerTreeViewItem>)layerTreeview.ItemsSource);
                    break;
                case "LabelOn":
                    toggleLabel(layerDefinition);
                    break;
            }
        }

        private void toggleLabel(LayerDefinition layerDefinition)
        {
            switch (layerDefinition.LayerType)
            {
                case 1: //Vector Layer
                    Vector.SetLabelVisibility(layerDefinition, MapInstance);
                    break;
            }
            if (layerDefinition.LabelOn)
            {
                //Header should be ticked but not trigger event.
                selectLabelHeaderSilent(layerDefinition.LayerID);
            }
            else
            {
                checkunselectLabelHeaderSilent();
            }
        }

        private void toggleLayer(LayerDefinition layerDefinition)
        {
            if (layerDefinition.Selected)
            {
                //load layer
                LoadLayer(layerDefinition);

                //Header should be ticked but not trigger event.
                selectHeaderSilent(layerDefinition.LayerID);
            }
            else
            {
                //hide layer if loaded
                UnloadLayer(layerDefinition);

                checkunselectHeaderSilent();
            }
        }

        //If all Headers Children are now unselected need to unselect Header and not trigger event
        private void checkunselectHeaderSilent()
        {
            if (layerTreeview.ItemsSource != null)
            {
                bool found = false;
                foreach (LayerTreeViewItem header in layerTreeview.ItemsSource)
                {
                    foreach (LayerDefinition layer in header.Layers)
                    {
                        if (layer.Selected)
                        {
                            found = true;
                            break;
                        }
                    }
                    bool subfound = false;
                    foreach (LayerTreeViewItem sub in header.Children)
                    {
                        foreach (LayerDefinition layer in sub.Layers)
                        {
                            if (layer.Selected)
                            {
                                subfound = true;
                                found = true;
                                break;
                            }
                        }
                        if (!subfound)
                        {
                            sub.PropertyChanged -= currentGroup_PropertyChanged;
                            sub.Selected = false;
                            sub.PropertyChanged += currentGroup_PropertyChanged;
                        }
                        subfound = false;
                    }
                    if (!found)
                    {
                        header.PropertyChanged -= currentGroup_PropertyChanged;
                        header.Selected = false;
                        header.PropertyChanged += currentGroup_PropertyChanged;
                    }
                    found = false;
                }
            }
        }

        //Select Header without triggering event
        private void selectHeaderSilent(string ID)
        {
            if (layerTreeview.ItemsSource != null)
            {
                foreach (LayerTreeViewItem header in layerTreeview.ItemsSource)
                {
                    foreach (var layer in header.Layers)
                    {
                        if (layer.LayerID == ID)
                        {
                            header.PropertyChanged -= currentGroup_PropertyChanged;
                            header.Selected = true;
                            header.PropertyChanged += currentGroup_PropertyChanged;
                            return;
                        }
                    }
                    foreach (var sub in header.Children)
                    {
                        foreach (var layer in sub.Layers)
                        {
                            if (layer.LayerID == ID)
                            {
                                sub.PropertyChanged -= currentGroup_PropertyChanged;
                                sub.Selected = true;
                                sub.PropertyChanged += currentGroup_PropertyChanged;

                                header.PropertyChanged -= currentGroup_PropertyChanged;
                                header.Selected = true;
                                header.PropertyChanged += currentGroup_PropertyChanged;
                                return;
                            }
                        }
                    }
                }
            }
        }

        //If all Headers Children are now unselected need to unselect Header and not trigger event
        private void checkunselectLabelHeaderSilent()
        {
            if (layerTreeview.ItemsSource != null)
            {
                bool found = false;
                foreach (LayerTreeViewItem header in layerTreeview.ItemsSource)
                {
                    foreach (LayerDefinition layer in header.Layers)
                    {
                        if (layer.LabelOn)
                        {
                            found = true;
                            break;
                        }
                    }
                    bool subfound = false;
                    foreach (LayerTreeViewItem sub in header.Children)
                    {
                        foreach (LayerDefinition layer in sub.Layers)
                        {
                            if (layer.LabelOn)
                            {
                                found = true;
                                subfound = true;
                                break;
                            }
                        }
                        if (!subfound)
                        {
                            sub.PropertyChanged -= currentGroup_PropertyChanged;
                            sub.LabelOn = false;
                            sub.PropertyChanged += currentGroup_PropertyChanged;
                        }
                        subfound = false;
                    }
                    if (!found)
                    {
                        header.PropertyChanged -= currentGroup_PropertyChanged;
                        header.LabelOn = false;
                        header.PropertyChanged += currentGroup_PropertyChanged;
                    }
                    found = false;
                }
            }
        }

        //Select Header without triggering event
        private void selectLabelHeaderSilent(string ID)
        {
            if (layerTreeview.ItemsSource != null)
            {
                foreach (LayerTreeViewItem header in layerTreeview.ItemsSource)
                {
                    foreach (LayerDefinition layer in header.Layers)
                    {
                        if (layer.LayerID == ID)
                        {
                            header.PropertyChanged -= currentGroup_PropertyChanged;
                            header.LabelOn = true;
                            header.PropertyChanged += currentGroup_PropertyChanged;
                            return;
                        }
                    }
                    foreach (LayerTreeViewItem sub in header.Children)
                    {
                        foreach (LayerDefinition layer in sub.Layers)
                        {
                            if (layer.LayerID == ID)
                            {
                                sub.PropertyChanged -= currentGroup_PropertyChanged;
                                sub.LabelOn = true;
                                sub.PropertyChanged += currentGroup_PropertyChanged;

                                header.PropertyChanged -= currentGroup_PropertyChanged;
                                header.LabelOn = true;
                                header.PropertyChanged += currentGroup_PropertyChanged;
                                return;
                            }
                        }
                    }
                }
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            layerTreeview = (TreeView)GetTemplateChild(PART_LayerTreeview);
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

            IsEnabled = true;
            applyPersistedState();
        }

        private void toggleLayers_Click(object sender, RoutedEventArgs e)
        {
            if (layerTreeview.ItemsSource != null)
            {
                var LayerItems = (ObservableCollection<LayerTreeViewItem>)layerTreeview.ItemsSource;
                if (LayerItems.Count > 0)
                {
                    var visible = !LayerItems[0].Selected;
                    foreach (var item in LayerItems)
                    {
                        foreach (var layer in item.Layers)
                        {
                            layer.Selected = visible;
                        }
                        foreach (var subitem in item.Children)
                        {
                            //subitem.Selected = visible;
                            foreach (var layer in subitem.Layers)
                            {
                                layer.Selected = visible;
                            }
                        }
                    }
                }
            }
        }

        private void toggleLabels_Click(object sender, RoutedEventArgs e)
        {
            if (layerTreeview.ItemsSource != null)
            {
                var LayerItems = (ObservableCollection<LayerTreeViewItem>) layerTreeview.ItemsSource;
                if (LayerItems.Count > 0)
                {
                    bool visible = !LayerItems[0].LabelOn;
                    foreach (LayerTreeViewItem item in LayerItems)
                    {
                        foreach (LayerDefinition layer in item.Layers)
                        {
                            layer.LabelOn = visible;
                        }
                        foreach (LayerTreeViewItem subitem in item.Children)
                        {
                            //subitem.LabelOn = visible;
                            foreach (LayerDefinition layer in subitem.Layers)
                            {
                                layer.LabelOn = visible;
                            }
                        }
                    }
                }
            }
        }

        private void mouseLeave(object sender, MouseEventArgs e)
        {
            isMouseOver = false;
            if (IsEnabled)
            {
                GoToState(true);
            }
        }

        private void mouseEnter(object sender, MouseEventArgs e)
        {
            isMouseOver = true;
            if (IsEnabled)
            {
                GoToState(true);
            }
        }

        private void GoToState(bool useTransitions)
        {
            if (isMouseOver)
            {
                VisualStateManager.GoToState(this, VSM_MouseOver, useTransitions);
            }
            else
            {
                VisualStateManager.GoToState(this, VSM_Normal, useTransitions);
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

            MouseEnter -= mouseEnter;
            MouseLeave -= mouseLeave;
            base.Dispose();
        }

    }
}