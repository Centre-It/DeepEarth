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
using DeepEarth.BingMapsToolkit.Client.Common;
using Microsoft.Maps.MapControl.Core;
using System.Collections.Generic;

namespace DeepEarth.BingMapsToolkit.Client.Controls
{
    [TemplatePart(Name = PART_logText, Type = typeof (TextBlock))]
	[TemplatePart(Name = PART_statusTextBlock, Type = typeof(TextBox))]
    [TemplateVisualState(Name = VSM_Normal, GroupName = VSM_CommonStates)]
    [TemplateVisualState(Name = VSM_MouseOver, GroupName = VSM_CommonStates)]
	public class StatusPanel : Control, IDisposable
	{
		private const string PART_logText = "PART_logText";
		private const string PART_statusTextBlock = "PART_statusTextBlock";
	 	private const string VSM_CommonStates = "CommonStates";
        private const string VSM_MouseOver = "MouseOver";
        private const string VSM_Normal = "Normal";

		private List<string> logBuffer = new List<string>();
		private const int notesCount = 4;
		private int currNotesCount = 0; 
        private bool isMouseOver;
		private TextBlock logTextBlock;
		private TextBlock newScaleText;

		public StatusPanel()
        {
            IsEnabled = false;
			DefaultStyleKey = typeof(StatusPanel);
			StatusCommands.ReportStatusCommand.Executed += writeLogCommand_Executed;
        }

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			logTextBlock = (TextBlock)GetTemplateChild(PART_logText);
			newScaleText = (TextBlock)GetTemplateChild(PART_statusTextBlock);
			
			IsEnabled = true;

			foreach (string item in logBuffer)
			{
				AddStatus(item);
			}
		}
		
		public void AddStatus(string status)
		{
			if (IsEnabled)
			{
				logTextBlock.Text += status + Environment.NewLine;
				newScaleText.Text = status;
				if (currNotesCount >= notesCount)
					logTextBlock.Text = logTextBlock.Text.Remove(0,
																 logTextBlock.Text.IndexOf(Environment.NewLine) +
																 Environment.NewLine.Length);

				++currNotesCount;
			}
			else
			{
				logBuffer.Add(status);
			}
		}

		private void writeLogCommand_Executed(object sender, Common.Commanding.ExecutedEventArgs e)
		{
			AddStatus((string)e.Parameter);
		}
		
		#region IDisposable Members

		public void Dispose()
		{
		}

		#endregion
	}
}
