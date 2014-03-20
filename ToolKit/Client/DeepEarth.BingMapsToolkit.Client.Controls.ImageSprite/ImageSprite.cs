using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace DeepEarth.BingMapsToolkit.Client.Controls
{
    [TemplatePart(Name = PART_LayoutRoot, Type = typeof (Panel))]
    public class ImageSprite : Control, IDisposable
    {
        private const string PART_LayoutRoot = "PART_LayoutRoot";

        private Panel LayoutRoot;
        private ScaleTransform contentScaleTransform;
        private Storyboard sb;

        public ImageSprite()
        {
            DefaultStyleKey = typeof (ImageSprite);
            Loaded += ImageSprite_Loaded;
        }

        public int SpriteWidth { get; set; }
        public int SpriteHeight { get; set; }
        public int Frames { get; set; }
        public int MilliSecondsPerFrame { get; set; }
        public ImageSource ImageSource { get; set; }

        #region IDisposable Members

        public void Dispose()
        {
            if (sb != null)
            {
                sb.Stop();
                sb = null;
            }
            if (LayoutRoot != null)
            {
                LayoutRoot.SizeChanged -= LayoutRoot_SizeChanged;
            }
        }

        #endregion

        private void ImageSprite_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyTemplate();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            LayoutRoot = (Panel) GetTemplateChild(PART_LayoutRoot);

            setupControls();
        }

        private void setupControls()
        {

            contentScaleTransform = new ScaleTransform();
            var content = new Canvas
                              {
                                  HorizontalAlignment = HorizontalAlignment.Left,
                                  VerticalAlignment = VerticalAlignment.Top,
                                  RenderTransformOrigin = new Point(0.5, 0.5),
                                  RenderTransform = contentScaleTransform
                              };
            LayoutRoot.Children.Add(content);

            var element = new Rectangle
                              {
                                  Width = SpriteWidth,
                                  Height = SpriteHeight
                              };

            var spriteSheet = new ImageBrush
                                  {
                                      Stretch = Stretch.None,
                                      AlignmentX = AlignmentX.Left,
                                      AlignmentY = AlignmentY.Top
                                  };

            var sprite_sheet_position = new TranslateTransform();
            spriteSheet.Transform = sprite_sheet_position;
            spriteSheet.ImageSource = ImageSource;

            element.Fill = spriteSheet;

            var sprite_anim = new DoubleAnimationUsingKeyFrames();
            for (int i = 0; i < Frames; i++)
            {
                var frame_span = new TimeSpan(0, 0, 0, 0, i * MilliSecondsPerFrame);
                sprite_anim.KeyFrames.Add(new DiscreteDoubleKeyFrame
                                              {
                                                  Value = (-SpriteWidth * ((Frames - i) - 1)),
                                                  KeyTime = KeyTime.FromTimeSpan(frame_span)
                                              });
            }

            sb = new Storyboard { RepeatBehavior = RepeatBehavior.Forever };
            sb.Children.Add(sprite_anim);
            Storyboard.SetTarget(sprite_anim, sprite_sheet_position);
            Storyboard.SetTargetProperty(sprite_anim,
                                         new PropertyPath(TranslateTransform.XProperty));
            sb.Begin();

            content.Children.Add(element);
            LayoutRoot.SizeChanged += LayoutRoot_SizeChanged;
        }

        void LayoutRoot_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            contentScaleTransform.ScaleX = LayoutRoot.ActualWidth / SpriteWidth;
            contentScaleTransform.ScaleY = LayoutRoot.ActualHeight / SpriteHeight;
        }
    }
}