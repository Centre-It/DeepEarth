using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using DeepEarth.BingMapsToolkit.Client.Common.Commanding;

namespace DeepEarth.BingMapsToolkit.Client.Controls
{
	public class StatusCommands
	{
		static StatusCommands()
        {
			ReportStatusCommand = new Command("ReportStatus");
        }

        /// <summary>
        /// Fired when status writed
        /// </summary>
		public static Command ReportStatusCommand { get; private set; }
	}
}
