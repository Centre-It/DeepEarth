using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace DeepEarth.Client.Common.Entities
{
    public class LayerDefinition : INotifyPropertyChanged
    {
        private DateTime currentVersion;
        private bool isEditable;
        private bool labelOn;
        private string layerAlias;
        private string layerId;
        private string layerStyleName;
        private int layerTimeout;
        private int layerType;
        private int maxDisplayLevel;
        private byte[] mbr;
        private int minDisplayLevel;
        private bool permissionToEdit;
        private bool selected;
        private string tags;
        private int zIndex;

        public LayerDefinition()
        {
            ObjectAttributes = new Dictionary<int, LayerObjectAttributeDefinition>();
        }

        public string LayerID
        {
            get { return layerId; }
            set
            {
                layerId = value;
                UpdateProperty("LayerID");
            }
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

        public string Tags
        {
            get { return tags; }
            set
            {
                tags = value;
                UpdateProperty("Tags");
            }
        }

        public int LayerType
        {
            get { return layerType; }
            set
            {
                layerType = value;
                UpdateProperty("LayerType");
            }
        }

        public bool IsEditable
        {
            get { return isEditable; }
            set
            {
                isEditable = value;
                UpdateProperty("IsEditable");
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

        public DateTime CurrentVersion
        {
            get { return currentVersion; }
            set
            {
                currentVersion = value;
                UpdateProperty("CurrentVersion");
            }
        }

        public int MinDisplayLevel
        {
            get { return minDisplayLevel; }
            set
            {
                minDisplayLevel = value;
                UpdateProperty("MinDisplayLevel");
            }
        }

        public int MaxDisplayLevel
        {
            get { return maxDisplayLevel; }
            set
            {
                maxDisplayLevel = value;
                UpdateProperty("MaxDisplayLevel");
            }
        }

        public byte[] MBR
        {
            get { return mbr; }
            set
            {
                mbr = value;
                UpdateProperty("MBR");
            }
        }

        public string LayerStyleName
        {
            get { return layerStyleName; }
            set
            {
                layerStyleName = value;
                UpdateProperty("LayerStyleName");
            }
        }

        public int LayerTimeout
        {
            get { return layerTimeout; }
            set
            {
                layerTimeout = value;
                UpdateProperty("LayerTimeout");
            }
        }

        public int ZIndex
        {
            get { return zIndex; }
            set
            {
                zIndex = value;
                UpdateProperty("ZIndex");
            }
        }

        public bool PermissionToEdit
        {
            get { return permissionToEdit; }
            set
            {
                permissionToEdit = value;
                UpdateProperty("PermissionToEdit");
            }
        }

        public Dictionary<int, LayerObjectAttributeDefinition> ObjectAttributes { get; set; }

#if SILVERLIGHT
        

#else
                
        //not exposed to the client
        public string LayerName { get; set; }
        public bool ShowInLayerControlByDefault { get; set; }
        public string LayerDescription { get; set; }
        public string LayerConnection { get; set; }
        public string LayerURI { get; set; }
        public string ObjectURI { get; set; }
        public string SearchURI { get; set; }
        public bool ObjectHasStyle { get; set; }
        public bool ObjectHasLabel { get; set; }

#endif



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
