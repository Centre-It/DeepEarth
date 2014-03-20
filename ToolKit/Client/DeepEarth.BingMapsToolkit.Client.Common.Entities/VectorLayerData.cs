// Deep Earth is a community project available under the Microsoft Public License (Ms-PL)
// Code is provided as is and with no warrenty – Use at your own risk
// View the project and the latest code at http://DeepEarth.codeplex.com/

using System;

namespace DeepEarth.BingMapsToolkit.Common.Entities
{
    public class VectorLayerData
    {
        public byte[] Geo { get; set; }
        public string ID { get; set; }
        public string Label { get; set; }
        public string Style { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
