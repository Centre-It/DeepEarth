// Deep Earth is a community project available under the Microsoft Public License (Ms-PL)
// Code is provided as is and with no warrenty – Use at your own risk
// View the project and the latest code at http://DeepEarth.codeplex.com/

using System.Collections;
using DeepEarth.BingMapsToolkit.Client.Controls;
using System.Collections.Generic;
namespace ExampleControlBing.ControlDemos
{
    public partial class FindExample
    {
        public FindExample()
        {
            InitializeComponent();
			Loaded += FindExample_Loaded;
        }

		void FindExample_Loaded(object sender, System.Windows.RoutedEventArgs e)
		{
			find.OnGetSearchLayer += find_OnGetSearchLayer;
			find.OnGetSearchLocation += find_OnGetSearchLocation;
		}

		void find_OnGetSearchLocation(object sender, DeepEarth.BingMapsToolkit.Client.Controls.FindLocationEventArgs e)
		{
			FoundLocation loc = new FoundLocation()
			{
				Key = "Test key",
				DisplayName = "Test name",
				Latitude = 10,
				Longitude = 10
			};

			FoundLocation loc2 = new FoundLocation()
			{
				Key = "Test key2",
				DisplayName = "Test name2",
				Latitude = 20,
				Longitude = 20
			};

			FoundLocationCollection collection = new FoundLocationCollection()
			{
				LayerTable_Name = "Test table name",
				Locations = new List<FoundLocation>(),
				
			};
			collection.Locations.Add(loc);

			FoundLocationCollection collection2 = new FoundLocationCollection()
			{
				LayerTable_Name = "Test table name2",
				Locations = new List<FoundLocation>()
			};
			collection2.Locations.Add(loc2);

			List<FoundLocationCollection> locationCollection = new List<FoundLocationCollection>();
			locationCollection.Add(collection);
			locationCollection.Add(collection2);

			e.OnGetResultLocationsSearchComplete(locationCollection);
		}

		void find_OnGetSearchLayer(object sender, DeepEarth.BingMapsToolkit.Client.Controls.FindLayerEventArgs e)
		{
			List<FoundLayer> layers = new List<FoundLayer>();
			layers.Add(new FoundLayer (){
				TableName = "Test table name",
				TagsOther = "Test tags Other",
			});
			layers.Add(new FoundLayer()
			{
				TableName = "Test table name2",
				TagsOther = "Test tags Other2",
			});

			e.OnGetResultLayersSearchComplete(layers);
		}
    }
}
