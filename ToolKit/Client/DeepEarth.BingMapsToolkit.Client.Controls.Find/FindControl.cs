// Deep Earth is a community project available under the Microsoft Public License (Ms-PL)
// Code is provided as is and with no warrenty – Use at your own risk
// View the project and the latest code at http://DeepEarth.codeplex.com/

using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.ServiceModel;
using System.Windows;
using System.Windows.Browser;
using System.Windows.Controls;
using System.Windows.Input;
using DeepEarth.BingMapsToolkit.Client.Common;
using DeepEarth.BingMapsToolkit.Client.Controls.GeocodeService;
using Microsoft.Maps.MapControl;
using Microsoft.Maps.MapControl.Core;
using System.Windows.Controls.Primitives;
using System.Collections.Generic;
using System.Windows.Media;
using System.Xml.Linq;
using System.Windows.Markup;

namespace DeepEarth.BingMapsToolkit.Client.Controls
{
	[TemplatePart(Name = PART_layersSearchResultScrollViewer, Type = typeof(ScrollViewer))]
	[TemplatePart(Name = PART_allSearchResultsGrid, Type = typeof(Grid))]
	[TemplatePart(Name = PART_anyLocationRadio, Type = typeof(RadioButton))]
	[TemplatePart(Name = PART_currentViewRadio, Type = typeof(RadioButton))]
	[TemplatePart(Name = PART_layersOption, Type = typeof(CheckBox))]
	[TemplatePart(Name = PART_featuresOption, Type = typeof(CheckBox))]
	[TemplatePart(Name = PART_VEOption, Type = typeof(CheckBox))]
	[TemplatePart(Name = PART_triggerOptions, Type = typeof(ToggleButton))]
	[TemplatePart(Name = PART_searchResult, Type = typeof(ToggleButton))]
	[TemplatePart(Name = PART_layersSearchResult, Type = typeof(DataGrid))]
	[TemplatePart(Name = PART_locationsSearchResultBorder, Type = typeof(Border))]
	[TemplatePart(Name = PART_VESearchResultsBorder, Type = typeof(Border))]
	[TemplatePart(Name = PART_layersSearchResultBorder, Type = typeof(Border))]
	[TemplatePart(Name = PART_locationsSearchResultAccordion, Type = typeof(Accordion))]
	[TemplatePart(Name = PART_detailsButton, Type = typeof(HyperlinkButton))]
	[TemplatePart(Name = PART_gotoButton, Type = typeof(HyperlinkButton))]
	[TemplatePart(Name = PART_VESearchResults, Type = typeof(ListBox))]
	[TemplatePart(Name = PART_LoadingIndicator, Type = typeof(UIElement))]
	[TemplateVisualState(Name = VSM_Normal, GroupName = VSM_CommonStates)]
	[TemplateVisualState(Name = VSM_MouseOver, GroupName = VSM_CommonStates)]
	[TemplateVisualState(Name = VSM_OptionsOpened, GroupName = VSM_OptionsStates)]
	[TemplateVisualState(Name = VSM_OptionsClosed, GroupName = VSM_OptionsStates)]
	public class FindControl : Control, IMapControl<MapCore>
	{
		private const string PART_layersSearchResultScrollViewer = "PART_layersSearchResultScrollViewer";
		private const string PART_allSearchResultsGrid = "PART_allSearchResultsGrid";
		private const string PART_anyLocationRadio = "PART_anyLocationRadio";
		private const string PART_currentViewRadio = "PART_currentViewRadio";
		private const string PART_layersOption = "PART_layersOption";
		private const string PART_featuresOption = "PART_featuresOption";
		private const string PART_VEOption = "PART_VEOption";
		private const string PART_triggerOptions = "PART_triggerOptions";
		private const string PART_searchResult = "PART_searchResult";
		private const string PART_ClearFind = "PART_ClearFind";
		private const string PART_FindSearchbox = "PART_FindSearchbox";
		private const string PART_LoadingIndicator = "PART_LoadingIndicator";
		private const string PART_TriggerFind = "PART_TriggerFind";
		private const string PART_layersSearchResult = "PART_layersSearchResult";
		private const string PART_locationsSearchResultBorder = "PART_locationsSearchResultBorder";
		private const string PART_layersSearchResultBorder = "PART_layersSearchResultBorder";
		private const string PART_VESearchResultsBorder = "PART_VESearchResultsBorder";
		private const string PART_locationsSearchResultAccordion = "PART_locationsSearchResultAccordion";
		private const string PART_detailsButton = "PART_detailsButton";
		private const string PART_gotoButton = "PART_gotoButton";
		private const string PART_VESearchResults = "PART_VESearchResults";

		private const string VSM_CommonStates = "CommonStates";
		private const string VSM_OptionsStates = "OptionsStates";
		private const string VSM_MouseOver = "MouseOver";
		private const string VSM_Normal = "Normal";
		private const string VSM_OptionsOpened = "OptionsOpened";
		private const string VSM_OptionsClosed = "OptionsClosed";

		private const string stylePath = "/DeepEarth.BingMapsToolkit.Client.Common;component/Resources/CommonStyles.xaml";

		private Grid allSearchResultsGrid;

		private ScrollViewer layersSearchResultScrollViewer;
		private RadioButton anyLocationRadio;
		private RadioButton currentViewRadio;
		private CheckBox layersOption;
		private CheckBox featuresOption;
		private CheckBox VEOption;

		private ToggleButton triggerOptions;
		private ToggleButton searchResult;
		private DataGrid layersSearchResult;
		private Border locationsSearchResultBorder;
		private Border layersSearchResultBorder;
		private Border VESearchResultsBorder;
		private Accordion locationsSearchResultAccordion;
		private HyperlinkButton detailsButton;
		private HyperlinkButton gotoButton;
		private ListBox VESearchResults;

		private Button clearFind;
		private TextBox findSearchbox;
		private GeocodeServiceClient geocodeClient;
		private GeocodeLayer geocodeLayer;
		private int geocodesInProgress;

		private bool isMouseOver;
		private UIElement loadingIndicator;
		private string mapName;
		private Button triggerFind;

		private bool isOptionsHided = true;

		public event EventHandler<FindLocationEventArgs> OnGetSearchLocation = (u, e) => { ;};
		public event EventHandler<FindLayerEventArgs> OnGetSearchLayer = (u, e) => { ;};

		public FindControl()
		{
			DefaultStyleKey = typeof(FindControl);
			Loaded += FindControl_Loaded;
		}

		public LocationRect PreferredTargetView { get; set; }

		private GeocodeServiceClient GeocodeClient
		{
			get
			{
				if (null == geocodeClient)
				{
					//Handle http/https; OutOfBrowser is currently supported on the MapControl only for http pages
					bool httpsUriScheme = !Application.Current.IsRunningOutOfBrowser &&
										  HtmlPage.Document.DocumentUri.Scheme.Equals(Uri.UriSchemeHttps);
					BasicHttpBinding binding = httpsUriScheme
												   ? new BasicHttpBinding(BasicHttpSecurityMode.Transport)
												   : new BasicHttpBinding(BasicHttpSecurityMode.None);
					var serviceUri =
						new UriBuilder("http://dev.virtualearth.net/webservices/v1/GeocodeService/GeocodeService.svc");
					if (httpsUriScheme)
					{
						//For https, change the UriSceheme to https and change it to use the default https port.
						serviceUri.Scheme = Uri.UriSchemeHttps;
						serviceUri.Port = -1;
					}

					//Create the Service Client
					geocodeClient = new GeocodeServiceClient(binding, new EndpointAddress(serviceUri.Uri));
				}
				return geocodeClient;
			}
		}

		private GeocodeLayer GeocodeLayer
		{
			get
			{
				if (null == geocodeLayer)
				{
					geocodeLayer = new GeocodeLayer(MapInstance);
				}
				return geocodeLayer;
			}
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

		public void Dispose()
		{
			MouseEnter -= mouseEnter;
			MouseLeave -= mouseLeave;
			MapInstance = null;
		}

		#endregion

		public override void OnApplyTemplate()
		{
		
			base.OnApplyTemplate();
			clearFind = (Button)GetTemplateChild(PART_ClearFind);
			findSearchbox = (TextBox)GetTemplateChild(PART_FindSearchbox);
			triggerFind = (Button)GetTemplateChild(PART_TriggerFind);
			loadingIndicator = (UIElement)GetTemplateChild(PART_LoadingIndicator);

			anyLocationRadio = (RadioButton)GetTemplateChild(PART_anyLocationRadio);
			currentViewRadio = (RadioButton)GetTemplateChild(PART_currentViewRadio);
			layersOption = (CheckBox)GetTemplateChild(PART_layersOption);
			featuresOption = (CheckBox)GetTemplateChild(PART_featuresOption);
			VEOption = (CheckBox)GetTemplateChild(PART_VEOption);

			layersSearchResultScrollViewer = (ScrollViewer)GetTemplateChild(PART_layersSearchResultScrollViewer);
			allSearchResultsGrid = (Grid)GetTemplateChild(PART_allSearchResultsGrid);
			triggerOptions = (ToggleButton)GetTemplateChild(PART_triggerOptions);
			searchResult = (ToggleButton)GetTemplateChild(PART_searchResult);
			layersSearchResult = (DataGrid)GetTemplateChild(PART_layersSearchResult);
			locationsSearchResultBorder = (Border)GetTemplateChild(PART_locationsSearchResultBorder);
			layersSearchResultBorder = (Border)GetTemplateChild(PART_layersSearchResultBorder);
			VESearchResultsBorder = (Border)GetTemplateChild(PART_VESearchResultsBorder);
			locationsSearchResultAccordion = (Accordion)GetTemplateChild(PART_locationsSearchResultAccordion);
			detailsButton = (HyperlinkButton)GetTemplateChild(PART_detailsButton);
			gotoButton = (HyperlinkButton)GetTemplateChild(PART_gotoButton);
			VESearchResults = (ListBox)GetTemplateChild(PART_VESearchResults);

			if (clearFind != null)
			{
				clearFind.Click += clearFind_Click;
			}
			if (triggerFind != null)
			{
				triggerFind.Click += triggerFind_Click;
			}
			if (findSearchbox != null)
			{
				findSearchbox.KeyDown += findSearchbox_KeyDown;
			}
			if (VESearchResults != null)
			{
				VESearchResults.SelectionChanged += searchResults_SelectionChanged;
			}
			if (triggerOptions != null)
			{
				triggerOptions.Checked += triggerOptions_Checked;
				triggerOptions.Unchecked += triggerOptions_Unchecked;
			}
			if (searchResult != null)
			{
				searchResult.Checked += searchResult_Checked;
				searchResult.Unchecked += searchResult_Unchecked;
			}
			if (layersSearchResult != null)
			{
				layersSearchResult.SelectionChanged += layersSearchResult_SelectionChanged;
			}
		}

		void layersSearchResult_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			//disable selection on datagrid
			layersSearchResult.SelectedIndex = -1;
		}

		void searchResult_Checked(object sender, RoutedEventArgs e)
		{
			//toggling on search results
			allSearchResultsGrid.Visibility = Visibility.Visible;
		}

		void searchResult_Unchecked(object sender, RoutedEventArgs e)
		{
			//toggling off search results
			allSearchResultsGrid.Visibility = Visibility.Collapsed;
		}

		void triggerOptions_Checked(object sender, RoutedEventArgs e)
		{
			//toggling on options panel
			isOptionsHided = false;
			GoToState(true);
		}

		void triggerOptions_Unchecked(object sender, RoutedEventArgs e)
		{
			//toggling off options panel
			isOptionsHided = true;
			GoToState(true);
		}

		private void findSearchbox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				triggerSearch();
			}
		}

		private void searchResults_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.AddedItems.Count > 0)
			{
				//trigger map view
				var item = (GeocodeResult)e.AddedItems[0];
				if (item.Locations != null)
				{
					Location loc = GeocodeLayer.AddResult(item);
					MapInstance.SetView(loc, 14);
				}
			}
		}

		private void triggerFind_Click(object sender, RoutedEventArgs e)
		{
			triggerSearch();
		}

		private void clearFind_Click(object sender, RoutedEventArgs e)
		{
			searchResult.IsChecked = false;
			clear();
		}

		private void clear()
		{
			GeocodeLayer.Clear();
			findSearchbox.Text = "";
			VEClear();
			LayersClear();
			FeaturesClear();
		}

		private void FeaturesClear()
		{
			if (locationsSearchResultAccordion.ItemsSource != null)
			{
				locationsSearchResultAccordion.ItemsSource = null;
			}
			else
			{
				locationsSearchResultAccordion.SelectedIndex = -1;
				locationsSearchResultAccordion.Items.Clear();
			}
		}

		private void LayersClear()
		{
			layersSearchResult.ItemsSource = null;
			layersSearchResultScrollViewer.Visibility = Visibility.Collapsed;
		}

		private void VEClear()
		{
			if (VESearchResults.ItemsSource != null)
			{
				VESearchResults.ItemsSource = null;
			}
			else
			{
				VESearchResults.Items.Clear();
			}
			VESearchResults.Visibility = Visibility.Collapsed;
		}

		private void triggerSearch()
		{
			if (findSearchbox != null && findSearchbox.Text.Length > 0)
			{
				if (VESearchResults.ItemsSource != null)
					VESearchResults.ItemsSource = null;
				VESearchResults.Visibility = Visibility.Collapsed;

				LocationRect searchRect;

				string searchTerm = findSearchbox.Text;
				string clientType = String.Empty;

				if (anyLocationRadio.IsChecked.Value)
				{
					searchRect = null;
				}
				else
				{
					searchRect = MapInstance.TargetBoundingRectangle;
				}

				LocationsSearch(searchRect);

				LayersSearch(searchRect);

				VESearch(searchRect);
			}
		}

		private void LocationsSearch(LocationRect searchRect)
		{
			if ((featuresOption.IsChecked != null) && (featuresOption.IsChecked.Value))
			{
				searchResult.IsChecked = true;
				locationsSearchResultBorder.Visibility = Visibility.Visible;
				FindLocationEventArgs arg = new FindLocationEventArgs()
				{
					Term = findSearchbox.Text, 
					SearchZone = searchRect,
					OnGetResultLocationsSearchComplete = ShowLocationResult
				};
				OnGetSearchLocation(this, arg);
			}
			else
			{
				locationsSearchResultBorder.Visibility = Visibility.Collapsed;
			}
		}

		private void LayersSearch(LocationRect searchRect)
		{
			if ((layersOption.IsChecked != null) && (layersOption.IsChecked.Value))
			{
				searchResult.IsChecked = true;
				layersSearchResultBorder.Visibility = Visibility.Visible;
				FindLayerEventArgs arg = new FindLayerEventArgs()
				{
					Term = findSearchbox.Text, 
					SearchZone = searchRect,
					OnGetResultLayersSearchComplete = ShowLayersResult
				};
				layersSearchResultScrollViewer.Visibility = Visibility.Visible;
				OnGetSearchLayer(this, arg);
			}
			else
			{
				layersSearchResultBorder.Visibility = Visibility.Collapsed;
			}
		}

		private void VESearch(LocationRect searchRect)
		{
			if ((VEOption.IsChecked != null) && (VEOption.IsChecked.Value))
			{
				searchResult.IsChecked = true;
				VESearchResultsBorder.Visibility = Visibility.Visible;
				geocodeAddress(searchRect, findSearchbox.Text, (s, e) =>
				{
					ProcessResult(e, searchRect);
				});
			}
			else
			{
				VESearchResultsBorder.Visibility = Visibility.Collapsed;
			}
		}

		private void ShowLayersResult(IEnumerable<FoundLayer> layers)
		{
			layersSearchResult.ItemsSource = layers;
		}

		private void ShowLocationResult(IEnumerable<FoundLocationCollection> locations)
		{
			if (locationsSearchResultAccordion.Items.Count > 0)
			{
				locationsSearchResultAccordion.SelectedIndex = -1;
				locationsSearchResultAccordion.Items.Clear();
			}
			foreach (var item in locations)
			{
				ListBox listBox = new ListBox();
				
				listBox.ItemsSource= item.Locations;
				listBox.DisplayMemberPath = "DisplayName";

				listBox.Style = (Style)Application.Current.Resources["ListBoxStyle"];
				listBox.ItemContainerStyle = (Style)Application.Current.Resources["ListBoxItemStyle"];
				listBox.SelectionChanged +=listBox_SelectionChanged;

				AccordionItem a = new AccordionItem();
				a.Header = new TextBlock() { Text = item.LayerTable_Name, Foreground = new SolidColorBrush(Colors.White) };
				a.Content = listBox;
				locationsSearchResultAccordion.Items.Add(a);
			}
		}
	
		void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var item = (FoundLocation)e.AddedItems[0];

			Location loc = GeocodeLayer.AddResult(item.Latitude, item.Longitude, item.DisplayName);
			MapInstance.SetView(loc, 14);
			try
			{
				(sender as ListBox).SelectedIndex = -1;
			}catch {}
		}

		private void FindControl_Loaded(object sender, RoutedEventArgs e)
		{
			setMapInstance(MapName);

			MouseEnter += mouseEnter;
			MouseLeave += mouseLeave;
			GoToState(true);

			LoadMergedDictionary();
		}

		private void LoadMergedDictionary()
		{
			XDocument xaml = XDocument.Load(stylePath);
			ResourceDictionary rd = XamlReader.Load(xaml.ToString()) as ResourceDictionary;
			Application.Current.Resources.MergedDictionaries.Add(rd);
		}

		private void setMapInstance(string mapname)
		{
			MapInstance = Utilities.FindVisualChildByName<MapCore>(Application.Current.RootVisual, mapname);
		}

		private List<GeocodeResult> FilterVEResult(ObservableCollection<GeocodeResult> result, LocationRect bound)
		{
			//filter result inside border
			List<GeocodeResult> resultList;
			if (bound == null)
			{
				resultList = new List<GeocodeResult>(result);
			}
			else
			{
				resultList = new List<GeocodeResult>();
				foreach (var geocodeResult in result)
				{
					bool all = true;

					foreach (var loc in geocodeResult.Locations)
					{
						LocationRect tempRect = new LocationRect(loc, loc);

						if (!bound.Intersects(tempRect))
						{
							all = false;
							break;
						}
					}
					if (all)
						resultList.Add(geocodeResult);
				}
			}
			return resultList;
		}

		private void ProcessResult(GeocodeCompletedEventArgs e, LocationRect bound)
		{
			if (VESearchResults.ItemsSource != null)
				VESearchResults.ItemsSource = null;
			else
				VESearchResults.Items.Clear();
			
			geocodesInProgress--;
			GoToState(true);
			try
			{
				if (e.Result.ResponseSummary.StatusCode == ResponseStatusCode.Success && e.Result.Results.Count > 0)
				{
					VESearchResults.ItemsSource = FilterVEResult(e.Result.Results, bound);
					VESearchResults.Visibility = Visibility.Visible;
				}
				else
				{
					VESearchResults.Items.Add(new GeocodeResult { DisplayName = "Address Not Found" });
					VESearchResults.Visibility = Visibility.Visible;
				}
				(Application.Current.RootVisual as UserControl).Cursor = Cursors.Arrow;
			}
			catch (Exception ex)
			{
				//swollow error?
				Debug.WriteLine(ex.Message);
			}
		}

		private void geocodeAddress(LocationRect searchRect, string address, EventHandler<GeocodeCompletedEventArgs> callback)
		{
			var request = new GeocodeRequest
							  {
								  Culture = MapInstance.Culture,
								  Query = address,
								  UserProfile = new UserProfile { MapView = PreferredTargetView ?? MapInstance.BoundingRectangle },
								  ExecutionOptions = new ExecutionOptions
														 {
															 // Don't raise exceptions.
															 SuppressFaults = true
														 },
								  Options = new GeocodeOptions { Filters = new ObservableCollection<FilterBase>() }
							  };

			// accept results with low confidence as we show these back to the user to choose from.
			var filter = new ConfidenceFilter { MinimumConfidence = Confidence.Low };
			request.Options.Filters.Add(filter);

			geocodesInProgress++;
			GoToState(true);

			MapInstance.CredentialsProvider.GetCredentials(
				credentials =>
				{
					//Pass in credentials for web services call.
					request.Credentials = credentials;

					// Make asynchronous call to fetch the data ... pass state object.
					GeocodeClient.GeocodeCompleted += callback;
					GeocodeClient.GeocodeAsync(request, address);
				});

		}

		private void mouseLeave(object sender, MouseEventArgs e)
		{
			isMouseOver = false;
			if (IsEnabled)
			{
				GoToState(true);
			}
		}

		private void mouseEnter(object sender, MouseEventArgs e)
		{
			isMouseOver = true;
			if (IsEnabled)
			{
				GoToState(true);
			}
		}

		private void GoToState(bool useTransitions)
		{
			if (isMouseOver)
			{
				VisualStateManager.GoToState(this, VSM_MouseOver, useTransitions);
			}
			else
			{
				VisualStateManager.GoToState(this, VSM_Normal, useTransitions);
			}
			if (isOptionsHided)
			{
				VisualStateManager.GoToState(this, VSM_OptionsClosed, useTransitions);
			}
			else
			{
				VisualStateManager.GoToState(this, VSM_OptionsOpened, useTransitions);
			}

			if (loadingIndicator != null)
			{
				loadingIndicator.Visibility = geocodesInProgress > 0 ? Visibility.Visible : Visibility.Collapsed;
			}
		}
	}
}