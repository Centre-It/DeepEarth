// Deep Earth is a community project available under the Microsoft Public License (Ms-PL)
// Code is provided as is and with no warrenty – Use at your own risk
// View the project and the latest code at http://DeepEarth.codeplex.com/

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using DeepEarth.BingMapsToolkit.Client.Common.Commanding;

namespace DeepEarth.BingMapsToolkit.Client.Controls
{
    [TemplatePart(Name = PART_Chrome, Type = typeof (FrameworkElement))]
    [TemplatePart(Name = PART_CloseButton, Type = typeof (ButtonBase))]
    [TemplatePart(Name = PART_ContentRoot, Type = typeof (FrameworkElement))]
    [TemplatePart(Name = PART_Root, Type = typeof (Canvas))]
    [TemplatePart(Name = PART_Resizer, Type = typeof (FrameworkElement))]
    [TemplateVisualState(Name = VSMSTATE_StateClosed, GroupName = VSMGROUP_Window)]
    [TemplateVisualState(Name = VSMSTATE_StateOpen, GroupName = VSMGROUP_Window)]
    [TemplateVisualState(Name = VSMSTATE_StateMouseOver, GroupName = VSMGROUP_Resizer)]
    [TemplateVisualState(Name = VSMSTATE_StateNormal, GroupName = VSMGROUP_Resizer)]
    public class FloatingPanel : ContentControl
    {
        private const string ISOKeySnap = "FloatingPanel.Snap";
        private const string ISOKeyX = "FloatingPanel.X";
        private const string ISOKeyY = "FloatingPanel.Y";
        private const string ISOKeyOpen = "FloatingPanel.Open";
        private const string PART_Chrome = "PART_Chrome";
        private const string PART_CloseButton = "PART_CloseButton";
        private const string PART_ContentRoot = "PART_ContentRoot";
        private const string PART_Resizer = "PART_Resizer";
        private const string PART_Root = "PART_Root";
        private const string VSMGROUP_Resizer = "ResizerStates";
        private const string VSMGROUP_Window = "WindowStates";
        private const string VSMSTATE_StateClosed = "Closed";
        private const string VSMSTATE_StateMouseOver = "MouseOver";
        private const string VSMSTATE_StateNormal = "Normal";
        private const string VSMSTATE_StateOpen = "Open";

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof (object), typeof (FloatingPanel), null);

        private FrameworkElement chrome;
        private Point clickPoint;
        private ButtonBase closeButton;
        private FrameworkElement contentRoot;
        private bool isClosing;
        private bool isMoving;
        private bool isResizeMouseOver;
        private bool isResizing;
        private string persistedSettingSnap;
        private bool persistedSettingOpen;
        private string persistedSettingX;
        private string persistedSettingY;
        private FrameworkElement resizer;
        private Canvas root;

        private SnapPosition snapPosition;
        private Point windowPosition;

        //until the interface is drawn we cannot apply the intial position or persisted state, so we store until resize event fires.
        private bool isInitialSnapPositionSet;
        private SnapPosition initialSnapPosition;
        private bool isInitialWindowPositionSet;
        private Point initialWindowPosition;

        private bool isInitialised 
        {
            get
            {
                return (contentRoot != null && contentRoot.ActualWidth > 0);
            }
        }

        public FloatingPanel()
        {
            initialWindowPosition = new Point(0,0);
            initialSnapPosition = SnapPosition.None;
            DefaultStyleKey = typeof (FloatingPanel);
            Loaded += FloatingPanel_Loaded;
            PersistedCommands.PersistedStateLoadedCommand.Executed += PersistedStateLoadedCommand_Executed;
        }

        public SnapPosition SnapPosition
        {
            get { return snapPosition; }
            set
            {
                if (!isInitialised)
                {
                    initialSnapPosition = value;
                    isInitialSnapPositionSet = true;
                    return;
                }
                if (isInitialSnapPositionSet)
                {
                    value = initialSnapPosition;
                    isInitialSnapPositionSet = false;
                }
                if (snapPosition != value)
                {
                    snapPosition = value;
                    switch (snapPosition)
                    {
                        case SnapPosition.Bottom:
                            WindowPosition = new Point(WindowPosition.X, double.MaxValue);
                            break;
                        case SnapPosition.BottomLeft:
                            WindowPosition = new Point(0, double.MaxValue);
                            break;
                        case SnapPosition.BottomRight:
                            WindowPosition = new Point(double.MaxValue, double.MaxValue);
                            break;
                        case SnapPosition.Top:
                            WindowPosition = new Point(WindowPosition.X, 0);
                            break;
                        case SnapPosition.TopLeft:
                            WindowPosition = new Point(0, 0);
                            break;
                        case SnapPosition.TopRight:
                            WindowPosition = new Point(double.MaxValue, 0);
                            break;
                        case SnapPosition.Left:
                            WindowPosition = new Point(0, WindowPosition.Y);
                            break;
                        case SnapPosition.Right:
                            WindowPosition = new Point(double.MaxValue, WindowPosition.Y);
                            break;
                        default:
                            WindowPosition = WindowPosition;
                            break;
                    }
                    if (SnapPositionChanged != null)
                    {
                        SnapPositionChanged(this, new EventArgs());
                    }
                }
            }
        }

        public Point WindowPosition
        {
            get { return windowPosition; }
            set
            {
                if (!isInitialised)
                {
                    initialWindowPosition = value;
                    isInitialWindowPositionSet = true;
                    return;
                }
                if (isInitialWindowPositionSet)
                {
                    isInitialWindowPositionSet = false;
                    value = initialWindowPosition;
                }
                value = validatePosition(value);
                if (windowPosition != value)
                {
                    windowPosition = value;
                    if (contentRoot != null)
                    {
                        contentRoot.SetValue(Canvas.TopProperty, windowPosition.Y);
                        contentRoot.SetValue(Canvas.LeftProperty, windowPosition.X);
                        if (PositionChanged != null)
                        {
                            PositionChanged(this, new EventArgs());
                        }
                    }
                }
                validateDimensions();
            }
        }

        private void validateDimensions()
        {
            if (isInitialised)
            {
                contentRoot.MaxHeight = ActualHeight - WindowPosition.Y;
                contentRoot.MaxWidth = ActualWidth - WindowPosition.X;
            }
        }

        public object Title
        {
            get { return GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        private bool enableResizer = true;
        public bool EnableResizer
        {
            get { return enableResizer; }
            set
            {
                enableResizer = value;
                if (resizer != null)
                {
                    resizer.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }

        public string ISOPrefix { get; set; }

        public event EventHandler Closed;
        public event EventHandler SnapPositionChanged;
        public event EventHandler PositionChanged;

        protected void onClosed()
        {
            if (Closed != null)
            {
                Closed(this, new EventArgs());
            }
        }

        private Point validatePosition(Point value)
        {
            SnapPosition newSnapPosition = SnapPosition.None;
            //must be withing bounds
            if (value.X <= 0)
            {
                value.X = 0;
                newSnapPosition = SnapPosition.Left;
            }
            if (value.Y <= 0)
            {
                value.Y = 0;
                newSnapPosition = newSnapPosition == SnapPosition.Left ? SnapPosition.TopLeft : SnapPosition.Top;
            }
            if (root != null && contentRoot != null)
            {
                if (value.X >= root.ActualWidth - contentRoot.ActualWidth)
                {
                    value.X = root.ActualWidth - contentRoot.ActualWidth;
                    newSnapPosition = newSnapPosition == SnapPosition.Top ? SnapPosition.TopRight : SnapPosition.Right;
                }
                if (value.Y >= root.ActualHeight - contentRoot.ActualHeight)
                {
                    value.Y = root.ActualHeight - contentRoot.ActualHeight;
                    newSnapPosition = newSnapPosition == SnapPosition.Left
                                          ? SnapPosition.BottomLeft
                                          : (newSnapPosition == SnapPosition.Right
                                                 ? SnapPosition.BottomRight
                                                 : SnapPosition.Bottom);
                }
            }
            SnapPosition = newSnapPosition;
            return value;
        }

        private void FloatingPanel_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyTemplate();
            PersistedCommands.PersistedStateSaveCommand.Executed += PersistedStateSaveCommand_Executed;
            goToState(false);
        }

        public void Open()
        {
            isClosing = false;
            goToState(true);
        }

        public void Close()
        {
            isClosing = true;
            goToState(true);
            onClosed();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            chrome = (FrameworkElement) GetTemplateChild(PART_Chrome);
            closeButton = (ButtonBase) GetTemplateChild(PART_CloseButton);
            contentRoot = (FrameworkElement) GetTemplateChild(PART_ContentRoot);
            root = (Canvas) GetTemplateChild(PART_Root);
            resizer = (FrameworkElement) GetTemplateChild(PART_Resizer);

            HorizontalAlignment = HorizontalAlignment.Stretch;
            VerticalAlignment = VerticalAlignment.Stretch;
            //force validation on resize
            Application.Current.Host.Content.Resized += (o, e) => validateCurrentPosition();

            if (chrome != null)
            {
                chrome.MouseLeftButtonDown += (o, e) =>
                                                  {
                                                      chrome.CaptureMouse();
                                                      clickPoint = e.GetPosition(contentRoot);
                                                      isMoving = true;
                                                  };
                chrome.MouseLeftButtonUp += (o, e) =>
                                                {
                                                    chrome.ReleaseMouseCapture();
                                                    isMoving = false;
                                                };
                chrome.MouseMove += (o, e) =>
                                        {
                                            if (isMoving)
                                            {
                                                // If the panel is dragged out of the page, return
                                                Point position = e.GetPosition(Application.Current.RootVisual);
                                                if (Application.Current != null &&
                                                    Application.Current.RootVisual != null &&
                                                    (position.X < 0 || position.Y < 0))
                                                {
                                                    return;
                                                }

                                                Point p = e.GetPosition(root);
                                                WindowPosition = new Point(p.X - clickPoint.X, p.Y - clickPoint.Y);
                                            }
                                        };
            }

            if (closeButton != null)
            {
                closeButton.Click += (o, e) => Close();
            }

            if (resizer != null)
            {
                resizer.MouseLeftButtonDown += (o, e) =>
                                                   {
                                                       resizer.CaptureMouse();
                                                       clickPoint = e.GetPosition(o as UIElement);
                                                       isResizing = true;
                                                   };
                resizer.MouseLeftButtonUp += (o, e) =>
                                                 {
                                                     resizer.ReleaseMouseCapture();
                                                     isResizing = false;
                                                     isResizeMouseOver = false;
                                                     goToState(true);
                                                 };
                resizer.MouseMove += (o, e) =>
                                         {
                                             if (isResizing)
                                             {
                                                 // If the panel is resized out of the page, return
                                                 Point position = e.GetPosition(Application.Current.RootVisual);
                                                 if (Application.Current != null &&
                                                     Application.Current.RootVisual != null &&
                                                     (position.X < 0 || position.Y < 0))
                                                 {
                                                     return;
                                                 }

                                                 Point p = e.GetPosition(contentRoot);

                                                 if ((p.X > clickPoint.X) && (p.Y > clickPoint.Y))
                                                 {
                                                     contentRoot.Width = p.X - (12 - clickPoint.X);
                                                     contentRoot.Height = p.Y - (12 - clickPoint.Y);
                                                 }
                                             }
                                         };
                resizer.MouseEnter += (o, e) =>
                                          {
                                              if (!isResizing)
                                              {
                                                  isResizeMouseOver = true;
                                                  goToState(true);
                                              }
                                          };
                resizer.MouseLeave += (o, e) =>
                                          {
                                              if (!isResizing)
                                              {
                                                  isResizeMouseOver = false;
                                                  goToState(true);
                                              }
                                          };
                EnableResizer = EnableResizer;
            }
            applyPersistedState();
        }

        private void validateCurrentPosition()
        {
            if (isInitialised) {
                //have to maintain any snapping
                SnapPosition previousSnapPosition = (isInitialSnapPositionSet) ? initialSnapPosition : SnapPosition;
                WindowPosition = WindowPosition;
                if (SnapPosition == previousSnapPosition)
                {
                    SnapPosition = SnapPosition.None;
                }
                SnapPosition = previousSnapPosition;
            }
        }

        private void PersistedStateLoadedCommand_Executed(object sender, ExecutedEventArgs e)
        {
            if (((PersistedSettings) e.Parameter).CustomSettings.ContainsKey(ISOPrefix + ISOKeyX))
                persistedSettingX = ((PersistedSettings) e.Parameter).CustomSettings[ISOPrefix + ISOKeyX];
            if (((PersistedSettings)e.Parameter).CustomSettings.ContainsKey(ISOPrefix + ISOKeyY))
                persistedSettingY = ((PersistedSettings) e.Parameter).CustomSettings[ISOPrefix + ISOKeyY];
            if (((PersistedSettings)e.Parameter).CustomSettings.ContainsKey(ISOPrefix + ISOKeySnap))
            persistedSettingSnap = ((PersistedSettings)e.Parameter).CustomSettings[ISOPrefix + ISOKeySnap];

            if ((((PersistedSettings)e.Parameter).CustomSettings.ContainsKey(ISOPrefix + ISOKeyOpen))&&(!bool.TryParse(((PersistedSettings)e.Parameter).CustomSettings[ISOPrefix + ISOKeyOpen], out persistedSettingOpen)))
            {
                persistedSettingOpen = true;
            }
            applyPersistedState();
        }

        private void applyPersistedState()
        {
            try
            {
                //have to wait until we actual have a width to work with.
                if (persistedSettingX != null && persistedSettingY != null && persistedSettingSnap != null)
                {
                    WindowPosition = new Point(double.Parse(persistedSettingX), double.Parse(persistedSettingY));
                    SnapPosition = (SnapPosition) Enum.Parse(typeof (SnapPosition), persistedSettingSnap, true);
                    if (!persistedSettingOpen)
                    {
                        Close();
                    }
                }
            }
            catch (Exception e)
            {
                //ignore
                Debug.WriteLine(e.Message);
            }
        }

        private void PersistedStateSaveCommand_Executed(object sender, ExecutedEventArgs e)
        {
            if (!((PersistedSettings)e.Parameter).CustomSettings.ContainsKey(ISOPrefix + ISOKeyX))
                ((PersistedSettings) e.Parameter).CustomSettings.Add(ISOPrefix + ISOKeyX, WindowPosition.X.ToString());
            if (!((PersistedSettings)e.Parameter).CustomSettings.ContainsKey(ISOPrefix + ISOKeyY))
                ((PersistedSettings) e.Parameter).CustomSettings.Add(ISOPrefix + ISOKeyY, WindowPosition.Y.ToString());
            if (!((PersistedSettings)e.Parameter).CustomSettings.ContainsKey(ISOPrefix + ISOKeySnap))
                ((PersistedSettings) e.Parameter).CustomSettings.Add(ISOPrefix + ISOKeySnap, SnapPosition.ToString());
            if (!((PersistedSettings)e.Parameter).CustomSettings.ContainsKey(ISOPrefix + ISOKeyOpen))
                ((PersistedSettings)e.Parameter).CustomSettings.Add(ISOPrefix + ISOKeyOpen, (!isClosing).ToString());
        }

        private void goToState(bool useTransitions)
        {
            if (isClosing)
            {
                VisualStateManager.GoToState(this, VSMSTATE_StateClosed, useTransitions);
            }
            else
            {
                VisualStateManager.GoToState(this, VSMSTATE_StateOpen, useTransitions);
            }
            if (isResizeMouseOver)
            {
                VisualStateManager.GoToState(this, VSMSTATE_StateMouseOver, useTransitions);
            }
            else
            {
                VisualStateManager.GoToState(this, VSMSTATE_StateNormal, useTransitions);
            }
        }

        public void Dispose()
        {
        }
    }
}