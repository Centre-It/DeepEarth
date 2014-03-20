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
using DeepEarth.Client.MapControl;
using DeepEarth.Client.Services.NearMap;
using System.IO;
using System.Windows.Browser;
using DeepEarth.Client.MapControl.Layers;

namespace DeepEarth.Client.Controls.DEMap
{
	[TemplatePart(Name = PART_datesList, Type = typeof(ListBox))]
	[TemplatePart(Name = PART_showing, Type = typeof(TextBlock))]
	[TemplatePart(Name = PART_sliderCanvas, Type = typeof(Canvas))]
	public class DateLine : Control, IMapControl<Map>
	{
		private const string PART_datesList = "PART_datesList";
		private const string PART_showing = "PART_showing";
		private const string PART_sliderCanvas = "PART_sliderCanvas";

		private List<Ellipse> marks = new List<Ellipse>();

		private TextBlock showing;
		private ComboBox datesList;
		private Canvas sliderCanvas;

		private Map mapInstance;
		private string mapName;

		private DateTime? currentDate;

		private bool isProcessChanged = true;

		private const string datesUrl = @"http://www.nearmap.com/maps/x={0}&y={1}&z={2}&nmq=INFO&nmf=json&nmjsonp=jsonp1267176209643";

		private const double defaultEllipsMarkHeight = 13;
		private const double defaultEllipsMarkWidth = 13;

		HtmlElement oldScript;

		public DateLine()
		{
			this.DefaultStyleKey = typeof(DateLine);
			Loaded += DateLine_Loaded;
		}

		void DateLine_Loaded(object sender, RoutedEventArgs e)
		{
			setMapInstance(MapName);
			GenerateJavascript();
		}

		private void GenerateJavascript()
		{
			//genereta javascript function for transferring from javascript to silverlight
			HtmlPage.RegisterScriptableObject("DateLine", this);

			HtmlElement script = HtmlPage.Document.CreateElement("script");
			script.SetAttribute("Language", "javascript");
			script.SetProperty("text",
@"function jsonp1267176209643(s){
	document.getElementById('exampleControl').Content.DateLine.ParseDates(s.layers.Vert.join());
}");
			HtmlPage.Document.Body.AppendChild(script);
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			datesList = (ComboBox)GetTemplateChild(PART_datesList);
			showing = (TextBlock)GetTemplateChild(PART_showing);
			sliderCanvas = (Canvas)GetTemplateChild(PART_sliderCanvas);

			if (datesList != null)
			{
				datesList.SelectionChanged += datesList_SelectionChanged;

				FillDatesList();
				if (datesList.Items.Count > 0)
				{
					datesList.SelectedIndex = 0;
				}
			}
		}

		private void FillDatesList()
		{
			//deleting old script and generating new
			var center = mapInstance.GeoCenter;
			if (oldScript != null)
			{
				HtmlPage.Document.Body.RemoveChild(oldScript);
			}
			if (!double.IsNaN(center.X) && !double.IsNaN(center.Y))
			{
				HtmlElement script = HtmlPage.Document.CreateElement("script");
				oldScript = script;
				var centerInPixels = mapInstance.CoordHelper.GeoToPixelAtZoom(center, Convert.ToInt32(mapInstance.ZoomLevel));
				//getting center tile and request
				script.SetAttribute("src", String.Format(datesUrl, Convert.ToInt64(centerInPixels.X / 256), Convert.ToInt64(centerInPixels.Y / 256), Math.Round(mapInstance.ZoomLevel)));
				script.SetAttribute("Language", "javascript");
				HtmlPage.Document.Body.AppendChild(script);
			}
		}

		void datesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (isProcessChanged)
			{
				if (datesList.SelectedItem == null)
					SetNewNearMapSource(null);
				else
					SetNewNearMapSource((DateTime)datesList.SelectedItem);
			}
			RefreshSlider();
		}

		private NearMapTileSource NewNearMapTileSource()
		{
			NearMapTileSource newSource = new NearMapTileSource(NearMapModes.NearMap);
			newSource.DateMap = CurrentShortDate();

			return newSource;
		}

		[ScriptableMember]
		public void ParseDates(string data)
		{
			List<DateTime> result = new List<DateTime>();
			DateTime basicDate = new DateTime(1970, 1, 1);

			//getting milliseconds
			data = data.Replace("Date", "").Replace("(", "").Replace(")", "").Replace("/", "");
			var milliseconds = data.Split(new char[] { ',' }).Select(u => double.Parse(u));

			//calculation dates
			foreach (double item in milliseconds)
			{
				result.Add(basicDate.AddMilliseconds(item));
			}

			result.Reverse();

			//finding nearest date if current date is absent in dateslist, then use it
			if (datesList.SelectedItem != null)
			{
				var date = (DateTime)datesList.SelectedItem;
				if (!result.Contains(date))
				{

					if ((currentDate != null) && (result.Count > 0))
					{
						DateTime minDate = result[0];

						int cmp = Math.Abs(minDate.CompareTo(date));


						foreach (var d in result)
						{
							if (cmp > Math.Abs(d.CompareTo(date)))
								minDate = d;
						}
						SetNewNearMapSource(minDate);
					}
					result.Insert(0, date);
				}

				isProcessChanged = false;
				Dates2Marks(result);
				datesList.ItemsSource = result;
				datesList.SelectedItem = date;
				isProcessChanged = true;
			}
			else
			{
				datesList.ItemsSource = result;
				if (result.Count > 0)
				{
					datesList.SelectedIndex = 0;
				}
			}
		}

		private void SetNewNearMapSource(DateTime? date)
		{
			if ((mapInstance != null) && (currentDate != date))
			{
				NearMapTileSource source = mapInstance.BaseLayer.Source as NearMapTileSource;

				if (source != null)
				{
					mapInstance.BaseLayer.Source = NewNearMapTileSource();
				}

				showing.Text = String.Format("Showing {0}", date == null ? " " : date.Value.ToShortDateString());
				currentDate = date;
			}
		}

		#region IMapControl<Map> Members

		public string MapName
		{
			get { return mapName; }
			set
			{
				mapName = value;
				setMapInstance(MapName);
			}
		}

		public Map MapInstance
		{
			get { return mapInstance; }
			set
			{
				if (mapInstance != null)
				{
					mapInstance.Events.MapViewChangedEnded -= Events_MapViewChangedEnded;
				}
				mapInstance = value;
				if (mapInstance != null)
				{
					mapInstance.Events.MapViewChangedEnded += Events_MapViewChangedEnded;
				}
			}
		}
	
		public void Dispose()
		{
			MapInstance = null;
		}


		private void setMapInstance(string mapname)
		{
			MapInstance = Utilities.FindVisualChildByName<Map>(Application.Current.RootVisual, mapname);
		}

		#endregion

		void Events_MapViewChangedEnded(Map map, DeepEarth.Client.MapControl.Events.MapEventArgs args)
		{
			//recreating dateslist
			FillDatesList();
		}

		private string CurrentShortDate()
		{
			if ((mapInstance != null) && (datesList.SelectedItem != null))
			{
				return ((DateTime)datesList.SelectedItem).ToString("yyyyMMdd");
			}
			else
			{
				return null;
			}
		}

		#region Marks
		private void Dates2Marks(List<DateTime> datesList)
		{
			//deleting old marks
			foreach (Ellipse mark in marks)
			{
				sliderCanvas.Children.Remove(mark);
				mark.MouseEnter -= mark_MouseEnter;
				mark.MouseLeave -= mark_MouseLeave;
				mark.MouseLeftButtonDown -= mark_MouseLeftButtonDown;
			}

			marks = new List<Ellipse>();

			//if one - set in center
			if (datesList.Count == 1)
			{
				Ellipse mark = GetMark(sliderCanvas.ActualWidth / 2, datesList[0]);

				sliderCanvas.Children.Add(mark);
				marks.Add(mark);

			}
			//else distributing uniformly
			else if (datesList.Count > 1)
			{
				DateTime minDate = datesList.Min();
				DateTime maxDate = datesList.Max();
				double duration = maxDate.Ticks - minDate.Ticks;

				foreach (DateTime d in datesList)
				{
					Ellipse mark = GetMark(((d.Ticks - minDate.Ticks) / duration) * sliderCanvas.ActualWidth - defaultEllipsMarkWidth/2, d);

					sliderCanvas.Children.Add(mark);
					marks.Add(mark);
				}
			}
		}
		private Ellipse GetMark(double left, DateTime d)
		{
			Ellipse mark = new Ellipse();

			mark.Height = defaultEllipsMarkHeight;
			mark.Width = defaultEllipsMarkWidth;
			mark.Stroke = new SolidColorBrush(Colors.Green);
			mark.Fill = new SolidColorBrush(Colors.Yellow);
			mark.SetValue(Canvas.TopProperty, 0.0);
			mark.SetValue(Canvas.LeftProperty, left);
			mark.DataContext = d;
			mark.MouseEnter += mark_MouseEnter;
			mark.MouseLeave += mark_MouseLeave;
			mark.MouseLeftButtonDown += mark_MouseLeftButtonDown;
			mark.Cursor = Cursors.Hand;

			return mark;
		}

		void mark_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			datesList.SelectedItem = (DateTime)((sender as Ellipse).DataContext);
			RefreshSlider();
		}

		void mark_MouseLeave(object sender, MouseEventArgs e)
		{
			if ((DateTime)((sender as Ellipse).DataContext) != (DateTime)datesList.SelectedItem)
				(sender as Ellipse).Fill = new SolidColorBrush(Colors.Yellow);
		}

		void mark_MouseEnter(object sender, MouseEventArgs e)
		{
			if ((DateTime)((sender as Ellipse).DataContext) != (DateTime)datesList.SelectedItem)
				(sender as Ellipse).Fill = new SolidColorBrush(Colors.Red);
		}

		private void RefreshSlider()
		{
			foreach (Ellipse mark in marks)
			{
				if ((datesList.SelectedItem != null) && ((DateTime)((mark as Ellipse).DataContext) == (DateTime)datesList.SelectedItem))
				{
					mark.Fill = new SolidColorBrush(Colors.Green);
				}
				else
				{
					mark.Fill = new SolidColorBrush(Colors.Yellow);
				}
			}
		}
		#endregion
	}
}
