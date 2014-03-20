using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace DeepEarth.Client.Services.GeoRss
{
    /*
     * The following is the supported format. These strings and attributes will 
     * be parsed into a GeoRssItem, if they are present:
     * 
     * <item>
     *      <title>
     *      <link>
     *      <description>
     *      <pubDate>
     *      <guid isPermaLink="BOOL">
     *      <georss:point>
     *      <geo:lat>
     *      <geo:long>
     *      <media:thumbnail url="URL">
     * </item>
     */

    public class GeoRssItem
    {
        public string title;
        public string link;
        public string description;
        public string pubDate;
        public string guid;
        public bool?  guid_isPermaLink;      
        public string georss_point;
        public string geo_lat;
        public string geo_long;
        public string media_thumbnail_url;

        public Point point
        {
            get
            {
                return new Point(Longitude, Latitude);
            }
        }

        public double Latitude
        {
            get
            {
                if (geo_lat == null && georss_point != null)
                    geo_lat = (georss_point.Split(new char[] { ' ' }))[0];

                return double.Parse(geo_lat.Replace('.', ','));
            }
        }

        public double Longitude
        {
            get
            {
                if (geo_long == null && georss_point != null)
                    geo_long = (georss_point.Split(new char[] { ' ' }))[1];

                return double.Parse(geo_long.Replace('.', ','));
            }
        }
    }
}
