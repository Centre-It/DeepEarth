/// Deep Earth is a community project available under the Microsoft Public License (Ms-PL)
/// Code is provided as is and with no warrenty – Use at your own risk
/// View the project and the latest code at http://codeplex.com/deepearth/
/// 
/// Core Slider is from http://blacklight.codeplex.com modifed to handle date range.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace DeepEarth.BingMapsToolkit.Client.Controls
{
    public class DateRangeSlider : Control, IDisposable
    {
        /// <summary>
        /// The element name for the range center thumb.
        /// </summary>
        private const string ElementRangeCenterThumb = "RangeCenterThumb";

        /// <summary>
        /// The element name for the range end thumb.
        /// </summary>
        private const string ElementRangeEndThumb = "RangeEndThumb";

        /// <summary>
        /// The element name for the range start thumb.
        /// </summary>
        private const string ElementRangeStartThumb = "RangeStartThumb";

        /// <summary>
        /// The element name for the selected range borer.
        /// </summary>
        private const string ElementSelectedRangeBorder = "SelectedRangeBorder";

        /// <summary>
        /// The maximum value dependency protperty.
        /// </summary>
        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register("Maximum", typeof(DateTime), typeof(DateRangeSlider),
                                        new PropertyMetadata(DateTime.Now, RangeBounds_Changed));

        /// <summary>
        /// The minimum value dependency protperty.
        /// </summary>
        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register("Minimum", typeof(DateTime), typeof(DateRangeSlider),
                                        new PropertyMetadata(DateTime.Now.AddMonths(-1), RangeBounds_Changed));

        /// <summary>
        /// The minimum range span dependency protperty.
        /// </summary>
        public static readonly DependencyProperty MinimumRangeSpanProperty =
            DependencyProperty.Register("MinimumRangeSpan", typeof(TimeSpan), typeof(DateRangeSlider),
                                        new PropertyMetadata(TimeSpan.FromDays(1)));

        /// <summary>
        /// The range end dependency property.
        /// </summary>
        public static readonly DependencyProperty RangeEndProperty =
            DependencyProperty.Register("RangeEnd", typeof(DateTime), typeof(DateRangeSlider),
                                        new PropertyMetadata(DateTime.Now, Range_Changed));

        /// <summary>
        /// The range start dependency property.
        /// </summary>
        public static readonly DependencyProperty RangeStartProperty =
            DependencyProperty.Register("RangeStart", typeof(DateTime), typeof(DateRangeSlider),
                                        new PropertyMetadata(DateTime.Now.AddDays(-1), Range_Changed));

        /// <summary>
        /// The range center thumb.
        /// </summary>
        private Thumb rangeCenterThumb;

        /// <summary>
        /// The range end thumb.
        /// </summary>
        private Thumb rangeEndThumb;

        /// <summary>
        /// The range start thumb.
        /// </summary>
        private Thumb rangeStartThumb;

        /// <summary>
        /// The selected range border.
        /// </summary>
        private Border selectedRangeBorder;

        /// <summary>
        /// DateRangeSlider constructor.
        /// </summary>
        public DateRangeSlider()
        {
            DefaultStyleKey = typeof(DateRangeSlider);
            SizeChanged += DateRangeSlider_SizeChanged;
        }

        /// <summary>
        /// Gets or sets the slider minimum value.
        /// </summary>
        public DateTime Minimum
        {
            get { return (DateTime)GetValue(MinimumProperty); }

            set
            {
                SetValue(MinimumProperty, value.CompareTo(Maximum) > 0 ? Maximum : value);

                if (Maximum - Minimum < MinimumRangeSpan)
                {
                    MinimumRangeSpan = Maximum - Minimum;
                }
            }
        }

        /// <summary>
        /// Gets or sets the slider maximum value.
        /// </summary>
        public DateTime Maximum
        {
            get { return (DateTime)GetValue(MaximumProperty); }

            set
            {
                SetValue(MaximumProperty, value.CompareTo(Minimum) < 0 ? Minimum : value);

                if (Maximum - Minimum < MinimumRangeSpan)
                {
                    MinimumRangeSpan = Maximum - Minimum;
                }
            }
        }

        /// <summary>
        /// Gets or sets the slider minimum range span.
        /// </summary>
        public TimeSpan MinimumRangeSpan
        {
            get { return (TimeSpan)GetValue(MinimumRangeSpanProperty); }

            set
            {
                TimeSpan diff = Maximum.Subtract(Minimum);
                SetValue(MinimumRangeSpanProperty, diff.CompareTo(value) < 0 ? diff : value);
                UpdateSelectedRangeMinimumWidth();
                UpdateRange(false);
            }
        }

        /// <summary>
        /// Gets or sets the range start.
        /// </summary>
        public DateTime RangeStart
        {
            get { return (DateTime)GetValue(RangeStartProperty); }

            set
            {
                //DateTime rangeStart = Minimum.CompareTo(value) > 0 ? Minimum : value;
                SetValue(RangeStartProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the range end.
        /// </summary>
        public DateTime RangeEnd
        {
            get { return (DateTime)GetValue(RangeEndProperty); }

            set
            {
                //DateTime rangeEnd = Maximum.CompareTo(value) < 0 ? Maximum : value;
                SetValue(RangeEndProperty, value);
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
        }

        #endregion

        /// <summary>
        /// RangeChanged event.
        /// </summary>
        public event EventHandler RangeChanged;

        /// <summary>
        /// Gets the template parts from the template.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            selectedRangeBorder = GetTemplateChild(ElementSelectedRangeBorder) as Border;
            rangeStartThumb = GetTemplateChild(ElementRangeStartThumb) as Thumb;
            if (rangeStartThumb != null)
            {
                rangeStartThumb.DragDelta += RangeStartThumb_DragDelta;
                rangeStartThumb.SizeChanged += RangeThumb_SizeChanged;
            }

            rangeCenterThumb = GetTemplateChild(ElementRangeCenterThumb) as Thumb;
            if (rangeCenterThumb != null)
            {
                rangeCenterThumb.DragDelta += RangeCenterThumb_DragDelta;
            }

            rangeEndThumb = GetTemplateChild(ElementRangeEndThumb) as Thumb;
            if (rangeEndThumb != null)
            {
                rangeEndThumb.DragDelta += RangeEndThumb_DragDelta;
                rangeEndThumb.SizeChanged += RangeThumb_SizeChanged;
            }
        }

        /// <summary>
        /// Updates the thumb mimimum width.
        /// </summary>
        private void UpdateSelectedRangeMinimumWidth()
        {
            if (selectedRangeBorder != null && rangeStartThumb != null && rangeEndThumb != null)
            {
                selectedRangeBorder.MinWidth = Math.Max(
                    rangeStartThumb.ActualWidth + rangeEndThumb.ActualWidth,
                    (MinimumRangeSpan.Ticks / ((double)(Maximum.Ticks - Minimum.Ticks) == 0 ? 1 : (double)(Maximum.Ticks - Minimum.Ticks))) * ActualWidth);
            }
        }

        /// <summary>
        /// Updates the slider UI.
        /// </summary>
        private void UpdateSlider()
        {
            if (selectedRangeBorder != null)
            {
                double startMargin = ((double)(RangeStart.Ticks - Minimum.Ticks) / (double)(Maximum.Ticks - Minimum.Ticks)) * ActualWidth;
                double endMargin = ((double)(Maximum.Ticks - RangeEnd.Ticks) / (double)(Maximum.Ticks - Minimum.Ticks)) * ActualWidth;

                if (!double.IsNaN(startMargin) && !double.IsNaN(endMargin))
                {
                    selectedRangeBorder.Margin = new Thickness(
                        startMargin,
                        selectedRangeBorder.Margin.Top,
                        endMargin,
                        selectedRangeBorder.Margin.Bottom);
                }
            }
        }

        /// <summary>
        /// Updates the selected range.
        /// </summary>
        /// <param name="raiseEvent">Whether the range changed event should fire.</param>
        private void UpdateRange(bool raiseEvent)
        {
            if (selectedRangeBorder != null)
            {
                bool rangeChanged = false;
                double rangeStart = ((Maximum.Ticks - Minimum.Ticks) * (selectedRangeBorder.Margin.Left / ActualWidth)) + Minimum.Ticks;
                double rangeEnd = Maximum.Ticks - ((Maximum.Ticks - Minimum.Ticks) * (selectedRangeBorder.Margin.Right / ActualWidth));

                if (rangeEnd - rangeStart < MinimumRangeSpan.Ticks)
                {
                    if (rangeStart + MinimumRangeSpan.Ticks > Maximum.Ticks)
                    {
                        rangeStart = Maximum.Ticks - MinimumRangeSpan.Ticks;
                    }

                    rangeEnd = Math.Min(Maximum.Ticks, rangeStart + MinimumRangeSpan.Ticks);
                }

                if (rangeStart != RangeStart.Ticks || rangeEnd != RangeEnd.Ticks)
                {
                    rangeChanged = true;
                }

                RangeStart = new DateTime((long)rangeStart);
                RangeEnd = new DateTime((long)rangeEnd);

                if (raiseEvent && rangeChanged && RangeChanged != null)
                {
                    RangeChanged(this, EventArgs.Empty);
                }
            }
        }

        #region Dependency property events.

        /// <summary>
        /// Updates the slider when the selected range changes.
        /// </summary>
        /// <param name="d">The range slider.</param>
        /// <param name="args">Dependency Property Changed Event Args.</param>
        private static void Range_Changed(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            var rangeSlider = d as DateRangeSlider;

            if (rangeSlider != null)
            {

                //validate Min and Max
                if (rangeSlider.Minimum.CompareTo(rangeSlider.RangeStart) > 0)
                {
                    rangeSlider.Minimum = rangeSlider.RangeStart;
                }
                if (rangeSlider.Maximum.CompareTo(rangeSlider.RangeEnd) < 0)
                {
                    rangeSlider.Maximum = rangeSlider.RangeEnd;
                }

                rangeSlider.UpdateSlider();

                if (rangeSlider.RangeChanged != null)
                {
                    rangeSlider.RangeChanged(rangeSlider, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Updates the range start and end values.
        /// </summary>
        /// <param name="d">The range slider.</param>
        /// <param name="args">Dependency Property Changed Event Args.</param>
        private static void RangeBounds_Changed(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            var rangeSlider = d as DateRangeSlider;
            if (rangeSlider != null) rangeSlider.UpdateRange(true);
        }

        #endregion

        #region Range Slider events

        /// <summary>
        /// Updates the slider UI.
        /// </summary>
        /// <param name="sender">The range slider.</param>
        /// <param name="e">Size Changed Event Args.</param>
        private void DateRangeSlider_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateSelectedRangeMinimumWidth();
            UpdateSlider();
        }

        #endregion

        #region Thumb events

        /// <summary>
        /// Updates the slider's minimum width.
        /// </summary>
        /// <param name="sender">The range thumb.</param>
        /// <param name="e">Size changed event args.</param>
        private void RangeThumb_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateSelectedRangeMinimumWidth();
        }

        /// <summary>
        /// Moves the whole range slider.
        /// </summary>
        /// <param name="sender">The range cetner thumb.</param>
        /// <param name="e">Drag Delta Event Args.</param>
        private void RangeCenterThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (selectedRangeBorder != null)
            {
                double startMargin = (selectedRangeBorder.Margin.Left + e.HorizontalChange);
                double endMargin = (selectedRangeBorder.Margin.Right - e.HorizontalChange);

                if (startMargin + e.HorizontalChange <= 0)
                {
                    startMargin = 0;
                    endMargin = ActualWidth - (((double)(RangeEnd.Ticks - RangeStart.Ticks) / (double)(Maximum.Ticks - Minimum.Ticks)) * ActualWidth);
                }
                else if (endMargin - e.HorizontalChange <= 0)
                {
                    endMargin = 0;
                    startMargin = ActualWidth - (((double)(RangeEnd.Ticks - RangeStart.Ticks) / (double)(Maximum.Ticks - Minimum.Ticks)) * ActualWidth);
                }

                if (!double.IsNaN(startMargin) && !double.IsNaN(endMargin))
                {
                    selectedRangeBorder.Margin = new Thickness(
                        startMargin,
                        selectedRangeBorder.Margin.Top,
                        endMargin,
                        selectedRangeBorder.Margin.Bottom);
                }

                UpdateRange(true);
            }
        }

        /// <summary>
        /// Moves the range end thumb.
        /// </summary>
        /// <param name="sender">The range end thumb.</param>
        /// <param name="e">Drag Delta Event Args.</param>
        private void RangeEndThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (selectedRangeBorder != null)
            {
                double endMargin = Math.Min(ActualWidth - selectedRangeBorder.MinWidth,
                                            Math.Max(0, selectedRangeBorder.Margin.Right - e.HorizontalChange));
                double startMargin = selectedRangeBorder.Margin.Left;

                if (ActualWidth - startMargin - endMargin < selectedRangeBorder.MinWidth)
                {
                    startMargin = ActualWidth - endMargin - selectedRangeBorder.MinWidth;
                }

                selectedRangeBorder.Margin = new Thickness(
                    startMargin,
                    selectedRangeBorder.Margin.Top,
                    endMargin,
                    selectedRangeBorder.Margin.Bottom);

                UpdateRange(true);
            }
        }

        /// <summary>
        /// Moves the range start thumb.
        /// </summary>
        /// <param name="sender">The range start thumb.</param>
        /// <param name="e">Drag Delta Event Args.</param>
        private void RangeStartThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (selectedRangeBorder != null)
            {
                double startMargin = Math.Min(ActualWidth - selectedRangeBorder.MinWidth,
                                              Math.Max(0, selectedRangeBorder.Margin.Left + e.HorizontalChange));
                double endMargin = selectedRangeBorder.Margin.Right;

                if (ActualWidth - startMargin - endMargin < selectedRangeBorder.MinWidth)
                {
                    endMargin = ActualWidth - startMargin - selectedRangeBorder.MinWidth;
                }

                selectedRangeBorder.Margin = new Thickness(
                    startMargin,
                    selectedRangeBorder.Margin.Top,
                    endMargin,
                    selectedRangeBorder.Margin.Bottom);

                UpdateRange(true);
            }
        }

        #endregion
    }
}