// Deep Earth is a community project available under the Microsoft Public License (Ms-PL)
// Code is provided as is and with no warrenty – Use at your own risk
// View the project and the latest code at http://DeepEarth.codeplex.com/

using System.Collections.ObjectModel;
using System.ComponentModel;
using DeepEarth.BingMapsToolkit.Common.Entities;

namespace DeepEarth.BingMapsToolkit.Client.Controls
{
    public class LayerTreeViewItem : INotifyPropertyChanged
    {
        private string layerAlias;
        private ObservableCollection<LayerDefinition> layers;
        private ObservableCollection<LayerTreeViewItem> children;
        private bool selected;
        private bool labelOn;
        private string iconURI;

        public LayerTreeViewItem()
        {
            layers = new ObservableCollection<LayerDefinition>();
            children = new ObservableCollection<LayerTreeViewItem>();
        }

        public string LayerAlias
        {
            get { return layerAlias; }
            set
            {
                layerAlias = value;
                UpdateProperty("LayerAlias");
            }
        }

        public bool Selected
        {
            get { return selected; }
            set
            {
                selected = value;
                UpdateProperty("Selected");
            }
        }

        public bool LabelOn
        {
            get { return labelOn; }
            set
            {
                labelOn = value;
                UpdateProperty("LabelOn");
            }
        }

        public ObservableCollection<LayerDefinition> Layers
        {
            get { return layers; }
            set
            {
                layers = value;
                UpdateProperty("Layers");
            }
        }

        public ObservableCollection<LayerTreeViewItem> Children
        {
            get { return children; }
            set
            {
                children = value;
                UpdateProperty("Children");
            }
        }

        public string IconURI
        {
            get { return iconURI; }
            set 
            { 
                iconURI = value;
                UpdateProperty("IconURI");
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        public void UpdateProperty(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}