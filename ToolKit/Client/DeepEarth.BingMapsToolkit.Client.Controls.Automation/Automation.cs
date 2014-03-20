// Deep Earth is a community project available under the Microsoft Public License (Ms-PL)
// Code is provided as is and with no warrenty – Use at your own risk
// View the project and the latest code at http://DeepEarth.codeplex.com/

using System.Windows.Controls;
using System.Threading;
using System.Xml.Linq;
using System.Collections.Generic;
using System;
using System.Windows;
using DeepEarth.BingMapsToolkit.Client.Common.Converters;
using GeoAPI.Geometries;
using System.Windows.Threading;
using Microsoft.Maps.MapControl;
using Point = GisSharpBlog.NetTopologySuite.Geometries.Point;
using System.Windows.Browser;
using System.Linq;
using Microsoft.Maps.MapControl.Core;
using DeepEarth.BingMapsToolkit.Client.Common;
using DeepEarth.BingMapsToolkit.Client.Common.Commanding;

namespace DeepEarth.BingMapsToolkit.Client.Controls
{
	public class Automation : Control, IMapControl<MapCore>
	{
		#region Properties

		private Dictionary<string, Action<XElement>> cmd2Action;
		private Dispatcher uiDispatcher;

		private string mapName;

		private bool isProcessing = false;
		private bool isStopped = false;
		private object locker = new object();

		#endregion

		#region Events

		public event EventHandler<GetFeatureLocationEventArgs> OnGetFeatureLocation;
		public event EventHandler<GetLayerLocationsEventArgs> OnGetLayerLocations;
		public event EventHandler OnPauseBegin;
		public event EventHandler OnPauseEnded;

		#endregion

		#region Methods 

		public Automation()
		{
			Loaded += Automation_Loaded;

			SetMatch();
		}

		private void SetMatch()
		{
			cmd2Action = new Dictionary<string, Action<XElement>>();

			cmd2Action.Add("zoomtoxy", ZoomToXY);
			cmd2Action.Add("pause", Pause);
			cmd2Action.Add("zoomtorecord", ZoomToRecord);
			cmd2Action.Add("zoomtolevel", ZoomToLevel);
			cmd2Action.Add("foreachrecordinlayer", ForEachRecordInLayer);
		}

		void Automation_Loaded(object sender, RoutedEventArgs e)
		{
			setMapInstance(MapName);
			uiDispatcher = Application.Current.RootVisual.Dispatcher;
		}

		public void Process(string mamlXml)
		{
			if (isProcessing == false)
			{
				uiDispatcher = Application.Current.RootVisual.Dispatcher;

				Thread t = new Thread(delegate()
				{
					lock (locker)
					{
						uiDispatcher.BeginInvoke(() =>
							{ WriteStatus("MAML is Starting"); });
						isProcessing = true;
					}
					Proc(mamlXml);
					lock (locker)
					{
						if (isStopped == true)
						{
						uiDispatcher.BeginInvoke(() =>
						{ WriteStatus("MAML is canceled"); WriteStatus("Waiting..."); });
						}
						else
						{
						uiDispatcher.BeginInvoke(() =>
						{ WriteStatus("MAML is ended"); WriteStatus("Waiting..."); });

						}
						isProcessing = false;
						isStopped = false;
					}
				});

				t.Start();
			}
		}

		public void Stop()
		{
			lock (locker)
			{
				if (isProcessing == true)
					isStopped = true;
			}
		}

		private void Proc(string mamlXml)
		{
            try
            {
                XDocument doc = XDocument.Parse(mamlXml);

                if (doc.Elements() != null)
                {
                    foreach (XElement node in doc.Elements().Elements())
                    {
                        ProcessNode(node);

                        if (isStopped == true)
                        {
                            break;
                        }
                    }
                }
            }
            catch {   }
		}

		private void ProcessNode(XElement node)
		{
			if (cmd2Action.ContainsKey(node.Name.LocalName.ToLower()))
				cmd2Action[node.Name.LocalName.ToLower()](node);
		}

		#region Actions

		#region ZoomToXY

		private void ZoomToXY(XElement node)
		{
			string coord = node.Attribute("value").Value;
			IPoint p = new Point(double.Parse(coord.Split(new char[] { ',' })[1]), double.Parse(coord.Split(new char[] { ',' })[0]));
			ZoomToXY(p.X, p.Y);
		}

		[ScriptableMember]
		public void ZoomToXY(double latitude, double longitude)
		{
			uiDispatcher.BeginInvoke(() =>
			{
				OnGetPointComplete(latitude, longitude);
			});
		}

		private void OnGetPointComplete(double latitude, double longitude)
		{
			MapInstance.SetView(new Microsoft.Maps.MapControl.Location(latitude, longitude), 18);
		}

		#endregion

		#region ZoomToLevel

		private void ZoomToLevel(XElement node)
		{
			double level = double.Parse(node.Attribute("level").Value);
			ZoomToLevel(level);
		}

		[ScriptableMember]
		public void ZoomToLevel(double level)
		{
			uiDispatcher.BeginInvoke(() =>
			{
				MapInstance.ZoomLevel = level;
			});
		}
		
		#endregion

		#region ZoomToRecord

		private void ZoomToRecord(XElement node)
		{
			uiDispatcher.BeginInvoke(() =>
			{
				string layerID = node.Attribute("layerid").Value;
				string itemID = node.Attribute("itemid").Value;
				ZoomToRecord(layerID, itemID);
			});
		}

		[ScriptableMember]
		public void ZoomToRecord(string layerID, string itemID)
		{
			uiDispatcher.BeginInvoke(() =>
			{
				if (OnGetFeatureLocation != null)
				{
					OnGetFeatureLocation(this, new GetFeatureLocationEventArgs(layerID, itemID, OnGetFeatureLocationComplete));
				}
			});
		}

		public void OnGetFeatureLocationComplete(LocationRect rect)
		{
			MapInstance.SetView(rect);
		}

		#endregion

		#region Pause

		private void Pause(XElement node)
		{
			Pause(int.Parse(node.Attribute("time").Value));
		}

		[ScriptableMember]
		public void Pause(int duration)
		{
			if (OnPauseBegin != null)
				OnPauseBegin(this, new EventArgs());
			Thread.Sleep(duration);
			if (OnPauseEnded != null)
				OnPauseEnded(this, new EventArgs());
		}

		#endregion

		#region ForEachRecordInLayer

		private void ForEachRecordInLayer(XElement node)
		{
			ForEachRecordInLayer(node.Attribute("layerid").Value, node.ToString());
		}

		[ScriptableMember]
		public void ForEachRecordInLayer(string layerId, string xml)
		{
			var node = XElement.Parse(xml);
			
			if (OnGetLayerLocations != null)
			{
				OnGetLayerLocations(this, new GetLayerLocationsEventArgs(layerId, u => { OnGetLayerComplete(layerId, u, node); }));
			}
		}

		public void OnGetLayerComplete(string layerId, List<string> features, XElement node)
		{
			var xmlNodes = node.Elements();

			foreach (var record in features)
			{
				foreach (var cmd in xmlNodes)
				{
					if (cmd.Name.LocalName.ToLower().CompareTo("zoomtorecord") == 0)
					{
						if (!cmd.Attributes().Any(u => u.Name.LocalName.CompareTo("itemid") == 0))
						{
							cmd.SetAttributeValue("itemid", "[item]");
						}
						if (!cmd.Attributes().Any(u => u.Name.LocalName.CompareTo("layerid") == 0))
						{
							cmd.SetAttributeValue("layerid", layerId.ToString());
						}
					}
					ProcessNode(XElement.Parse(cmd.ToString().Replace("[item]", record)));
					if (isStopped == true)
					{
						break;
					}
				}
				if (isStopped == true)
				{
					break;
				}
			}
		}

		#endregion

		#endregion

		private void WriteStatus(string status)
		{
			Command cmd = StatusCommands.ReportStatusCommand;
			if (cmd != null)
				cmd.Execute(status);
		}

		#region IMapControl<MapCore> Members

		public string MapName
		{
			get { return mapName; }
			set
			{
				mapName = value;
				setMapInstance(MapName);
			}
		}

		public MapCore MapInstance { get; set; }

		private void setMapInstance(string mapname)
		{
			MapInstance = Utilities.FindVisualChildByName<Map>(Application.Current.RootVisual, mapname);
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			
		}

		#endregion

		#endregion
	}
}
