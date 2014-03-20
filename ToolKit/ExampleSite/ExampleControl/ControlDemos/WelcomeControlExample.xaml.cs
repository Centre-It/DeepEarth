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
using DeepEarth.BingMapsToolkit.Client.Controls;

namespace ExampleControlBing.ControlDemos
{
	public partial class WelcomeControlExample : UserControl
	{
		static DateTime timeStamp = DateTime.Now.AddSeconds(20);

		public WelcomeControlExample()
		{
			InitializeComponent();

			Loaded += WelcomeControlExample_Loaded;
		}

		void WelcomeControlExample_Loaded(object sender, RoutedEventArgs e)
		{
            welcomecontrol.ShowWelcomeDialog("<TextBlock xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" Foreground=\"#FFFFFFFF\" Text=\"Your content here. The Quick Brown Fox Jumped Over the Lazy Dog.\" />",
				timeStamp);
		}
	}
}
