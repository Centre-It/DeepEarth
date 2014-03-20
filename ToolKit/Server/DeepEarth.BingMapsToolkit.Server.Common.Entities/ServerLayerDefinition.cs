// Deep Earth is a community project available under the Microsoft Public License (Ms-PL)
// Code is provided as is and with no warrenty – Use at your own risk
// View the project and the latest code at http://DeepEarth.codeplex.com/

namespace DeepEarth.BingMapsToolkit.Common.Entities
{
    public partial class LayerDefinition
    {
        //not exposed to the client
        public string LayerName { get; set; }
        public bool ShowInLayerControlByDefault { get; set; }
        public string LayerDescription { get; set; }
        public string LayerConnection { get; set; }
        public string ObjectURI { get; set; }
        public string SearchURI { get; set; }
        public bool ObjectHasStyle { get; set; }
        public bool ObjectHasLabel { get; set; }

        public string LayerDataURI { get; set; }
        public string ReplayVectorURI { get; set; }
        public string ReplayDataURI { get; set; }

    }
}
