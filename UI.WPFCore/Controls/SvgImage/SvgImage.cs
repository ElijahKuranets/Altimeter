using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace UI.WPFCore.Controls.SvgImage
{
    /// <summary>
    /// SvgImage displays an image from a svg file added as a resource to the project.
    /// </summary>
    public class SvgImage : Control
    {
        private DrawingGroup? _drawingGroup;

        static SvgImage()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SvgImage),
                new FrameworkPropertyMetadata(typeof(SvgImage)));

            IsTabStopProperty.OverrideMetadata(typeof(SvgImage),
                new FrameworkPropertyMetadata(false));
        }

        public SvgImage()
        {
            Template = (ControlTemplate)TryFindResource("DI_Image");

            MouseEnter += SvgImage_MouseEnter;
            MouseLeave += SvgImage_MouseLeave;
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property.Name == "Foreground" && _drawingGroup!=null)
            {
                OverrideColor(_drawingGroup.Children, ((SolidColorBrush)Foreground).Color);
            }
        }

        private void SvgImage_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (_drawingGroup == null) return;

            OverrideColor(_drawingGroup.Children, ((SolidColorBrush)Foreground).Color);
        }

        private void SvgImage_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (_drawingGroup == null) return;

            OverrideColor(_drawingGroup.Children, HoverColor);
        }

        #region SourceProperty

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.RegisterAttached(
                "Source",
                typeof(Uri),
                typeof(SvgImage),
                new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnSourceValueChanged)));

        public Uri Source
        {
            get => (Uri)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static Uri GetSource(DependencyObject obj)
        {
            return (Uri)obj.GetValue(SourceProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetSource(DependencyObject obj, Uri value)
        {
            obj.SetValue(SourceProperty, value);
        }

        private static void OnSourceValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (SvgImage)obj;

            var e = new RoutedPropertyChangedEventArgs<Uri>(
                (Uri)args.OldValue, (Uri)args.NewValue, SourceValueChangedEvent);

            control.SetSvgImage(control.Source);

            control.OnSourceValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent SourceValueChangedEvent = EventManager.RegisterRoutedEvent(
            "SourceValueChanged", RoutingStrategy.Bubble,
            typeof(RoutedPropertyChangedEventHandler<Uri>), typeof(SvgImage));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnSourceValueChanged(RoutedPropertyChangedEventArgs<Uri> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region SvgDrawingProperty

        public DrawingGroup SvgDrawing
        {
            get => (DrawingGroup)GetValue(SvgDrawingProperty);
            set => SetValue(SvgDrawingProperty, value);
        }

        public static readonly DependencyProperty SvgDrawingProperty =
            DependencyProperty.RegisterAttached(
                "SvgDrawing",
                typeof(DrawingGroup),
                typeof(SvgImage),
                new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnSvgDrawingValueChanged)));

        ///// <summary>
        ///// Gets the Icon.
        ///// </summary>
        public static DrawingGroup GetSvgDrawing(DependencyObject obj)
        {
            return (DrawingGroup)obj.GetValue(SvgDrawingProperty);
        }

        ///// <summary>
        ///// Sets the Icon.
        ///// </summary>
        public static void SetSvgDrawing(DependencyObject obj, DrawingGroup value)
        {
            obj.SetValue(SvgDrawingProperty, value);
        }

        private static void OnSvgDrawingValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (SvgImage)obj;

            var e = new RoutedPropertyChangedEventArgs<DrawingGroup>(
                (DrawingGroup)args.OldValue, (DrawingGroup)args.NewValue, SourceValueChangedEvent);
            control.OnSvgDrawingValueChanged(e);
        }

        ///// <summary>
        ///// Raises the ValueChanged event.
        ///// </summary>
        ///// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnSvgDrawingValueChanged(RoutedPropertyChangedEventArgs<DrawingGroup> args)
        {
            RaiseEvent(args);
        }

        ///// <summary>
        ///// Identifies the ValueChanged routed event.
        ///// </summary>
        public static readonly RoutedEvent SvgDrawingValueChangedEvent = EventManager.RegisterRoutedEvent(
            "SvgDrawingValueChanged", RoutingStrategy.Bubble,
            typeof(RoutedPropertyChangedEventHandler<DrawingGroup>), typeof(SvgImage));

        #endregion

        #region HoverColorProperty

        public static readonly DependencyProperty HoverColorProperty =
        DependencyProperty.RegisterAttached(
          "HoverColor",
          typeof(Color),
          typeof(SvgImage),
                new FrameworkPropertyMetadata(Colors.White, new PropertyChangedCallback(OnHoverColorValueChanged)));


        public Color HoverColor
        {
            get => (Color)GetValue(HoverColorProperty);
            set => SetValue(HoverColorProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static Color GetHoverColor(DependencyObject obj)
        {
            return (Color)obj.GetValue(HoverColorProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetHoverColor(DependencyObject obj, Color value)
        {
            obj.SetValue(HoverColorProperty, value);
        }

        private static void OnHoverColorValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (SvgImage)obj;

            var e = new RoutedPropertyChangedEventArgs<Color>(
              (Color)args.OldValue, (Color)args.NewValue, HoverColorValueChangedEvent);

            control.OnHoverColorValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent HoverColorValueChangedEvent = EventManager.RegisterRoutedEvent(
          "HoverColorValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<Color>), typeof(SvgImage));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnHoverColorValueChanged(RoutedPropertyChangedEventArgs<Color> args)
        {
            RaiseEvent(args);
        }
        #endregion


        private void SetSvgImage(Uri source)
        {
            if (source == null) return;

            var resourceUri = source;

            
            if (!source.ToString().ToLowerInvariant().StartsWith("pack://"))
            {
                resourceUri = new Uri("pack://application:,,,/" + source);
            }

            var loader = new SvgImageLoader();
            _drawingGroup = loader.Load(resourceUri);

            if (_drawingGroup != null)
            {
                // Check if Foreground is set. Leave alone if Color is black.
                // Might consider an override color property, but this is simpler
                if (Foreground is SolidColorBrush solidColorBrush && solidColorBrush.Color != Colors.Black)
                {
                    OverrideColor(_drawingGroup.Children, solidColorBrush.Color);
                }

                SetValue(SvgDrawingProperty, _drawingGroup);
            }
        }

        /// <summary>
        /// Recursively sets the brush color to the given color for all geometry drawings in the drawing tree.
        /// </summary>
        /// <param name="collection">The drawing collection to modify.</param>
        /// <param name="color">The new color to set.</param>
        private static void OverrideColor(DrawingCollection collection, Color color)
        {
            foreach (var drawing in collection)
            {
                switch (drawing)
                {
                    case DrawingGroup group:
                        OverrideColor(group.Children, color); // Recursively process child groups
                        break;
                    case GeometryDrawing geometryDrawing:
                    {
                        // Set the fill brush color
                        geometryDrawing.Brush = new SolidColorBrush(color);

                        // Check if the drawing has a pen (for outlines) and update its brush as well
                        if (geometryDrawing.Pen != null)
                        {
                            geometryDrawing.Pen.Brush = new SolidColorBrush(color);
                        }

                        break;
                    }
                }
            }
        }
    }

    public class SvgImageLoader : SharpVectors.Converters.SvgImageBase
    {
        public DrawingGroup? Load(Uri source)
        {
            //var uri = new Uri("pack://application:,,,/Images/svg/play.svg");
            try
            {
                return GetDrawing(source);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return null;
        }

        public override object? ProvideValue(IServiceProvider serviceProvider)
        {
            return null;
        }
    }
}
