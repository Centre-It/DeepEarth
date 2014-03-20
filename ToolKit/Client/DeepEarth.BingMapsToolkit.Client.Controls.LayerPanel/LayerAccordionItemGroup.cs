using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Collections.ObjectModel;
using DeepEarth.BingMapsToolkit.Common.Entities;
using System.Collections.Generic;

namespace DeepEarth.BingMapsToolkit.Client.Controls
{
	public class LayerAccordionItemGroup : INotifyPropertyChanged
	{
		private string name;
		private List<LayerDefinition> layers;
        private bool selected;

		public LayerAccordionItemGroup()
        {
			Layers = new List<LayerDefinition>();
        }

		public string Name
        {
			get { return name; }
            set
            {
				name = value;
				UpdateProperty("Name");
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

		public List<LayerDefinition> Layers
        {
			get { return layers; }
            set
            {
				layers = value;
				UpdateProperty("Layers");
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
