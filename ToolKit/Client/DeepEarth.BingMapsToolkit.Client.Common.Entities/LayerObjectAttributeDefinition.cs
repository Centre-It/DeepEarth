namespace DeepEarth.BingMapsToolkit.Common.Entities
{
 	public enum AttributeType
	{
		KeyValuePair = 0,
		Chart = 1,
		Gauge = 2
	}
	
	public class LayerObjectAttributeDefinition
    {
        public string ObjectAttributeID { get; set; }
        public string ObjectAttributeTemplate { get; set; }
		public AttributeType ObjectAttributeType { get; set; }

#if SILVERLIGHT
        
#else
        //not public
        public string ObjectAttributeURI { get; set; }
		public string Description { get; set; }
		public string ConnectionString { get; set; }
		public int DisplayOrder { get; set; }
#endif

    }
}
