// Deep Earth is a community project available under the Microsoft Public License (Ms-PL)
// Code is provided as is and with no warrenty – Use at your own risk
// View the project and the latest code at http://DeepEarth.codeplex.com/

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DeepEarth.BingMapsToolkit.Client.Controls
{
    [TemplatePart(Name = PART_LayoutRoot, Type = typeof(Canvas))]
    [TemplateVisualState(Name = VSM_Normal, GroupName = VSM_CommonStates)]
    [TemplateVisualState(Name = VSM_MouseOver, GroupName = VSM_CommonStates)]
    [TemplateVisualState(Name = VSM_Open, GroupName = VSM_LaunchContenttates)]
    [TemplateVisualState(Name = VSM_Closed, GroupName = VSM_LaunchContenttates)]
    public class LaunchPanel : ContentControl, IDisposable
    {
        private const string PART_LayoutRoot = "PART_LayoutRoot";
        private const string VSM_CommonStates = "CommonStates";
        private const string VSM_MouseOver = "MouseOver";
        private const string VSM_Normal = "Normal";
        private const string VSM_LaunchContenttates = "LaunchContenttates";
        private const string VSM_Open = "Open";
        private const string VSM_Closed = "Closed";

        public static readonly DependencyProperty TitleProperty =
    DependencyProperty.Register("Title", typeof(object), typeof(LaunchPanel), null);

        private Canvas root;
        private bool isMouseOver;
        private bool isLaunchContentOpen;

        #region public object LaunchContent
        /// <summary>
        /// Gets or sets the content for the LaunchContent of the control. 
        /// </summary>
        /// <remarks>
        /// This property is used to add content to the LaunchContent of the 
        /// LaunchPanel. You can apply a data template to the 
        /// LaunchContent by using the LaunchContentTemplate property. 
        /// </remarks>
        public object LaunchContent
        {
            get { return GetValue(LaunchContentProperty); }
            set { SetValue(LaunchContentProperty, value); }
        }

        /// <summary>
        /// Identifies the LaunchContent dependency property.
        /// </summary>
        public static readonly DependencyProperty LaunchContentProperty =
                DependencyProperty.Register(
                        "LaunchContent",
                        typeof(object),
                        typeof(LaunchPanel),
                        new PropertyMetadata(OnLaunchContentPropertyChanged));

        /// <summary>
        /// LaunchContentProperty property changed handler.
        /// </summary>
        /// <param name="d">LaunchPanel whose LaunchContent property is changed.</param>
        /// <param name="e">DependencyPropertyChangedEventArgs, which contains the old and new value.</param>
        private static void OnLaunchContentPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            LaunchPanel ctrl = (LaunchPanel)d;
            ctrl.OnLaunchContentChanged(e.OldValue, e.NewValue);
        }
        #endregion public object LaunchContent

        #region public DataTemplate LaunchContentTemplate
        /// <summary>
        /// Gets or sets the template that is used to display the content of 
        /// the control's LaunchContent. 
        /// </summary>
        public DataTemplate LaunchContentTemplate
        {
            get { return (DataTemplate)GetValue(LaunchContentTemplateProperty); }
            set { SetValue(LaunchContentTemplateProperty, value); }
        }

        /// <summary>
        /// Identifies the LaunchContentTemplate dependency property.
        /// </summary>
        public static readonly DependencyProperty LaunchContentTemplateProperty =
                DependencyProperty.Register(
                        "LaunchContentTemplate",
                        typeof(DataTemplate),
                        typeof(LaunchPanel),
                        new PropertyMetadata(OnLaunchContentTemplatePropertyChanged));

        /// <summary>
        /// LaunchContentTemplateProperty property changed handler.
        /// </summary>
        /// <param name="d">LaunchPanel whose LaunchContentTemplate property is changed.</param>
        /// <param name="e">DependencyPropertyChangedEventArgs, which contains the old and new value.</param>
        private static void OnLaunchContentTemplatePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            LaunchPanel ctrl = (LaunchPanel)d;
            ctrl.OnLaunchContentTemplateChanged((DataTemplate)e.OldValue, (DataTemplate)e.NewValue);
        }
        #endregion public DataTemplate LaunchContentTemplate


        public object Title
        {
            get { return GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        /// <summary>
        /// Default DependencyObject constructor.
        /// </summary>
        public LaunchPanel()
        {
            DefaultStyleKey = typeof(LaunchPanel);
            Loaded += LaunchPanel_Loaded;
        }

        void LaunchPanel_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyTemplate();
            MouseEnter += mouseEnter;
            MouseLeave += mouseLeave;
            GoToState(true);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            root = GetTemplateChild(PART_LayoutRoot) as Canvas;

            if (root != null)
            {
                root.Width = ActualWidth;
                SizeChanged += (o, e) => root.Width = ActualWidth;
            }
        }

        public void CloseLaunchContent()
        {
            isLaunchContentOpen = false;
            GoToState(true);
        }

        private void mouseLeave(object sender, MouseEventArgs e)
        {
            isMouseOver = false;
            isLaunchContentOpen = false;
            if (IsEnabled)
            {
                GoToState(true);
            }
        }

        private void mouseEnter(object sender, MouseEventArgs e)
        {
            isLaunchContentOpen = true;
            isMouseOver = true;
            if (IsEnabled)
            {
                GoToState(true);
            }
        }

        /// <summary>
        /// Called when the value of the LaunchContent property changes.
        /// </summary>
        /// <param name="oldLaunchContent">The old value of the LaunchContent property.</param>
        /// <param name="newLaunchContent">The new value of the LaunchContent property.</param>
        protected virtual void OnLaunchContentChanged(object oldLaunchContent, object newLaunchContent)
        {
        }

        /// <summary>
        /// Called when the value of the LaunchContentTemplate property changes.
        /// </summary>
        /// <param name="oldLaunchContentTemplate">The old value of the LaunchContentTemplate property.</param>
        /// <param name="newLaunchContentTemplate">The new value of the LaunchContentTemplate property.</param>
        protected virtual void OnLaunchContentTemplateChanged(DataTemplate oldLaunchContentTemplate, DataTemplate newLaunchContentTemplate)
        {
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
            if (LaunchContent != null && isLaunchContentOpen)
            {
                VisualStateManager.GoToState(this, VSM_Open, useTransitions);
            }
            else
            {
                VisualStateManager.GoToState(this, VSM_Closed, useTransitions);
            }
        }

        public void Dispose()
        {
            MouseEnter -= mouseEnter;
            MouseLeave -= mouseLeave;
        }

    }
}
