namespace DeepEarth.Client.Common.Entities
{
    public class LayerObjectAttributeDefinition
    {
        public string ObjectAttributeID { get; set; }
        public string ObjectAttributeTemplate { get; set; }

#if SILVERLIGHT
        
#else
        //not public
        public string ObjectAttributeURI { get; set; }
#endif

    }
}
