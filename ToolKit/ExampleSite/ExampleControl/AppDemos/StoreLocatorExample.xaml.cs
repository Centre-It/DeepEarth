using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using Microsoft.Maps.MapControl;
using DeepEarth.BingMapsToolkit.Client.Controls;
using DeepEarth.BingMapsToolkit.Client.Common.Entities;
using DeepEarth.BingMapsToolkit.Common.Entities;
using DeepEarth.BingMapsToolkit.Client.MapGeometry;
using Point = GisSharpBlog.NetTopologySuite.Geometries.Point;

namespace ExampleControlBing.AppDemos
{
	public partial class StoreLocatorExample : UserControl
	{
		private List<Store> Stores = new List<Store>();

		public StoreLocatorExample()
		{
			InitializeComponent();
			Loaded += new RoutedEventHandler(StoreLocatorExample_Loaded);
			find.OnGetSearchLocation += find_OnGetSearchLocation;
		}

		void StoreLocatorExample_Loaded(object sender, RoutedEventArgs e)
		{
			StoresInitialization();

			var styles = new Dictionary<string, StyleSpecification>();
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

            var layers = new ObservableCollection<LayerDefinition>();
			layers.Add(new LayerDefinition
			{
				CurrentVersion = DateTime.Now,
				IsEditable = false,
				LabelOn = true,
				LayerAlias = "Retail Outlets",
				LayerID = "1",
				LayerStyleName = "style 3",
				LayerTimeout = -1,
				LayerType = 1,
				MaxDisplayLevel = 100,
				MBR = new byte[0],
				MinDisplayLevel = 1,
				PermissionToEdit = false,
				Selected = true,
				Tags = "Our Stores",
				ZIndex = 30,
				Temporal = true,
				IconURI = "http://soulsolutions.com.au/Images/pin.png",
				ObjectAttributes = new Dictionary<int, LayerObjectAttributeDefinition>()
			});

			layerPanel.Styles = styles;
			layerPanel.EnableBalloon = true;

            layerPanel.LoadLayerData += layerPanel_LoadLayer;
			layerPanel.BalloonLaunch += layerPanel_BalloonLaunch;

			layerPanel.Layers = layers;
		}

		private void StoresInitialization()
		{
			Stores.Add(new Store() { Location = new Point(13.3528, 38.095), ID = "1", Name = "Store in Palermo", Description = "Your description and photo here", Address = "Italy, Palermo" });
			Stores.Add(new Store() { Location = new Point(12.4739, 41.898), ID = "2", Name = "Store in Rome", Description = "Your description and photo here", Address = "Italy, Rome" });
			Stores.Add(new Store() { Location = new Point(14.2317, 40.8427), ID = "3", Name = "Store in Naples", Description = "Your description and photo here", Address = "Italy, Naples" });
			Stores.Add(new Store() { Location = new Point(13.9271, 42.0493), ID = "4", Name = "Store in Sulmona", Description = "Your description and photo here", Address = "Italy, Sulmona" });
			Stores.Add(new Store() { Location = new Point(9.1999, 45.5023), ID = "5", Name = "Store in Milan", Description = "Your description and photo here", Address = "Italy, Milan" });
			Stores.Add(new Store() { Location = new Point(7.7082, 45.0721), ID = "6", Name = "Store in Turin", Description = "Your description and photo here", Address = "Italy, Turin" });
		}

		private void layerPanel_BalloonLaunch(object sender, BalloonEventArgs args)
        {
            //get balloon data for item (on demand), eg make a database call here.
            var grid = new Grid { Width = 200, Height = 100 };
			var textblock = new TextBlock { Foreground = new SolidColorBrush(Colors.White)};

			var store = Stores.FirstOrDefault(u => u.ID.CompareTo((sender as InfoGeometry).ItemID) == 0);
			if (store != null)
				textblock.Text = store.Name + Environment.NewLine + store.Description;
            grid.Children.Add(textblock);
            ((InfoGeometry)sender).BalloonData.Add(grid);
        }

		private void layerPanel_LoadLayer(object sender, LoadLayerEventArgs args,
										  Action<ObservableCollection<VectorLayerData>, LayerDefinition> callback)
		{
			//get layer data for layer
			var data = new ObservableCollection<VectorLayerData>();
			foreach (var coord in Stores)
			{
				data.Add(new VectorLayerData
				{
					Geo = coord.Location.AsBinary(),
					ID = coord.ID,
					Label = coord.Name,
				});
			}
			callback(data, args.Layer);
		}

		void find_OnGetSearchLocation(object sender, DeepEarth.BingMapsToolkit.Client.Controls.FindLocationEventArgs e)
		{
			FoundLocationCollection collection = new FoundLocationCollection()
			{
				LayerTable_Name = "Italian stores",
				Locations = new List<FoundLocation>(),
			};
			var term = e.Term.ToLower();

			foreach (var store in Stores)
			{
				if ((store.Name.ToLower().Contains(term)) || (store.Address.ToLower().Contains(term)))
				{
					if (e.SearchZone == null)
					{
						AddLocationToResultCollection(collection, store);
					}
					else
					{
						var loc = new Location(){Latitude = store.Location.Y, Longitude=store.Location.X };
						if (e.SearchZone.Intersects( new LocationRect(loc, loc)))
						{
							AddLocationToResultCollection(collection, store);
						}
					}
				}
			}

			List<FoundLocationCollection> locationCollection = new List<FoundLocationCollection>();
			locationCollection.Add(collection);
			
			e.OnGetResultLocationsSearchComplete(locationCollection);
		}

		private static void AddLocationToResultCollection(FoundLocationCollection collection, Store store)
		{
			collection.Locations.Add(new FoundLocation()
			{
				Key = store.ID,
				DisplayName = store.Name,
				Latitude = store.Location.Y,
				Longitude = store.Location.X
			});
		}
	}

	class Store
	{
		public Point Location { get; set; }
		public string Address { get; set; }
		public string ID { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
	}
}
