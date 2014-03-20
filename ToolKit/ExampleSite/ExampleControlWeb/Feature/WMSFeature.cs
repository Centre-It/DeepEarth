using System;
using System.Runtime.Serialization;

[DataContract]
public class WMSFeature
{
    [DataMember]
    public string Layer { get; set; }

    [DataMember]
    public string ID { get; set; }

    [DataMember]
    public string Description { get; set; }

    [DataMember]
    public string Value { get; set; }
}
