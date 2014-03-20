// Deep Earth is a community project available under the Microsoft Public License (Ms-PL)
// Code is provided as is and with no warrenty – Use at your own risk
// View the project and the latest code at http://DeepEarth.codeplex.com/

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DeepEarth.BingMapsToolkit.Client.Controls.GeocodeService;
using Microsoft.Maps.MapControl;
using Microsoft.Maps.MapControl.Core;

namespace DeepEarth.BingMapsToolkit.Client.Controls
{
	public class GeocodeLayer
	{
		private const int side = 5;
		private readonly MapLayer layer;
		private readonly MapCore myMap;

		public GeocodeLayer(MapCore _myMap)
		{
			myMap = _myMap;
			layer = new MapLayer();
			myMap.Children.Add(layer);
		}

		private static string ExtractNumber(Address address)
		{
			// Get street address ... works for most US addresses.
			int blankAt = address.AddressLine.IndexOf(' ');
			if (blankAt > 0)
			{
				return address.AddressLine.Substring(0, blankAt);
			}
			return string.Empty;
		}

		// Point inside box that is approximately (2*side) meters per side

		private LocationRect BoxIt(Location loc)
		{
			Point pt = myMap.LocationToViewportPoint(loc);
			double mpp = MercatorUtility.ZoomToScale(new Size(512, 512), myMap.ZoomLevel, loc);
			double offset = side / mpp;
			pt.X -= offset;
			pt.Y -= offset;
			Location nw = myMap.ViewportPointToLocation(pt);
			pt.X += (2 * offset);
			pt.Y += (2 * offset);
			Location se = myMap.ViewportPointToLocation(pt);
			var rect = new LocationRect(
				new Location(nw.Latitude, se.Longitude),
				new Location(se.Latitude, nw.Longitude));
			return rect;
		}

		public Location AddResult(GeocodeResult result)
		{
			MapShapeBase shape;
			var loc = new Location(result.Locations[0].Latitude, result.Locations[0].Longitude);
			LocationRect rect = BoxIt(loc);
			if (0 == string.CompareOrdinal(result.Locations[0].CalculationMethod, "Interpolation"))
			{
				// Draw plus sign at interpolated point.
				var polyline = new MapPolyline();
				double midLat = (rect.Southeast.Latitude + rect.Northwest.Latitude) / 2;
				double midLon = (rect.Southeast.Longitude + rect.Northwest.Longitude) / 2;
				var center = new Location(midLat, midLon);
				polyline.Locations = new LocationCollection
                                         {
                                             center,
                                             new Location(rect.Northwest.Latitude, midLon),
                                             center,
                                             new Location(midLat, rect.Northwest.Longitude),
                                             center,
                                             new Location(rect.Southeast.Latitude, midLon),
                                             center,
                                             new Location(midLat, rect.Southeast.Longitude),
                                             center
                                         };
				polyline.Stroke = new SolidColorBrush(Colors.Red);
				polyline.StrokeThickness = 1.0;
				polyline.StrokeEndLineCap = PenLineCap.Round;
				shape = polyline;
			}
			else
			{
				// Parcel or rooftop... show colored box.
				var polygon = new MapPolygon();
				if (0 == string.CompareOrdinal(result.Locations[0].CalculationMethod, "Parcel"))
				{
					// Blue box for Parcel geocode.
					polygon.Stroke = new SolidColorBrush(Colors.White);
					polygon.Fill = new SolidColorBrush(Colors.Blue);
				}
				else
				{
					// Red box for rooftop geocode.
					polygon.Stroke = new SolidColorBrush(Colors.White);
					polygon.Fill = new SolidColorBrush(Colors.Red);
				}
				polygon.StrokeThickness = 1.0;
				polygon.Opacity = .6;
				polygon.Locations = new LocationCollection
                                        {
                                            rect.Northwest,
                                            rect.Northeast,
                                            rect.Southeast,
                                            rect.Southwest
                                        };
				shape = polygon;
			}

			// Store address with shape in
			shape.Tag = result.DisplayName;
			// Add a tool tip to display the full address.
			ToolTipService.SetToolTip(shape, result.DisplayName);
			// Add shape to the layer
			layer.Children.Add(shape);

			// Put the street address on the map.
			string addressNumber = ExtractNumber(result.Address);
			if (!string.IsNullOrEmpty(addressNumber))
			{
				var tb = new TextBlock
							 {
								 Text = addressNumber,
								 FontFamily = new FontFamily("Times New Roman"),
								 FontSize = 12.0,
								 HorizontalAlignment = HorizontalAlignment.Left,
								 VerticalAlignment = VerticalAlignment.Center,
								 Foreground = new SolidColorBrush(Colors.White)
							 };
				// Set tool tip on text since it's above the shape.
				ToolTipService.SetToolTip(tb, result.DisplayName);
				tb.Tag = result.DisplayName;
				layer.AddChild(tb, loc, PositionOrigin.Center);
			}

			return loc;
		}

		public Location AddResult(double latitude, double longitude, string displayName)
		{
			MapShapeBase shape;
			var loc = new Location(latitude, longitude);
			LocationRect rect = BoxIt(loc);

			// Parcel or rooftop... show colored box.
			var polygon = new MapPolygon();

			polygon.Stroke = new SolidColorBrush(Colors.White);
			polygon.Fill = new SolidColorBrush(Colors.Red);

			polygon.StrokeThickness = 1.0;
			polygon.Opacity = .6;
			polygon.Locations = new LocationCollection
                                        {
                                            rect.Northwest,
                                            rect.Northeast,
                                            rect.Southeast,
                                            rect.Southwest
                                        };
			shape = polygon;

			// Store address with shape in
			shape.Tag = displayName;
			// Add a tool tip to display the full address.
			ToolTipService.SetToolTip(shape, displayName);
			// Add shape to the layer
			layer.Children.Add(shape);

			// Put the street address on the map.
			return loc;
		}

		public void Clear()
		{
			layer.Children.Clear();
		}
	}
}