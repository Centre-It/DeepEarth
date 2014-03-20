// Deep Earth is a community project available under the Microsoft Public License (Ms-PL)
// Code is provided as is and with no warrenty – Use at your own risk
// View the project and the latest code at http://DeepEarth.codeplex.com/

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DeepEarth.Client.Controls
{
    [TemplateVisualState(Name = VSM_Normal, GroupName = VSM_CommonStates)]
    [TemplateVisualState(Name = VSM_MouseOver, GroupName = VSM_CommonStates)]
    [TemplateVisualState(Name = VSM_Open, GroupName = VSM_OptionStates)]
    [TemplateVisualState(Name = VSM_Closed, GroupName = VSM_OptionStates)]
    public class ToolPanel : ContentControl, IDisposable
    {
        private const string VSM_CommonStates = "CommonStates";
        private const string VSM_MouseOver = "MouseOver";
        private const string VSM_Normal = "Normal";
        private const string VSM_OptionStates = "OptionStates";
        private const string VSM_Open = "Open";
        private const string VSM_Closed = "Closed";

        private bool isMouseOver;
        private bool isOptionsOpen;

        #region public object Options
        /// <summary>
        /// Gets or sets the content for the Options of the control. 
        /// </summary>
        /// <remarks>
        /// This property is used to add content to the Options of the 
        /// ToolPanel. You can apply a data template to the 
        /// Options by using the OptionsTemplate property. 
        /// </remarks>
        public object Options
        {
            get { return GetValue(OptionsProperty); }
            set { SetValue(OptionsProperty, value); }
        }

        /// <summary>
        /// Identifies the Options dependency property.
        /// </summary>
        public static readonly DependencyProperty OptionsProperty =
                DependencyProperty.Register(
                        "Options",
                        typeof(object),
                        typeof(ToolPanel),
                        new PropertyMetadata(OnOptionsPropertyChanged));

        /// <summary>
        /// OptionsProperty property changed handler.
        /// </summary>
        /// <param name="d">ToolPanel whose Options property is changed.</param>
        /// <param name="e">DependencyPropertyChangedEventArgs, which contains the old and new value.</param>
        private static void OnOptionsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ToolPanel ctrl = (ToolPanel)d;
            ctrl.OnOptionsChanged(e.OldValue, e.NewValue);
        }
        #endregion public object Options

        #region public DataTemplate OptionsTemplate
        /// <summary>
        /// Gets or sets the template that is used to display the content of 
        /// the control's Options. 
        /// </summary>
        public DataTemplate OptionsTemplate
        {
            get { return (DataTemplate)GetValue(OptionsTemplateProperty); }
            set { SetValue(OptionsTemplateProperty, value); }
        }

        /// <summary>
        /// Identifies the OptionsTemplate dependency property.
        /// </summary>
        public static readonly DependencyProperty OptionsTemplateProperty =
                DependencyProperty.Register(
                        "OptionsTemplate",
                        typeof(DataTemplate),
                        typeof(ToolPanel),
                        new PropertyMetadata(OnOptionsTemplatePropertyChanged));

        /// <summary>
        /// OptionsTemplateProperty property changed handler.
        /// </summary>
        /// <param name="d">ToolPanel whose OptionsTemplate property is changed.</param>
        /// <param name="e">DependencyPropertyChangedEventArgs, which contains the old and new value.</param>
        private static void OnOptionsTemplatePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ToolPanel ctrl = (ToolPanel)d;
            ctrl.OnOptionsTemplateChanged((DataTemplate)e.OldValue, (DataTemplate)e.NewValue);
        }
        #endregion public DataTemplate OptionsTemplate

        /// <summary>
        /// Default DependencyObject constructor.
        /// </summary>
        public ToolPanel()
        {
            DefaultStyleKey = typeof(ToolPanel);
            Loaded += ToolPanel_Loaded;
        }

        void ToolPanel_Loaded(object sender, RoutedEventArgs e)
        {
            MouseEnter += mouseEnter;
            MouseLeave += mouseLeave;
            MouseLeftButtonDown += ToolPanel_MouseLeftButtonDown;
            GoToState(true);
        }

        public void CloseOptions()
        {
            isOptionsOpen = false;
            GoToState(true);
        }

        void ToolPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isOptionsOpen = !isOptionsOpen;
            GoToState(true);
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

        /// <summary>
        /// Called when the value of the Options property changes.
        /// </summary>
        /// <param name="oldOptions">The old value of the Options property.</param>
        /// <param name="newOptions">The new value of the Options property.</param>
        protected virtual void OnOptionsChanged(object oldOptions, object newOptions)
        {
        }

        /// <summary>
        /// Called when the value of the OptionsTemplate property changes.
        /// </summary>
        /// <param name="oldOptionsTemplate">The old value of the OptionsTemplate property.</param>
        /// <param name="newOptionsTemplate">The new value of the OptionsTemplate property.</param>
        protected virtual void OnOptionsTemplateChanged(DataTemplate oldOptionsTemplate, DataTemplate newOptionsTemplate)
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
            if (Options != null && isOptionsOpen)
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
            MouseLeftButtonDown -= ToolPanel_MouseLeftButtonDown;
        }

    }
}
