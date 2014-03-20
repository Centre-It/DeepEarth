using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Xml.Linq;

namespace DeepEarth.Client.Services.GeoRss
{
    public delegate void DownloadGeoRssCompletedEventHandler(List <GeoRssItem> items);
    public delegate void DownloadGeoRssExceptionEventHandler(Exception e);

    public class GeoRssReader
    {
        public event DownloadGeoRssCompletedEventHandler DownloadGeoRssCompleted;
        public event DownloadGeoRssExceptionEventHandler DownloadGeoRssException;

        public GeoRssReader()
        {
            wc = new WebClient();
            wc.DownloadStringCompleted += new DownloadStringCompletedEventHandler(wc_DownloadGeoRssCompleted);            
        }

        public GeoRssReader(Uri uri) : this()
        {
            this.uri = uri;            
        }

        public GeoRssReader(Uri uri, DownloadGeoRssCompletedEventHandler evh) : this(uri)
        {
            DownloadGeoRssCompleted += evh;
        }

        public void ReadAsync()
        {
            if (DownloadGeoRssCompleted.Target != null)
                wc.DownloadStringAsync(uri);
        }

        public Uri uri { get; set; }

        #region _private

        WebClient wc;

        void wc_DownloadGeoRssCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            try
            {
                XNamespace ns_xml = "http://www.w3.org/2005/Atom";
                XNamespace ns_georss = "http://www.georss.org/georss";
                XNamespace ns_geo = "http://www.w3.org/2003/01/geo/wgs84_pos#";
                XNamespace ns_media = "http://search.yahoo.com/mrss/";

                var items = from item in XElement.Parse(e.Result).Descendants("item")
                            select new GeoRssItem()
                            {
                                title = (item.Element("title") != null) ? item.Element("title").Value : null,
                                link = (item.Element("link") != null) ? item.Element("link").Value : null,
                                description = (item.Element("description") != null) ? item.Element("description").Value : null,
                                pubDate = (item.Element("pubDate") != null) ? item.Element("pubDate").Value : null,
                                guid = (item.Element("guid") != null) ? item.Element("guid").Value : null,

                                guid_isPermaLink = (item.Element("guid") != null && item.Element("guid").Attribute("isPermaLink") != null) ?
                                        (bool?)bool.Parse(item.Element("guid").Attribute("isPermaLink").Value) : null,
                                
                                georss_point = (item.Element(ns_georss + "point") != null) ? item.Element(ns_georss + "point").Value : null,
                                geo_lat = (item.Element(ns_geo + "lat") != null) ? item.Element(ns_geo + "lat").Value : null,
                                geo_long = (item.Element(ns_geo + "long") != null) ? item.Element(ns_geo + "long").Value : null,

                                media_thumbnail_url = (item.Element(ns_media + "thumbnail") != null && item.Element(ns_media + "thumbnail").Attribute("url") != null) ?
                                        item.Element(ns_media + "thumbnail").Attribute("url").Value : null,
                            };              

                if (DownloadGeoRssCompleted.Method != null)
                    DownloadGeoRssCompleted.Invoke(items.ToList());
            }
            catch (Exception ex)
            {
                if (DownloadGeoRssException.Method != null)
                    DownloadGeoRssException.Invoke(ex);
                else
                    throw ex;
            }            
        }

        #endregion
    }
}
