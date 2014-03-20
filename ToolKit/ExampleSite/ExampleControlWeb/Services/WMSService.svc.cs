using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Net;
using System.IO;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.ServiceModel.Activation;

namespace ExampleControlBingWeb.Services
{
	[ServiceContract]
	[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
	public class WMSService
	{
		private const string getFeatureInfoRequest = "GetFeatureInfo";
		private const string getCapabilitiesRequest = "GetCapabilities";

		private const int defaultWidth = 256;
		private const int defaultHeight = 256;
		private const int defaultFeatureCount = 512;

		List<string> WMSServices = new List<string>();
		Dictionary<string, IEnumerable<string>> ServiceLayers = new Dictionary<string, IEnumerable<string>>();
		Dictionary<string, ManualResetEvent> getCapabilitiesProcess = new Dictionary<string, ManualResetEvent>();
		Dictionary<string, ManualResetEvent> getFeatureProcess = new Dictionary<string, ManualResetEvent>();

        List<List<WMSFeature>> Features = new List<List<WMSFeature>>();

		[OperationContract]
        public List<WMSFeature> WMSFeatureRequest(
			double minx,
			double maxx,
			double miny,
			double maxy,
			double x,
			double y,
			double width,
			double height)
		{
            //Add your service here...
            WMSServices.Add(@"http://www2.demis.nl/wms/wms.asp?wms=worldmap&VERSION=1.1.1&SERVICE=WMS&request=GetCapabilities");
          
			foreach (string service in WMSServices)
			{
				getCapabilitiesProcess.Add(service, new ManualResetEvent(false));
				GetCapabilities(service);
			}

			WaitHandle.WaitAll(getCapabilitiesProcess.Values.ToArray());

			foreach (string service in ServiceLayers.Keys)
			{
				getFeatureProcess.Add(service, new ManualResetEvent(false));
				GetFeatute(minx, maxx, miny, maxy, x, y, width, height, service);
			}

			WaitHandle.WaitAll(getFeatureProcess.Values.ToArray());

            List<WMSFeature> InterResult = Features.SelectMany(u => u).ToList();
            InterResult.Sort(delegate(WMSFeature u, WMSFeature v) { return u.Layer.CompareTo(v.Layer); });

            return InterResult;
		}

		private void GetFeatute(double minx, double maxx, double miny, double maxy, double x, double y, double width, double height, string WMSServiceUrl)
		{
			string bbox = minx.ToString() + ',' + miny.ToString() + ',' + maxx.ToString() + ',' + maxy.ToString();

			var url = WMSServiceUrl;

			string srs = "EPSG:4326";

			url = AddQueryParameter(url, "X", x.ToString());
			url = AddQueryParameter(url, "Y", y.ToString());
			url = AddQueryParameter(url, "SRS", srs);
			url = AddQueryParameter(url, "REQUEST", getFeatureInfoRequest);
			url = AddQueryParameter(url, "BBOX", bbox);
			url = AddQueryParameter(url, "QUERY_LAYERS", String.Join(",", ServiceLayers[WMSServiceUrl].ToArray()));
            url = AddQueryParameter(url, "LAYERS", String.Join(",", ServiceLayers[WMSServiceUrl].ToArray()));
			url = AddQueryParameter(url, "FEATURE_COUNT", defaultFeatureCount.ToString());
			url = AddQueryParameter(url, "WIDTH", width.ToString());
			url = AddQueryParameter(url, "HEIGHT", height.ToString());

			WebClient infoClient = new WebClient();
            List<WMSFeature> featureList = new List<WMSFeature>();
			lock (Features)
			{
				Features.Add(featureList);
			}

			infoClient.OpenReadCompleted += (s, e) =>
			{
				try
				{
                    if (e.Error == null)
                    {
                        ProcessFeatureResult(e.Result, featureList, WMSServiceUrl);
                    }
				}
				finally
				{
					getFeatureProcess[WMSServiceUrl].Set();
				}
			};
			Uri uri = new Uri(url, UriKind.Absolute);
			infoClient.OpenReadAsync(uri);
		}

        private void ProcessFeatureResult(Stream e, List<WMSFeature> result, string service)
		{
			using (TextReader reader = new StreamReader(e))
			{
				string html = reader.ReadToEnd();
				var matchestrtd = Regex.Matches(html, "<tr(.*?)>(.*?)<td(.*?)>(.*?)</tr>", RegexOptions.IgnoreCase);
				foreach (Match match in matchestrtd)
				{
					var matchesth = Regex.Matches(match.Value, "<td(.*?)>(.*?)</td>", RegexOptions.IgnoreCase);
                    result.Add(new WMSFeature()
					{
						ID = matchesth[0].Groups[2].Value,
						Layer = matchesth[1].Groups[2].Value,
						Description = matchesth[2].Groups[2].Value,
						Value = matchesth[3].Groups[2].Value
					});
				}
			}
		}

		private void GetCapabilities(string WMSServiceUrl)
		{
			WebClient client = new WebClient();

			client.OpenReadCompleted += (s, e) =>
			{
				try
				{
					if (e.Error == null)
					{
						ProcessGetCapabilitieResult(e.Result, WMSServiceUrl);
					}
				}
				finally
				{
					getCapabilitiesProcess[WMSServiceUrl].Set();
				}
			};

			System.Uri uri = new Uri(AddQueryParameter(WMSServiceUrl, "REQUEST", getCapabilitiesRequest), UriKind.Absolute);

			client.OpenReadAsync(uri);
		}

		private void ProcessGetCapabilitieResult(Stream e, string service)
		{
			using (TextReader reader = new StreamReader(e))
			{
				string xml = reader.ReadToEnd();
				XDocument doc = XDocument.Parse(xml);
				XElement capabilityElement = doc.Descendants("Capability").First();
				if (capabilityElement.Element("Request").Element(getFeatureInfoRequest) != null)
				{
					XElement layersElement = capabilityElement.Element("Layer");
					var layers = layersElement.Elements("Layer");
					var queryableLayers = layers.Where(u => u.Attribute("queryable").Value.CompareTo("1") == 0);
					var layerNames = queryableLayers.Select(u => u.Element("Name").Value);
					ServiceLayers.Add(service, layerNames);
				}
			}
		}

		private string AddQueryParameter(string currentUrl, string name, string value)
		{
			if (currentUrl.Contains('?'))
			{
				if (currentUrl.EndsWith("?"))
				{
					return currentUrl + name + "=" + value;
				}
				else
				{
					bool finded = false;
					string query = currentUrl.Substring(currentUrl.IndexOf('?') + 1);
					string url = currentUrl.Substring(0, currentUrl.IndexOf('?') + 1);

					foreach (var pair in query.Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries))
					{
						var splittedPair = pair.Split(new char[] { '=' });
						var key = splittedPair[0];
						var oldValue = splittedPair[1];

						if (key.ToLower() == name.ToLower())
						{
							url += key + "=" + value + "&";
							finded = true;
						}
						else
						{
							url += key + "=" + oldValue + "&";
						}
					}

					if (!finded)
						url = url + name + "=" + value;

					return url;
				}
			}
			else
			{
				return currentUrl + "?" + name + "=" + value;
			}
		}
	}

}