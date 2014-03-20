﻿// Deep Earth is a community project available under the Microsoft Public License (Ms-PL)
// Code is provided as is and with no warrenty - Use at your own risk
// View the project and the latest code at http://DeepEarth.codeplex.com/

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
using System.Windows.Browser;
using DeepEarth.BingMapsToolkit.Client.Common;

namespace ExampleControlBing.AppDemos
{
	public partial class JavascriptCallingExample : UserControl
	{
		public JavascriptCallingExample()
		{
			InitializeComponent();
			Loaded += JavascriptCallingExample_Loaded;
			
			var listBox = Utilities.FindVisualChildByName<ListBox>(Application.Current.RootVisual, "ExampleAppList");
			listBox.SelectionChanged += listbox_SelectionChanged;
		}

		void listbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			HtmlElement buttonsDiv = HtmlPage.Document.GetElementById("buttonsDiv"); ;
			HtmlPage.Document.GetElementById("form1").RemoveChild(buttonsDiv);
			var listBox = Utilities.FindVisualChildByName<ListBox>(Application.Current.RootVisual, "ExampleAppList");
			listBox.SelectionChanged -= listbox_SelectionChanged;
		}

		void JavascriptCallingExample_Loaded(object sender, RoutedEventArgs e)
		{
			//Generate html, javascript
			HtmlElement host = HtmlPage.Document.GetElementById("form1");

			HtmlElement buttonsDiv = HtmlPage.Document.CreateElement("div");
			buttonsDiv.SetAttribute("id", "buttonsDiv");

			//Coordinates controls
			HtmlElement zoomToXYButton = HtmlPage.Document.CreateElement("input");
			zoomToXYButton.SetAttribute("type", "button");
			zoomToXYButton.SetAttribute("value", "Zoom To Coordinates");
			zoomToXYButton.SetAttribute("onclick", @"
				var xc = document.getElementById('xCoord');
				var yc = document.getElementById('yCoord');
				document.getElementById('BingMapToolkitExample').Content.Automation.ZoomToXY(xc.value, yc.value);");
			zoomToXYButton.SetAttribute("type", "button");

			HtmlElement xCoord = HtmlPage.Document.CreateElement("input");
			xCoord.SetAttribute("id", "xCoord");
			xCoord.SetAttribute("type", "text");
			xCoord.SetAttribute("value", "15");

			HtmlElement yCoord = HtmlPage.Document.CreateElement("input");
			yCoord.SetAttribute("id", "yCoord");
			yCoord.SetAttribute("type", "text");
			yCoord.SetAttribute("value", "15");

			buttonsDiv.AppendChild(zoomToXYButton);
			buttonsDiv.AppendChild(xCoord);
			buttonsDiv.AppendChild(yCoord);

			//Zoom Controls
			HtmlElement zoomToLevelButton = HtmlPage.Document.CreateElement("input");
			zoomToLevelButton.SetAttribute("type", "button");
			zoomToLevelButton.SetAttribute("value", "Zoom To Level");
			zoomToLevelButton.SetAttribute("onclick", @"
				var zoomLevel = document.getElementById('zoomLevel');
				document.getElementById('BingMapToolkitExample').Content.Automation.ZoomToLevel(zoomLevel.value);");
			zoomToLevelButton.SetAttribute("type", "button");

			HtmlElement zoomLevel = HtmlPage.Document.CreateElement("input");
			zoomLevel.SetAttribute("id", "zoomLevel");
			zoomLevel.SetAttribute("type", "text");
			zoomLevel.SetAttribute("value", "15");

			buttonsDiv.AppendChild(zoomToLevelButton);
			buttonsDiv.AppendChild(zoomLevel);

			host.AppendChild(buttonsDiv, HtmlPage.Document.GetElementById("silverlightControlHost"));

			HtmlPage.RegisterScriptableObject("Automation", automation);
		}
	}
}