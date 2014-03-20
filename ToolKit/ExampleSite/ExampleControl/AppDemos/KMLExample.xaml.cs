using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Resources;
using System.Xml.Linq;
using DeepEarth.BingMapsToolkit.Client.Common;
using DeepEarth.BingMapsToolkit.Client.Common.Entities;
using DeepEarth.BingMapsToolkit.Client.Controls;
using DeepEarth.BingMapsToolkit.Common.Entities;
using Point=GisSharpBlog.NetTopologySuite.Geometries.Point;

namespace ExampleControlBing.AppDemos
{
    public partial class KMLExample
    {
        private readonly Dictionary<string, List<double>> Points = new Dictionary<string, List<double>>();

        public KMLExample()
        {
            InitializeComponent();
            Loaded += KMLExample_Loaded;
        }

        private void KMLExample_Loaded(object sender, RoutedEventArgs e)
        {
            layerPanel.LoadLayerData += layerPanel_LoadLayer;

            //Reading KML file
            StreamResourceInfo sri =
                Application.GetResourceStream(new Uri("/ExampleControlBing;component/AppDemos/KMLExample.kml",
                                                      UriKind.Relative));
            string kml;
            using (TextReader tr = new StreamReader(sri.Stream))
            {
                kml = tr.ReadToEnd();
            }

            layerPanel.Styles = GetStyles(kml);
            layerPanel.Layers = GetLayers(kml);
        }

        private ObservableCollection<LayerDefinition> GetLayers(string kml)
        {
            XDocument doc = XDocument.Parse(kml);

            int i = 0;
            var layerDefinitions = new ObservableCollection<LayerDefinition>();
            foreach (XElement elem in doc.Descendants("Point"))
            {
                string key = "point" + i;
                layerDefinitions.Add(new LayerDefinition
                                         {
                                             LayerStyleName =
                                                 elem.Parent.Element("styleUrl") != null
                                                     ? elem.Parent.Element("styleUrl").Value.Substring(1)
                                                     : "defaultstyle",
                                             MaxDisplayLevel = 100,
                                             MBR = new byte[0],
                                             MinDisplayLevel = 1,
                                             ZIndex = 30,
                                             Temporal = true,
                                             ObjectAttributes = new Dictionary<int, LayerObjectAttributeDefinition>(),
                                             LayerType = 1,
                                             LabelOn = true,
                                             Selected = true,
                                             LayerID = key,
                                             LayerAlias = key,
                                             Tags = "Points",
                                             IconURI =
                                                 ((elem.Parent.Element("styleUrl") != null) &&
                                                  (layerPanel.Styles.ContainsKey(
                                                      elem.Parent.Element("styleUrl").Value.Substring(1))))
                                                     ? layerPanel.Styles[
                                                           elem.Parent.Element("styleUrl").Value.Substring(1)].IconURL
                                                     : "defaultstyle",
                                         });

                if (Points.ContainsKey(key))
                    Points.Remove(key);

                Points.Add(key, new List<double>());

                if (elem.Descendants("coordinates") != null)
                    Points[key].AddRange(
                        (elem.Descendants("coordinates").FirstOrDefault().Value.Split(',').Select(u => double.Parse(u))));
                ++i;
            }

            return layerDefinitions;
        }

        private static Dictionary<string, StyleSpecification> GetStyles(string kml)
        {
            Dictionary<string, StyleSpecification> styles = Utilities.ProcessStyleXML(kml);

            styles.Add("defaultstyle", new StyleSpecification
                                           {
                                               ID = "style 1",
                                               LineColour = "FF1B0AA5",
                                               LineWidth = 2,
                                               PolyFillColour = "88677E1E",
                                               ShowFill = true,
                                               ShowLine = true,
                                               IconURL = "http://soulsolutions.com.au/Images/pin.png",
                                               IconScale = 2
                                           });
            return styles;
        }

        private void layerPanel_LoadLayer(object sender, LoadLayerEventArgs args,
                                          Action<ObservableCollection<VectorLayerData>, LayerDefinition> callback)
        {
            var data = new ObservableCollection<VectorLayerData>();

            //Processing point
            if (args.Layer.LayerID.IndexOf("point") == 0)
            {
                var point = new Point(Points[args.Layer.LayerID][0], Points[args.Layer.LayerID][1]);
                data.Add(new VectorLayerData
                             {
                                 Geo = point.AsBinary(),
                                 ID =
                                     args.Layer.LayerID + Points[args.Layer.LayerID][0] + ',' +
                                     Points[args.Layer.LayerID][1],
                                 //TimeStamp = DateTime.Now.AddHours(x),
                             });
            }
            callback(data, args.Layer);
        }
    }
}