// Deep Earth is a community project available under the Microsoft Public License (Ms-PL)
// Code is provided as is and with no warrenty – Use at your own risk
// View the project and the latest code at http://DeepEarth.codeplex.com/

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DeepEarth.BingMapsToolkit.Client.Common;
using Microsoft.Maps.MapControl.Core;

namespace DeepEarth.BingMapsToolkit.Client.Controls
{
	[TemplatePart(Name = PART_scaleTextBlock, Type = typeof(TextBlock))]
	[TemplatePart(Name = PART_newScaleText, Type = typeof(TextBox))]
	[TemplatePart(Name = PART_btnChangeScaleValue, Type = typeof(Button))]
	[TemplateVisualState(Name = VSM_Normal, GroupName = VSM_CommonStates)]
	[TemplateVisualState(Name = VSM_MouseOver, GroupName = VSM_CommonStates)]
	public class ScalePanel : Control, IMapControl<MapCore>
	{
		private const string PART_scaleTextBlock = "PART_scaleTextBlock";
		private const string PART_newScaleText = "PART_newScaleText";
		private const string PART_btnChangeScaleValue = "PART_btnChangeScaleValue";
		private const string VSM_CommonStates = "CommonStates";
		private const string VSM_MouseOver = "MouseOver";
		private const string VSM_Normal = "Normal";

		private bool isMouseOver;
		private TextBlock scaleTextBlock;
		private TextBox newScaleText;
		private Button btnChangeScaleValue;

		private MapCore mapInstance;
		private string mapName;

		public ScalePanel()
		{
			IsEnabled = false;
			DefaultStyleKey = typeof(ScalePanel);
			Loaded += ScalePanel_Loaded;
		}

		public string MapName
		{
			get { return mapName; }
			set
			{
				mapName = value;
				setMapInstance(MapName);
			}
		}

		public MapCore MapInstance
		{
			get { return mapInstance; }
			set
			{
				if (mapInstance != null)
				{
					mapInstance.ViewChangeEnd -= map_ViewChangeEnd;
					mapInstance.ViewChangeOnFrame -= map_ViewChangeOnFrame;
				}
				mapInstance = value;
				if (mapInstance != null)
				{
					mapInstance.ViewChangeEnd += map_ViewChangeEnd;
					mapInstance.ViewChangeOnFrame += map_ViewChangeOnFrame;
				}
			}
		}

		#region IDisposable Members

		public void Dispose()
		{
			MapInstance = null;
		}

		#endregion

		private void ScalePanel_Loaded(object sender, RoutedEventArgs e)
		{
			setMapInstance(MapName);
		}

		private void setMapInstance(string mapname)
		{
			MapInstance = Utilities.FindVisualChildByName<MapCore>(Application.Current.RootVisual, mapname);
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			scaleTextBlock = (TextBlock)GetTemplateChild(PART_scaleTextBlock);
			newScaleText = (TextBox)GetTemplateChild(PART_newScaleText);
			btnChangeScaleValue = (Button)GetTemplateChild(PART_btnChangeScaleValue);

			IsEnabled = true;

			btnChangeScaleValue.Click += new RoutedEventHandler(btnChangeScaleValue_Click);

		}

		void btnChangeScaleValue_Click(object sender, RoutedEventArgs e)
		{
			double d;
			if (double.TryParse(newScaleText.Text, out d))
				mapInstance.SetView(mapInstance.Center, Utilities.GetZoomLevelAtScale(mapInstance.Center.Latitude, d));
		}

		private void map_ViewChangeOnFrame(object sender, EventArgs e)
		{
			if (IsEnabled)
			{
				setMapScale();
			}
		}

		private void map_ViewChangeEnd(object sender, EventArgs e)
		{
			if (IsEnabled)
			{
				setMapScale();
			}
		}

		private void setMapScale()
		{
			scaleTextBlock.Text = "Scale 1:" + Math.Round(Utilities.GetScaleAtZoomLevel(MapInstance.Center.Latitude, MapInstance.ZoomLevel), 0);
		}
	}
}