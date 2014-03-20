using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using DeepEarth.BingMapsToolkit.Common.Entities;

namespace DeepEarth.BingMapsToolkit.Client.Controls
{
    [TemplatePart(Name = PART_dateFromTextBlock, Type = typeof(TextBlock))]
    [TemplatePart(Name = PART_dateToTextBlock, Type = typeof(TextBlock))]
    [TemplatePart(Name = PART_updateButton, Type = typeof(Button))]
    [TemplatePart(Name = PART_toolPanel, Type = typeof(ToolPanel))]
    public class DateRangeDisplayPanel : Control, IDisposable
    {
        private const string PART_dateFromTextBlock = "PART_dateFromTextBlock";
        private const string PART_dateToTextBlock = "PART_dateToTextBlock";
        private const string PART_updateButton = "PART_updateButton";
        private const string PART_toolPanel = "PART_toolPanel";

        private TextBlock dateFromTextBlock;
        private TextBlock dateToTextBlock;
        private Button updateButton;
        private ToolPanel toolPanel;

        private DateRange dateRange;
        private DispatcherTimer timer;

        #region DisplayStringFormat

        /// <summary> 
        /// Identifies the DisplayStringFormat dependency property.
        /// </summary> 
        public static readonly DependencyProperty DisplayStringFormatProperty =
            DependencyProperty.Register(
                "DisplayStringFormat",
                typeof(string),
                typeof(DateRangeDisplayPanel),
                null);

        /// <summary> 
        /// Gets or sets the DisplayStringFormat possible Value of the string object.
        /// </summary> 
        public string DisplayStringFormat
        {
            get { return (string)GetValue(DisplayStringFormatProperty); }
            set { SetValue(DisplayStringFormatProperty, value); }
        }

        #endregion DisplayStringFormat

        public DateRangeDisplayPanel()
        {
            DisplayStringFormat = "";
            dateRange = new DateRange();
            IsEnabled = false;
            DefaultStyleKey = typeof(DateRangeDisplayPanel);

            timer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 2) };
            timer.Tick += timer_Tick;
        }

        public DateRange DateRange { get { return dateRange.Clone(); } }

        private void startTimer()
        {
            timer.Stop();
            timer.Start();
        }

        void timer_Tick(object sender, EventArgs e)
        {
            OnValueChangedThrottled();
        }

        public event EventHandler ValueChangedThrottled;

        protected void OnValueChangedThrottled()
        {
            timer.Stop();
            if (ValueChangedThrottled != null)
            {
                ValueChangedThrottled(this, new EventArgs());
            }
        }

        private DateRangeSlider dateRangeSlider;
        public DateRangeSlider DateRangeSlider
        {
            get { return dateRangeSlider; }
            set
            {
                if (dateRangeSlider != null)
                {
                    //dispose events
                    dateRangeSlider.RangeChanged -= dateRangeSlider_ValueChanged;
                }
                if (value != null)
                {
                    dateRangeSlider = value;
                    dateRangeSlider.RangeChanged += dateRangeSlider_ValueChanged;
                    //set inital values
                    setDisplayValues(dateRangeSlider.RangeStart, dateRangeSlider.RangeEnd);
                }
            }
        }

        void dateRangeSlider_ValueChanged(object sender, EventArgs e)
        {
            startTimer();
            setDisplayValues(dateRangeSlider.RangeStart, dateRangeSlider.RangeEnd);
        }

        private void setDisplayValues(DateTime ValueLow, DateTime ValueHigh)
        {
            if (DateRangeSlider != null)
            {
                dateRange.ValueLow = ValueLow;
                dateRange.ValueHigh = ValueHigh;

                if (dateFromTextBlock != null)
                {
                    dateFromTextBlock.Text = ValueLow.ToString(DisplayStringFormat);
                }
                if (dateToTextBlock != null)
                {
                    dateToTextBlock.Text = ValueHigh.ToString(DisplayStringFormat);
                }
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            dateFromTextBlock = (TextBlock)GetTemplateChild(PART_dateFromTextBlock);
            dateToTextBlock = (TextBlock)GetTemplateChild(PART_dateToTextBlock);
            updateButton = (Button)GetTemplateChild(PART_updateButton);
            toolPanel = (ToolPanel)GetTemplateChild(PART_toolPanel);

            IsEnabled = true;

            //set inital values
            if (dateRangeSlider != null)
            {
                setDisplayValues(dateRangeSlider.RangeStart, dateRangeSlider.RangeEnd);
            }

            DataContext = dateRange;

            //events
            if (updateButton != null)
            {
                updateButton.Click += updateButton_Click;
            }
        }

        void updateButton_Click(object sender, RoutedEventArgs e)
        {
            OnValueChangedThrottled();
            if (DateRangeSlider != null)
            {
                var dr = dateRange.Clone();
                DateRangeSlider.Maximum = dr.ValueHigh.AddDays(7);
                DateRangeSlider.Minimum = dr.ValueLow.AddDays(-7);
                DateRangeSlider.Maximum = dr.ValueHigh.AddDays(7);
                DateRangeSlider.RangeStart = dr.ValueLow;
                DateRangeSlider.RangeEnd = dr.ValueHigh;
            }
            if (toolPanel != null)
            {
                toolPanel.CloseOptions();
            }
        }

        public void Dispose()
        {
            DateRangeSlider = null;
            if (updateButton != null)
            {
                updateButton.Click -= updateButton_Click;
            }
        }
    }
}
