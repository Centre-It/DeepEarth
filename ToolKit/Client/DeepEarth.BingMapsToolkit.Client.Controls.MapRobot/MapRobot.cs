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
using DeepEarth.BingMapsToolkit.Client.Common;

namespace DeepEarth.BingMapsToolkit.Client.Controls
{
	[TemplatePart(Name = PART_workflowsComboBox, Type = typeof(ComboBox))]
	[TemplatePart(Name = PART_stopButton, Type = typeof(Button))]
	[TemplateVisualState(Name = VSM_Normal, GroupName = VSM_CommonStates)]
	[TemplateVisualState(Name = VSM_MouseOver, GroupName = VSM_CommonStates)]
	public class MapRobot : Control
	{
		private const string PART_workflowsComboBox = "PART_workflowsComboBox";
		private const string PART_stopButton = "PART_stopButton";
		private const string VSM_CommonStates = "CommonStates";
		private const string VSM_MouseOver = "MouseOver";
		private const string VSM_Normal = "Normal";

		private List<Workflow> workflows;
		private Automation automationInstance;
		private string automationName;

		private bool isMouseOver;

		private ComboBox workflowsComboBox;
		private Button stopButton;
		
		public MapRobot()
		{
			this.DefaultStyleKey = typeof(MapRobot);

			IsEnabled = false;

			MouseEnter += mouseEnter;
			MouseLeave += mouseLeave;

			Loaded += MapRobot_Loaded;

			GoToState(true);

			ApplyTemplate();
		}

		void MapRobot_Loaded(object sender, RoutedEventArgs e)
		{
			setAutomationInstance(AutomationName);
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			workflowsComboBox = (ComboBox)GetTemplateChild(PART_workflowsComboBox);
			stopButton = (Button)GetTemplateChild(PART_stopButton);

			IsEnabled = true;

			if (workflows != null)
				workflowsComboBox.ItemsSource = workflows;

			stopButton.Click += new RoutedEventHandler(stopButton_Click);
			workflowsComboBox.SelectionChanged += new SelectionChangedEventHandler(workflowsComboBox_SelectionChanged);
			workflowsComboBox.MouseEnter += new MouseEventHandler(workflowsComboBox_MouseEnter);
		}

		void workflowsComboBox_MouseEnter(object sender, MouseEventArgs e)
		{
			isMouseOver = true;
		}

		void workflowsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (automationInstance != null)
			{
				automationInstance.Process((workflowsComboBox.SelectedItem as Workflow).Maml);
			}
		}

		void stopButton_Click(object sender, RoutedEventArgs e)
		{
			if (automationInstance != null)
			{
				automationInstance.Stop();
			}
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
		}

		public string AutomationName
		{
			get { return automationName; }
			set
			{
				automationName = value;
				setAutomationInstance(AutomationName);
			}
		}

		private void setAutomationInstance(string AutomationName)
		{
			automationInstance = Utilities.FindVisualChildByName<Automation>(Application.Current.RootVisual, AutomationName);
		}

		public Automation AutomationInstance
		{
			get { return automationInstance; }
			set
			{
				automationInstance = value;
			}
		}

		public List<Workflow> Workflows
		{
			get { return workflows; }
			set
			{
				workflows = value;
				if (IsEnabled != false)
					workflowsComboBox.ItemsSource = workflows;
			}
		}
	}
}
