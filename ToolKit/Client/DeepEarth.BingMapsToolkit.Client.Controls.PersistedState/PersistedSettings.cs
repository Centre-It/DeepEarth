using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DeepEarth.BingMapsToolkit.Client.Controls
{
    [DataContract]
    public class PersistedSettings
    {
        [DataMember]
        public double Longitude { get; set; }

        [DataMember]
        public double Latitude { get; set; }

        [DataMember]
        public double Altitude { get; set; }

        [DataMember]
        public double ZoomLevel { get; set; }

        [DataMember]
        public string Mode { get; set; }

        [DataMember]
        public Dictionary<string, string> CustomSettings { get; set; }

        public PersistedSettings()
        {
            CustomSettings = new Dictionary<string, string>();
        }
    }
}
