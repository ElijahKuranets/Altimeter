using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using UI.WPFCore;
using static System.Math;

namespace Dev.Sensors
{
    /// <summary>
    /// Interaction logic for HsiAdiMarkersUserControl.xaml.
    /// </summary>
    /// <remarks>
    /// Add Property code to the partial class below.
    /// </remarks>
    public partial class HsiAdiMarkersUserControl : UserControl
    {
        const double NeedleSpacingFromEdge = 16;

        public enum LineType
        {
            Single,
            Double
        }

        public enum ArrowType
        {
            Open,
            Closed,
            Diamond,
            OpenCircle
        }

        public HsiAdiMarkersUserControl()
        {
            InitializeComponent();
        }

        private double _widthHeight;

        private static readonly SolidColorBrush transparentBrush = new(Colors.Transparent);

        private double _currentAngle;
        private double _totalAngle;
        private double _startAngle;

        bool _dragStarted = false;


        private static readonly SolidColorBrush markerOffBrush = Theme.GetResource(ThemeResourceKey.PrimaryColorBrush9) as SolidColorBrush;
        private static readonly SolidColorBrush markerOuterBrush = new(Colors.LightBlue);
        private static readonly SolidColorBrush markerMiddleBrush = new(Color.FromRgb(255, 178, 0));
        private static readonly SolidColorBrush markerInnerBrush = new(Color.FromRgb(238, 238, 238));

        private void borderCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var p = new PointCollection();

            var border = sender as Border;

            double wh;
            // Resize the canvas to be square
            if (border!.ActualWidth > border.ActualHeight) // border.ActualHeight + 50
            {
                var width = border.ActualWidth * (border.ActualHeight / border.ActualWidth + 0.15); //+ 0.15
                wh = Math.Min(width, width);
            }
            else
            {
                wh = Math.Min(border.ActualWidth, border.ActualHeight);
            }

            _widthHeight = wh;

            SetCanvasWidth(this, wh);
            SetCanvasRect(this, new Rect(0, 0, wh, wh));
            SetCanvasMidPoint(this, new Point(wh / 2, wh / 2));
            SetCanvasBottomLeftPoint(this, new Point(0, wh));

            canvasArtificialHorizon.Width = wh;
            canvasArtificialHorizon.Height = wh;

            canvasNSWE.Width = wh;
            canvasNSWE.Height = wh;

            canvasPrimaryNeedle.Width = wh;
            canvasPrimaryNeedle.Height = wh;
            canvasSecondaryNeedles.Width = wh;
            canvasSecondaryNeedles.Height = wh;

            // Set border size for the horizon element
            borderHorizon.Width = wh;
            borderHorizon.Height = wh * 2;
            Canvas.SetTop(borderHorizon, -wh / 2);
            Canvas.SetLeft(borderHorizon, 0);

            borderAircraft.Width = wh;
            borderAircraft.Height = wh;

            DrawNsweDegreeLinesAndText();
            DrawTouchableBorder();
            DrawCirclesForLateralDeviation();
            
            SetPrimaryNeedleDeviationPointCollection(this, CreatePrimaryNeedleDeviationPointCollection());
            SetPrimaryRefNeedleDeviationPointCollection(this, CreatePrimaryRefNeedleDeviationPointCollection());
            CreateNav1Needle();
            CreateNav12Needle();
            CreateNav2Needle();
            CreateNav22Needle();
            CreateNav3Needle();
            CreateNav32Needle();
            CreateNav4Needle();
            CreateNav42Needle();
        }

        private void ChangeCirclesDeviationColor(Brush brush)
        {
            foreach (FrameworkElement el in canvasPrimaryNeedle.Children)
            {
                if (el is Ellipse ellipse)
                {
                    ellipse.Stroke = brush;
                }
                else if (el is Polyline polyline)
                {
                    if (polyline.Name == "deviationPart")
                    {
                        polyline.Stroke = brush;
                    }
                }
            }
        }

        private void DrawCirclesForLateralDeviation()
        {
            var w = canvasNSWE.Width;
            var h = canvasNSWE.Height;

            Canvas.SetLeft(ellipse1, w / 2 - 35.0);
            Canvas.SetLeft(ellipse2, w / 2 - 70.0);
            Canvas.SetLeft(ellipse3, w / 2 + 26.5);
            Canvas.SetLeft(ellipse4, w / 2 + 61.5);

            Canvas.SetTop(ellipse1, h / 2 - 5);
            Canvas.SetTop(ellipse2, h / 2 - 5);
            Canvas.SetTop(ellipse3, h / 2 - 5);
            Canvas.SetTop(ellipse4, h / 2 - 5);
        }

        private void DrawTriangles()
        {
            var w = canvasNSWE.Width;
            var h = canvasNSWE.Height;
            Canvas.SetLeft(toTriangle, w / 2 - 15.0);
            Canvas.SetLeft(fromTriangle, w / 2 - 15.0);
            Canvas.SetTop(toTriangle, h / 2 - 40.0);
            Canvas.SetTop(fromTriangle, h / 2 - 20.0);
        }

        private void DrawTouchableBorder()
        {
            borderTouchCanvas.Children.Clear();
            var w = canvasNSWE.Width + 40;
            var h = canvasNSWE.Height + 40;

            var cx = w / 2;
            var cy = h / 2;

            var el = new Ellipse
            {
                Width = w,
                Height = h,
                Stroke = new SolidColorBrush(Color.FromRgb(118, 140, 134)),
                StrokeThickness = 10.0,
                Opacity = 0.8
            };

            var radius = 160;
            var ellipse = new Ellipse { Stroke = Brushes.White, StrokeThickness = 50, Opacity = 0 };
            Canvas.SetLeft(ellipse, cx - radius);
            Canvas.SetTop(ellipse, cy - radius);
            ellipse.Width = 2 * radius;
            ellipse.Height = 2 * radius;
            borderTouchCanvas.Children.Add(ellipse);

            borderTouchCanvas.Width = w;
            borderTouchCanvas.Height = h;
            borderTouchCanvas.Children.Add(el);

            //draw triangles

            for (var t = 0; t < 360; t += 30)
            {
                double angle = t + 45;
                var trace = new RotateTransform(angle, cx, cy);

                borderTouchCanvas.Children.Add(new Polyline
                {
                    Points = new PointCollection() { new Point(30, 45), new Point(20, 20), new Point(45, 30) },
                    Fill = new SolidColorBrush(Color.FromRgb(65, 91, 94)),
                    Opacity = 0.8,
                    Stroke = new SolidColorBrush(Color.FromRgb(68, 101, 79)),
                    StrokeThickness = 1,
                    RenderTransform = trace
                });
            }
        }

        private void DrawNsweDegreeLinesAndText()
        {
            // Note: The canvas will always be square.
            canvasNSWE.Children.Clear();
            var w = canvasNSWE.Width;
            var h = canvasNSWE.Height;
            var lineBrush = Theme.GetResource(ThemeResourceKey.PrimaryColorBrush2) as SolidColorBrush;

            for (var t = 0; t < 360; t += 5)
            {
                canvasNSWE.Children.Add(new Line
                {
                    X1 = w / 2,
                    Y1 = 0,
                    X2 = w / 2,
                    Y2 = t % 30 == 0 ? 12 : 8,
                    Style = (Style)FindResource("LineStyleBody1"),
                    StrokeThickness = 1,
                    RenderTransform = new RotateTransform(t, w / 2, w / 2)
                });
            }

            var angle = 0;
            foreach (var str in new[] { "N", "3", "6", "E", "12", "15", "S", "21", "24", "W", "30", "33" })
            {
                var border = new Border
                {
                    Width = 24,
                    Height = 24
                };
                Canvas.SetLeft(border, w / 2);

                // Note: In a transform group, the order is important.
                // Below, the border is first translate transformed, then rotate transformed.
                var transformGroup = new TransformGroup();
                transformGroup.Children.Add(new TranslateTransform(-12, 16));
                transformGroup.Children.Add(new RotateTransform(angle, 0, w / 2));
                border.RenderTransform = transformGroup;

                var label = new Label
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Content = str,
                    Style = (Style)FindResource("LabelStyleBody1")
                };

                border.Child = label;
                canvasNSWE.Children.Add(border);
                angle += 30;
            }
        }

        private PointCollection CreatePrimaryNeedleDeviationPointCollection()
        {
            return new PointCollection(new Point[] {
                new Point(0,56),
                new Point(0,CanvasWidth-56)
            });
        }

        private PointCollection CreatePrimaryRefNeedleDeviationPointCollection()
        {
            return new PointCollection(new Point[] {
                new Point(0,66),
                new Point(0,CanvasWidth-66)
            });
        }

        private GeometryGroup CreateNavNeedle(LineType lineType, ArrowType arrowType, double lineThickness)
        {
            // NOTE: Always create geometry with 0 as the middle point. It will be translated to the middle on the canvas
            var gg = new GeometryGroup();

            lineThickness = 2;

            switch (arrowType)
            {
                case ArrowType.Open:
                    gg.Children.Add(CreateOpenArrow());
                    break;
                case ArrowType.Closed:
                    gg.Children.Add(CreateClosedArrow());
                    break;
                case ArrowType.Diamond:
                    gg.Children.Add(CreateDiamondArrow());
                    break;
                case ArrowType.OpenCircle:
                    gg.Children.Add(CreateOpenCircleArrow());
                    break;
            }

            if (lineType == LineType.Single)
            {
                switch (arrowType)
                {
                    case ArrowType.Closed:
                    case ArrowType.Open:
                        gg.Children.Add(new LineGeometry(new Point(0, NeedleSpacingFromEdge), new Point(0, _widthHeight - NeedleSpacingFromEdge)));
                        break;
                    case ArrowType.OpenCircle:
                        gg.Children.Add(new LineGeometry(
                            new Point(0, NeedleSpacingFromEdge),
                            new Point(0, NeedleSpacingFromEdge + 8)));

                        gg.Children.Add(new LineGeometry(
                            new Point(0, NeedleSpacingFromEdge + 20),
                            new Point(0, _widthHeight - NeedleSpacingFromEdge)));
                        break;
                    case ArrowType.Diamond:
                        gg.Children.Add(new LineGeometry(
                            new Point(0, NeedleSpacingFromEdge),
                            new Point(0, NeedleSpacingFromEdge + 8)));

                        gg.Children.Add(new LineGeometry(
                            new Point(0, NeedleSpacingFromEdge + 24),
                            new Point(0, _widthHeight - NeedleSpacingFromEdge)));
                        break;
                }
            }
            else
            {
                switch (arrowType)
                {
                    case ArrowType.Closed:
                    case ArrowType.Open:
                        var line1 = new LineGeometry(new Point(-2, 1 + NeedleSpacingFromEdge), new Point(-2, _widthHeight - NeedleSpacingFromEdge));
                        var line2 = new LineGeometry(new Point(2, 1 + NeedleSpacingFromEdge), new Point(2, _widthHeight - NeedleSpacingFromEdge));

                        gg.Children.Add(line1);
                        gg.Children.Add(line2);
                        break;
                    case ArrowType.OpenCircle:
                        gg.Children.Add(new LineGeometry(
                            new Point(-2, NeedleSpacingFromEdge),
                            new Point(-2, NeedleSpacingFromEdge + 8)));

                        gg.Children.Add(new LineGeometry(
                            new Point(-2, NeedleSpacingFromEdge + 20),
                            new Point(-2, _widthHeight - NeedleSpacingFromEdge)));

                        gg.Children.Add(new LineGeometry(
                            new Point(2, NeedleSpacingFromEdge),
                            new Point(2, NeedleSpacingFromEdge + 8)));

                        gg.Children.Add(new LineGeometry(
                            new Point(2, NeedleSpacingFromEdge + 20),
                            new Point(2, _widthHeight - NeedleSpacingFromEdge)));
                        break;
                    case ArrowType.Diamond:
                        gg.Children.Add(new LineGeometry(
                            new Point(-2, NeedleSpacingFromEdge),
                            new Point(-2, NeedleSpacingFromEdge + 10)));

                        gg.Children.Add(new LineGeometry(
                            new Point(-2, NeedleSpacingFromEdge + 22),
                            new Point(-2, _widthHeight - NeedleSpacingFromEdge)));

                        gg.Children.Add(new LineGeometry(
                            new Point(2, NeedleSpacingFromEdge),
                            new Point(2, NeedleSpacingFromEdge + 10)));

                        gg.Children.Add(new LineGeometry(
                            new Point(2, NeedleSpacingFromEdge + 22),
                            new Point(2, _widthHeight - NeedleSpacingFromEdge)));
                        break;
                }
            }

            return gg;
        }

        private void CreateNav1Needle()
        {
            Nav1NeedleFill = Nav1ArrowType == ArrowType.Closed ? Nav1Color : transparentBrush;
            SetNav1Data(this, CreateNavNeedle(Nav1NeedleLineType, Nav1ArrowType, Nav1NeedleLineThickness));
        }

        private void CreateNav12Needle()
        {
            Nav12NeedleFill = Nav12ArrowType == ArrowType.Closed ? Nav12Color : transparentBrush;
            SetNav12Data(this, CreateNavNeedle(Nav12NeedleLineType, Nav12ArrowType, Nav12NeedleLineThickness));
        }

        private void CreateNav2Needle()
        {
            Nav2NeedleFill = Nav2ArrowType == ArrowType.Closed ? Nav2Color : transparentBrush;
            SetNav2Data(this, CreateNavNeedle(Nav2NeedleLineType, Nav2ArrowType, Nav2NeedleLineThickness));
        }

        private void CreateNav22Needle()
        {
            Nav22NeedleFill = Nav22ArrowType == ArrowType.Closed ? Nav22Color : transparentBrush;
            SetNav22Data(this, CreateNavNeedle(Nav22NeedleLineType, Nav22ArrowType, Nav22NeedleLineThickness));
        }

        private void CreateNav3Needle()
        {
            Nav3NeedleFill = Nav3ArrowType == ArrowType.Closed ? Nav3Color : transparentBrush;
            SetNav3Data(this, CreateNavNeedle(Nav3NeedleLineType, Nav3ArrowType, Nav3NeedleLineThickness));
        }

        private void CreateNav32Needle()
        {
            Nav32NeedleFill = Nav32ArrowType == ArrowType.Closed ? Nav32Color : transparentBrush;
            SetNav32Data(this, CreateNavNeedle(Nav32NeedleLineType, Nav32ArrowType, Nav32NeedleLineThickness));
        }

        private void CreateNav4Needle()
        {
            Nav4NeedleFill = Nav4ArrowType == ArrowType.Closed ? Nav4Color : transparentBrush;
            SetNav4Data(this, CreateNavNeedle(Nav4NeedleLineType, Nav4ArrowType, Nav4NeedleLineThickness));
        }

        private void CreateNav42Needle()
        {
            Nav42NeedleFill = Nav42ArrowType == ArrowType.Closed ? Nav42Color : transparentBrush;
            SetNav42Data(this, CreateNavNeedle(Nav42NeedleLineType, Nav42ArrowType, Nav42NeedleLineThickness));
        }

        private Geometry CreateOpenArrow()
        {
            var gg = new GeometryGroup();

            gg.Children.Add(new LineGeometry(new Point(0, NeedleSpacingFromEdge), new Point(-6, 6 + NeedleSpacingFromEdge)));
            gg.Children.Add(new LineGeometry(new Point(0, NeedleSpacingFromEdge), new Point(6, 6 + NeedleSpacingFromEdge)));

            return gg;
        }

        private Geometry CreateClosedArrow()
        {
            var gg = new GeometryGroup();

            var path = new PathGeometry();
            path.Figures = new PathFigureCollection();

            // Filled triangle
            path.Figures.Add(new PathFigure(
                new Point(0, NeedleSpacingFromEdge),
                new PathSegment[] {
                    new LineSegment(new Point(-6, 6 + NeedleSpacingFromEdge), true),
                    new LineSegment(new Point(6, 6 + NeedleSpacingFromEdge), true)}, true));

            gg.Children.Add(path);

            return gg;
        }

        private Geometry CreateDiamondArrow()
        {
            var gg = new GeometryGroup();

            var path = new PathGeometry();
            path.Figures = new PathFigureCollection();

            path.Figures.Add(new PathFigure(
                new Point(0, NeedleSpacingFromEdge + 8),
                new PathSegment[] {
                    new LineSegment(new Point(-8, 8 + NeedleSpacingFromEdge + 8), true),
                    new LineSegment(new Point(0, 8 + NeedleSpacingFromEdge + 16), true),
                    new LineSegment(new Point(8, 8 + NeedleSpacingFromEdge + 8), true) }, true));

            gg.Children.Add(path);

            return gg;
        }

        private Geometry CreateOpenCircleArrow()
        {
            var gg = new GeometryGroup();

            gg.Children.Add(new EllipseGeometry(new Point(0, NeedleSpacingFromEdge + 14), 6, 6));
            return gg;
        }
    }

    /// <summary>
    /// HsiAdiMarkersUserControl partial class for all user control properties
    /// </summary>
    public partial class HsiAdiMarkersUserControl : UserControl
    {
        #region PrimaryNavVisibilityProperty

        public static readonly DependencyProperty PrimaryNavVisibilityProperty =
        DependencyProperty.RegisterAttached(
          "PrimaryNavVisibility",
          typeof(Visibility),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(Visibility.Visible, new PropertyChangedCallback(OnPrimaryNavVisibilityValueChanged)));


        public Visibility PrimaryNavVisibility
        {
            get => (Visibility)GetValue(PrimaryNavVisibilityProperty);
            set => SetValue(PrimaryNavVisibilityProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static Visibility GetPrimaryNavVisibility(DependencyObject obj)
        {
            return (Visibility)obj.GetValue(PrimaryNavVisibilityProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetPrimaryNavVisibility(DependencyObject obj, Visibility value)
        {
            obj.SetValue(PrimaryNavVisibilityProperty, value);
        }

        private static void OnPrimaryNavVisibilityValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<Visibility>(
              (Visibility)args.OldValue, (Visibility)args.NewValue, PrimaryNavVisibilityValueChangedEvent);

            control.OnPrimaryNavVisibilityValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent PrimaryNavVisibilityValueChangedEvent = EventManager.RegisterRoutedEvent(
          "PrimaryNavVisibilityValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<Visibility>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnPrimaryNavVisibilityValueChanged(RoutedPropertyChangedEventArgs<Visibility> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region PrimaryNavRefGpDeviationProperty
        public static readonly DependencyProperty PrimaryNavRefGpDeviationProperty =
        DependencyProperty.RegisterAttached(
          "PrimaryNavRefGpDeviation",
          typeof(double),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback(OnPrimaryNavRefGpDeviationValueChanged)));


        public double PrimaryNavRefGpDeviation
        {
            get => (double)GetValue(PrimaryNavRefGpDeviationProperty);
            set => SetValue(PrimaryNavRefGpDeviationProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static double GetPrimaryNavRefGpDeviation(DependencyObject obj)
        {
            return (double)obj.GetValue(PrimaryNavRefGpDeviationProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetPrimaryNavRefGpDeviation(DependencyObject obj, double value)
        {
            obj.SetValue(PrimaryNavRefGpDeviationProperty, value);
        }

        private static void OnPrimaryNavRefGpDeviationValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<double>(
              (double)args.OldValue, (double)args.NewValue, PrimaryNavRefGpDeviationValueChangedEvent);

            control.OnPrimaryNavRefGpDeviationValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent PrimaryNavRefGpDeviationValueChangedEvent = EventManager.RegisterRoutedEvent(
          "PrimaryNavRefGpDeviationValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<double>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnPrimaryNavRefGpDeviationValueChanged(RoutedPropertyChangedEventArgs<double> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region PrimaryNavGpDeviationProperty
        public static readonly DependencyProperty PrimaryNavGpDeviationProperty =
        DependencyProperty.RegisterAttached(
          "PrimaryNavGpDeviation",
          typeof(double),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback(OnPrimaryNavGpDeviationValueChanged)));


        public double PrimaryNavGpDeviation
        {
            get => (double)GetValue(PrimaryNavGpDeviationProperty);
            set => SetValue(PrimaryNavGpDeviationProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static double GetPrimaryNavGpDeviation(DependencyObject obj)
        {
            return (double)obj.GetValue(PrimaryNavGpDeviationProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetPrimaryNavGpDeviation(DependencyObject obj, double value)
        {
            obj.SetValue(PrimaryNavGpDeviationProperty, value);
        }

        private static void OnPrimaryNavGpDeviationValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<double>(
              (double)args.OldValue, (double)args.NewValue, PrimaryNavGpDeviationValueChangedEvent);

            control.OnPrimaryNavGpDeviationValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent PrimaryNavGpDeviationValueChangedEvent = EventManager.RegisterRoutedEvent(
          "PrimaryNavGpDeviationValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<double>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnPrimaryNavGpDeviationValueChanged(RoutedPropertyChangedEventArgs<double> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region PrimaryNavNameProperty

        public static readonly DependencyProperty PrimaryNavNameProperty =
        DependencyProperty.RegisterAttached(
          "PrimaryNavName",
          typeof(string),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(string.Empty, new PropertyChangedCallback(OnPrimaryNavNameValueChanged)));


        public string PrimaryNavName
        {
            get => (string)GetValue(PrimaryNavNameProperty);
            set => SetValue(PrimaryNavNameProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static string GetPrimaryNavName(DependencyObject obj)
        {
            return (string)obj.GetValue(PrimaryNavNameProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetPrimaryNavName(DependencyObject obj, string value)
        {
            obj.SetValue(PrimaryNavNameProperty, value);
        }

        private static void OnPrimaryNavNameValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<string>(
              (string)args.OldValue, (string)args.NewValue, PrimaryNavNameValueChangedEvent);

            control.OnPrimaryNavNameValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent PrimaryNavNameValueChangedEvent = EventManager.RegisterRoutedEvent(
          "PrimaryNavNameValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<string>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnPrimaryNavNameValueChanged(RoutedPropertyChangedEventArgs<string> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region DistanceSelectedProperty

        public static readonly DependencyProperty DistanceSelectedProperty =
        DependencyProperty.RegisterAttached(
          "DistanceSelected",
          typeof(string),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(string.Empty, new PropertyChangedCallback(OnDistanceSelectedValueChanged)));


        public string DistanceSelected
        {
            get => (string)GetValue(DistanceSelectedProperty);
            set => SetValue(DistanceSelectedProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static string GetDistanceSelected(DependencyObject obj)
        {
            return (string)obj.GetValue(DistanceSelectedProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetDistanceSelected(DependencyObject obj, string value)
        {
            obj.SetValue(DistanceSelectedProperty, value);
        }

        private static void OnDistanceSelectedValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<string>(
              (string)args.OldValue, (string)args.NewValue, DistanceSelectedValueChangedEvent);

            control.OnDistanceSelectedValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent DistanceSelectedValueChangedEvent = EventManager.RegisterRoutedEvent(
          "DistanceSelectedValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<string>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnDistanceSelectedValueChanged(RoutedPropertyChangedEventArgs<string> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region PrimaryNavDeviationProperty

        public static readonly DependencyProperty PrimaryNavDeviationProperty =
        DependencyProperty.RegisterAttached(
          "PrimaryNavDeviation",
          typeof(double),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback(OnPrimaryNavDeviationValueChanged)));


        public double PrimaryNavDeviation
        {
            get => (double)GetValue(PrimaryNavDeviationProperty);
            set => SetValue(PrimaryNavDeviationProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static double GetPrimaryNavDeviation(DependencyObject obj)
        {
            return (double)obj.GetValue(PrimaryNavDeviationProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetPrimaryNavDeviation(DependencyObject obj, double value)
        {
            obj.SetValue(PrimaryNavDeviationProperty, value);
        }

        private static void OnPrimaryNavDeviationValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<double>(
              (double)args.OldValue, (double)args.NewValue, PrimaryNavDeviationValueChangedEvent);

            control.OnPrimaryNavDeviationValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent PrimaryNavDeviationValueChangedEvent = EventManager.RegisterRoutedEvent(
          "PrimaryNavDeviationValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<double>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnPrimaryNavDeviationValueChanged(RoutedPropertyChangedEventArgs<double> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region PrimaryNavRefDeviationProperty

        public static readonly DependencyProperty PrimaryNavRefDeviationProperty =
        DependencyProperty.RegisterAttached(
          "PrimaryNavRefDeviation",
          typeof(double),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback(OnPrimaryNavRefDeviationValueChanged)));


        public double PrimaryNavRefDeviation
        {
            get => (double)GetValue(PrimaryNavRefDeviationProperty);
            set => SetValue(PrimaryNavRefDeviationProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static double GetPrimaryNavRefDeviation(DependencyObject obj)
        {
            return (double)obj.GetValue(PrimaryNavRefDeviationProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetPrimaryNavRefDeviation(DependencyObject obj, double value)
        {
            obj.SetValue(PrimaryNavRefDeviationProperty, value);
        }

        private static void OnPrimaryNavRefDeviationValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<double>(
              (double)args.OldValue, (double)args.NewValue, PrimaryNavRefDeviationValueChangedEvent);

            control.OnPrimaryNavRefDeviationValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent PrimaryNavRefDeviationValueChangedEvent = EventManager.RegisterRoutedEvent(
          "PrimaryNavRefDeviationValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<double>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnPrimaryNavRefDeviationValueChanged(RoutedPropertyChangedEventArgs<double> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region PrimaryNavCourseProperty

        public static readonly DependencyProperty PrimaryNavCourseProperty =
        DependencyProperty.RegisterAttached(
          "PrimaryNavCourse",
          typeof(double),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback(OnPrimaryNavCourseValueChanged)));


        public double PrimaryNavCourse
        {
            get => (double)GetValue(PrimaryNavCourseProperty);
            set => SetValue(PrimaryNavCourseProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static double GetPrimaryNavCourse(DependencyObject obj)
        {
            return (double)obj.GetValue(PrimaryNavCourseProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetPrimaryNavCourse(DependencyObject obj, double value)
        {
            obj.SetValue(PrimaryNavCourseProperty, value);
        }

        private static void OnPrimaryNavCourseValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<double>(
              (double)args.OldValue, (double)args.NewValue, PrimaryNavCourseValueChangedEvent);

            control.OnPrimaryNavCourseValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent PrimaryNavCourseValueChangedEvent = EventManager.RegisterRoutedEvent(
          "PrimaryNavCourseValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<double>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnPrimaryNavCourseValueChanged(RoutedPropertyChangedEventArgs<double> args)
        {
            RaiseEvent(args);
        }
        #endregion


        #region PrimaryNavCourseCompassProperty

        public static readonly DependencyProperty PrimaryNavCourseCompassProperty =
        DependencyProperty.RegisterAttached(
          "PrimaryNavCourseCompass",
          typeof(double),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback(OnPrimaryNavCourseCompassValueChanged)));


        public double PrimaryNavCourseCompass
        {
            get => (double)GetValue(PrimaryNavCourseCompassProperty);
            set => SetValue(PrimaryNavCourseCompassProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static double GetPrimaryNavCourseCompass(DependencyObject obj)
        {
            return (double)obj.GetValue(PrimaryNavCourseCompassProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetPrimaryNavCourseCompass(DependencyObject obj, double value)
        {
            obj.SetValue(PrimaryNavCourseCompassProperty, value);
        }

        private static void OnPrimaryNavCourseCompassValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<double>(
              (double)args.OldValue, (double)args.NewValue, PrimaryNavCourseCompassValueChangedEvent);

            control.OnPrimaryNavCourseCompassValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent PrimaryNavCourseCompassValueChangedEvent = EventManager.RegisterRoutedEvent(
          "PrimaryNavCourseCompassValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<double>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnPrimaryNavCourseCompassValueChanged(RoutedPropertyChangedEventArgs<double> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region PrimaryNavRefVisibilityProperty

        public static readonly DependencyProperty PrimaryNavRefVisibilityProperty =
        DependencyProperty.RegisterAttached(
          "PrimaryNavRefVisibility",
          typeof(Visibility),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(Visibility.Visible, new PropertyChangedCallback(OnPrimaryNavRefVisibilityValueChanged)));


        public Visibility PrimaryNavRefVisibility
        {
            get => (Visibility)GetValue(PrimaryNavRefVisibilityProperty);
            set => SetValue(PrimaryNavRefVisibilityProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static Visibility GetPrimaryNavRefVisibility(DependencyObject obj)
        {
            return (Visibility)obj.GetValue(PrimaryNavRefVisibilityProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetPrimaryNavRefVisibility(DependencyObject obj, Visibility value)
        {
            obj.SetValue(PrimaryNavRefVisibilityProperty, value);
        }

        private static void OnPrimaryNavRefVisibilityValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<Visibility>(
              (Visibility)args.OldValue, (Visibility)args.NewValue, PrimaryNavRefVisibilityValueChangedEvent);

            control.OnPrimaryNavRefVisibilityValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent PrimaryNavRefVisibilityValueChangedEvent = EventManager.RegisterRoutedEvent(
          "PrimaryNavRefVisibilityValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<Visibility>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnPrimaryNavRefVisibilityValueChanged(RoutedPropertyChangedEventArgs<Visibility> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region OutBoundInboundTxtProperty

        public static readonly DependencyProperty OutBoundInboundTxtProperty =
        DependencyProperty.RegisterAttached(
          "OutBoundInboundTxt",
          typeof(string),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(string.Empty, new PropertyChangedCallback(OnOutBoundInboundTxtValueChanged)));


        public string OutBoundInboundTxt
        {
            get => (string)GetValue(OutBoundInboundTxtProperty);
            set => SetValue(OutBoundInboundTxtProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static string GetOutBoundInboundTxt(DependencyObject obj)
        {
            return (string)obj.GetValue(OutBoundInboundTxtProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetOutBoundInboundTxt(DependencyObject obj, string value)
        {
            obj.SetValue(PrimaryNavNameProperty, value);
        }

        private static void OnOutBoundInboundTxtValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<string>(
              (string)args.OldValue, (string)args.NewValue, OutBoundInboundTxtValueChangedEvent);

            control.OnOutBoundInboundTxtValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent OutBoundInboundTxtValueChangedEvent = EventManager.RegisterRoutedEvent(
          "OutBoundInboundTxtValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<string>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnOutBoundInboundTxtValueChanged(RoutedPropertyChangedEventArgs<string> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region ShowOutboundInBoundProperty

        public static readonly DependencyProperty ShowOutboundInBoundProperty =
        DependencyProperty.RegisterAttached(
          "ShowOutboundInBound",
          typeof(bool),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(true, new PropertyChangedCallback(OnShowOutboundInBoundValueChanged)));


        public bool ShowOutboundInBound
        {
            get => (bool)GetValue(ShowOutboundInBoundProperty);
            set => SetValue(ShowOutboundInBoundProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static bool GetShowOutboundInBound(DependencyObject obj)
        {
            return (bool)obj.GetValue(ShowOutboundInBoundProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetShowOutboundInBound(DependencyObject obj, bool value)
        {
            obj.SetValue(ShowOutboundInBoundProperty, value);
        }

        private static void OnShowOutboundInBoundValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<bool>(
              (bool)args.OldValue, (bool)args.NewValue, ShowOutboundInBoundValueChangedEvent);

            control.OnShowOutboundInBoundValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent ShowOutboundInBoundValueChangedEvent = EventManager.RegisterRoutedEvent(
          "ShowOutboundInBoundValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<bool>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnShowOutboundInBoundValueChanged(RoutedPropertyChangedEventArgs<bool> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region ShowLateralGuidanceReferenceProperty

        public static readonly DependencyProperty ShowLateralGuidanceReferenceProperty =
        DependencyProperty.RegisterAttached(
          "ShowLateralGuidanceReference",
          typeof(bool),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(true, new PropertyChangedCallback(OnShowLateralGuidanceReferenceValueChanged)));


        public bool ShowLateralGuidanceReference
        {
            get => (bool)GetValue(ShowLateralGuidanceReferenceProperty);
            set => SetValue(ShowLateralGuidanceReferenceProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static bool GetShowLateralGuidanceReference(DependencyObject obj)
        {
            return (bool)obj.GetValue(ShowLateralGuidanceReferenceProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetShowLateralGuidanceReference(DependencyObject obj, bool value)
        {
            obj.SetValue(ShowLateralGuidanceReferenceProperty, value);
        }

        private static void OnShowLateralGuidanceReferenceValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<bool>(
              (bool)args.OldValue, (bool)args.NewValue, ShowLateralGuidanceReferenceValueChangedEvent);

            control.OnShowLateralGuidanceReferenceValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent ShowLateralGuidanceReferenceValueChangedEvent = EventManager.RegisterRoutedEvent(
          "ShowLateralGuidanceReferenceValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<bool>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnShowLateralGuidanceReferenceValueChanged(RoutedPropertyChangedEventArgs<bool> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region ShowLateralGuidanceProperty

        public static readonly DependencyProperty ShowLateralGuidanceProperty =
        DependencyProperty.RegisterAttached(
          "ShowLateralGuidance",
          typeof(bool),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(true, new PropertyChangedCallback(OnShowLateralGuidanceValueChanged)));


        public bool ShowLateralGuidance
        {
            get => (bool)GetValue(ShowLateralGuidanceProperty);
            set => SetValue(ShowLateralGuidanceProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static bool GetShowLateralGuidance(DependencyObject obj)
        {
            return (bool)obj.GetValue(ShowLateralGuidanceProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetShowLateralGuidance(DependencyObject obj, bool value)
        {
            obj.SetValue(ShowLateralGuidanceProperty, value);
        }

        private static void OnShowLateralGuidanceValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<bool>(
              (bool)args.OldValue, (bool)args.NewValue, ShowLateralGuidanceValueChangedEvent);

            control.OnShowLateralGuidanceValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent ShowLateralGuidanceValueChangedEvent = EventManager.RegisterRoutedEvent(
          "ShowLateralGuidanceValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<bool>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnShowLateralGuidanceValueChanged(RoutedPropertyChangedEventArgs<bool> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region ShowVerticalGuidanceProperty

        public static readonly DependencyProperty ShowVerticalGuidanceProperty =
        DependencyProperty.RegisterAttached(
          "ShowVerticalGuidance",
          typeof(bool),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(true, new PropertyChangedCallback(OnShowVerticalGuidanceValueChanged)));


        public bool ShowVerticalGuidance
        {
            get => (bool)GetValue(ShowVerticalGuidanceProperty);
            set => SetValue(ShowVerticalGuidanceProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static bool GetShowVerticalGuidance(DependencyObject obj)
        {
            return (bool)obj.GetValue(ShowVerticalGuidanceProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetShowVerticalGuidance(DependencyObject obj, bool value)
        {
            obj.SetValue(ShowVerticalGuidanceProperty, value);
        }

        private static void OnShowVerticalGuidanceValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<bool>(
              (bool)args.OldValue, (bool)args.NewValue, ShowVerticalGuidanceValueChangedEvent);

            control.OnShowVerticalGuidanceValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent ShowVerticalGuidanceValueChangedEvent = EventManager.RegisterRoutedEvent(
          "ShowVerticalGuidanceValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<bool>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnShowVerticalGuidanceValueChanged(RoutedPropertyChangedEventArgs<bool> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region ShowVerticalGuidanceReferenceProperty

        public static readonly DependencyProperty ShowVerticalGuidanceReferenceProperty =
        DependencyProperty.RegisterAttached(
          "ShowVerticalGuidanceReference",
          typeof(bool),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(true, new PropertyChangedCallback(OnShowVerticalGuidanceReferenceValueChanged)));


        public bool ShowVerticalGuidanceReference
        {
            get => (bool)GetValue(ShowVerticalGuidanceReferenceProperty);
            set => SetValue(ShowVerticalGuidanceReferenceProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static bool GetShowVerticalGuidanceReference(DependencyObject obj)
        {
            return (bool)obj.GetValue(ShowVerticalGuidanceReferenceProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetShowVerticalGuidanceReference(DependencyObject obj, bool value)
        {
            obj.SetValue(ShowVerticalGuidanceReferenceProperty, value);
        }

        private static void OnShowVerticalGuidanceReferenceValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<bool>(
              (bool)args.OldValue, (bool)args.NewValue, ShowVerticalGuidanceReferenceValueChangedEvent);

            control.OnShowVerticalGuidanceReferenceValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent ShowVerticalGuidanceReferenceValueChangedEvent = EventManager.RegisterRoutedEvent(
          "ShowVerticalGuidanceReferenceValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<bool>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnShowVerticalGuidanceReferenceValueChanged(RoutedPropertyChangedEventArgs<bool> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region ShowGlideslopeProperty

        public static readonly DependencyProperty ShowGlideslopeProperty =
        DependencyProperty.RegisterAttached(
          "ShowGlideslope",
          typeof(bool),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(true, new PropertyChangedCallback(OnShowGlideslopeValueChanged)));


        public bool ShowGlideslope
        {
            get => (bool)GetValue(ShowGlideslopeProperty);
            set => SetValue(ShowGlideslopeProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static bool GetShowGlideslope(DependencyObject obj)
        {
            return (bool)obj.GetValue(ShowGlideslopeProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetShowGlideslope(DependencyObject obj, bool value)
        {
            obj.SetValue(ShowGlideslopeProperty, value);
        }

        private static void OnShowGlideslopeValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<bool>(
              (bool)args.OldValue, (bool)args.NewValue, ShowGlideslopeValueChangedEvent);

            control.OnShowGlideslopeValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent ShowGlideslopeValueChangedEvent = EventManager.RegisterRoutedEvent(
          "ShowGlideslopeValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<bool>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnShowGlideslopeValueChanged(RoutedPropertyChangedEventArgs<bool> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region Nav1VisibilityProperty

        public static readonly DependencyProperty Nav1VisibilityProperty =
        DependencyProperty.RegisterAttached(
          "Nav1Visibility",
          typeof(Visibility),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(Visibility.Hidden, new PropertyChangedCallback(OnNav1VisibilityValueChanged)));


        public Visibility Nav1Visibility
        {
            get => (Visibility)GetValue(Nav1VisibilityProperty);
            set => SetValue(Nav1VisibilityProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static Visibility GetNav1Visibility(DependencyObject obj)
        {
            return (Visibility)obj.GetValue(Nav1VisibilityProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetNav1Visibility(DependencyObject obj, Visibility value)
        {
            obj.SetValue(Nav1VisibilityProperty, value);
        }

        private static void OnNav1VisibilityValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<Visibility>(
              (Visibility)args.OldValue, (Visibility)args.NewValue, Nav1VisibilityValueChangedEvent);

            if (((Visibility)args.NewValue) == Visibility.Visible) control.CreateNav1Needle();

            control.OnNav1VisibilityValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent Nav1VisibilityValueChangedEvent = EventManager.RegisterRoutedEvent(
          "Nav1VisibilityValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<Visibility>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnNav1VisibilityValueChanged(RoutedPropertyChangedEventArgs<Visibility> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region Nav12VisibilityProperty

        public static readonly DependencyProperty Nav12VisibilityProperty =
        DependencyProperty.RegisterAttached(
          "Nav12Visibility",
          typeof(Visibility),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(Visibility.Hidden, new PropertyChangedCallback(OnNav12VisibilityValueChanged)));


        public Visibility Nav12Visibility
        {
            get => (Visibility)GetValue(Nav12VisibilityProperty);
            set => SetValue(Nav12VisibilityProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static Visibility GetNav12Visibility(DependencyObject obj)
        {
            return (Visibility)obj.GetValue(Nav12VisibilityProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetNav12Visibility(DependencyObject obj, Visibility value)
        {
            obj.SetValue(Nav12VisibilityProperty, value);
        }

        private static void OnNav12VisibilityValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<Visibility>(
              (Visibility)args.OldValue, (Visibility)args.NewValue, Nav12VisibilityValueChangedEvent);

            if (((Visibility)args.NewValue) == Visibility.Visible) control.CreateNav12Needle();
            control.OnNav12VisibilityValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent Nav12VisibilityValueChangedEvent = EventManager.RegisterRoutedEvent(
          "Nav12VisibilityValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<Visibility>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnNav12VisibilityValueChanged(RoutedPropertyChangedEventArgs<Visibility> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region Nav1NameProperty

        public static readonly DependencyProperty Nav1NameProperty =
        DependencyProperty.RegisterAttached(
          "Nav1Name",
          typeof(string),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(string.Empty, new PropertyChangedCallback(OnNav1NameValueChanged)));


        public string Nav1Name
        {
            get => (string)GetValue(Nav1NameProperty);
            set => SetValue(Nav1NameProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static string GetNav1Name(DependencyObject obj)
        {
            return (string)obj.GetValue(Nav1NameProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetNav1Name(DependencyObject obj, string value)
        {
            obj.SetValue(Nav1NameProperty, value);
        }

        private static void OnNav1NameValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<string>(
              (string)args.OldValue, (string)args.NewValue, Nav1NameValueChangedEvent);

            control.OnNav1NameValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent Nav1NameValueChangedEvent = EventManager.RegisterRoutedEvent(
          "Nav1NameValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<string>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnNav1NameValueChanged(RoutedPropertyChangedEventArgs<string> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region Nav12NameProperty

        public static readonly DependencyProperty Nav12NameProperty =
        DependencyProperty.RegisterAttached(
          "Nav12Name",
          typeof(string),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(string.Empty, new PropertyChangedCallback(OnNav12NameValueChanged)));


        public string Nav12Name
        {
            get => (string)GetValue(Nav12NameProperty);
            set => SetValue(Nav12NameProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static string GetNav12Name(DependencyObject obj)
        {
            return (string)obj.GetValue(Nav12NameProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetNav12Name(DependencyObject obj, string value)
        {
            obj.SetValue(Nav12NameProperty, value);
        }

        private static void OnNav12NameValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<string>(
              (string)args.OldValue, (string)args.NewValue, Nav12NameValueChangedEvent);

            control.OnNav12NameValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent Nav12NameValueChangedEvent = EventManager.RegisterRoutedEvent(
          "Nav12NameValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<string>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnNav12NameValueChanged(RoutedPropertyChangedEventArgs<string> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region Nav1BearingProperty

        public static readonly DependencyProperty Nav1BearingProperty =
        DependencyProperty.RegisterAttached(
          "Nav1Bearing",
          typeof(double),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback(OnNav1BearingValueChanged)));


        public double Nav1Bearing
        {
            get => (double)GetValue(Nav1BearingProperty);
            set => SetValue(Nav1BearingProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static double GetNav1Bearing(DependencyObject obj)
        {
            return (double)obj.GetValue(Nav1BearingProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetNav1Bearing(DependencyObject obj, double value)
        {
            obj.SetValue(Nav1BearingProperty, value);
        }

        private static void OnNav1BearingValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<double>(
              (double)args.OldValue, (double)args.NewValue, Nav1BearingValueChangedEvent);

            control.OnNav1BearingValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent Nav1BearingValueChangedEvent = EventManager.RegisterRoutedEvent(
          "Nav1BearingValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<double>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnNav1BearingValueChanged(RoutedPropertyChangedEventArgs<double> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region Nav12BearingProperty

        public static readonly DependencyProperty Nav12BearingProperty =
        DependencyProperty.RegisterAttached(
          "Nav12Bearing",
          typeof(double),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback(OnNav12BearingValueChanged)));


        public double Nav12Bearing
        {
            get => (double)GetValue(Nav12BearingProperty);
            set => SetValue(Nav12BearingProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static double GetNav12Bearing(DependencyObject obj)
        {
            return (double)obj.GetValue(Nav12BearingProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetNav12Bearing(DependencyObject obj, double value)
        {
            obj.SetValue(Nav12BearingProperty, value);
        }

        private static void OnNav12BearingValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<double>(
              (double)args.OldValue, (double)args.NewValue, Nav12BearingValueChangedEvent);

            control.OnNav12BearingValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent Nav12BearingValueChangedEvent = EventManager.RegisterRoutedEvent(
          "Nav12BearingValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<double>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnNav12BearingValueChanged(RoutedPropertyChangedEventArgs<double> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region Nav1ColorProperty

        public static readonly DependencyProperty Nav1ColorProperty =
        DependencyProperty.RegisterAttached(
          "Nav1Color",
          typeof(Brush),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(new SolidColorBrush(Colors.White), new PropertyChangedCallback(OnNav1ColorValueChanged)));


        public Brush Nav1Color
        {
            get => (Brush)GetValue(Nav1ColorProperty);
            set => SetValue(Nav1ColorProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static Brush GetNav1Color(DependencyObject obj)
        {
            return (Brush)obj.GetValue(Nav1ColorProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetNav1Color(DependencyObject obj, Brush value)
        {
            obj.SetValue(Nav1ColorProperty, value);
        }

        private static void OnNav1ColorValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<Brush>(
              (Brush)args.OldValue, (Brush)args.NewValue, Nav1ColorValueChangedEvent);

            control.OnNav1ColorValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent Nav1ColorValueChangedEvent = EventManager.RegisterRoutedEvent(
          "Nav1ColorValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<Brush>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnNav1ColorValueChanged(RoutedPropertyChangedEventArgs<Brush> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region Nav12ColorProperty

        public static readonly DependencyProperty Nav12ColorProperty =
        DependencyProperty.RegisterAttached(
          "Nav12Color",
          typeof(Brush),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(new SolidColorBrush(Colors.White), new PropertyChangedCallback(OnNav12ColorValueChanged)));


        public Brush Nav12Color
        {
            get => (Brush)GetValue(Nav12ColorProperty);
            set => SetValue(Nav12ColorProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static Brush GetNav12Color(DependencyObject obj)
        {
            return (Brush)obj.GetValue(Nav12ColorProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetNav12Color(DependencyObject obj, Brush value)
        {
            obj.SetValue(Nav12ColorProperty, value);
        }

        private static void OnNav12ColorValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<Brush>(
              (Brush)args.OldValue, (Brush)args.NewValue, Nav12ColorValueChangedEvent);

            control.OnNav12ColorValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent Nav12ColorValueChangedEvent = EventManager.RegisterRoutedEvent(
          "Nav12ColorValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<Brush>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnNav12ColorValueChanged(RoutedPropertyChangedEventArgs<Brush> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region Nav1NeedleLineTypeProperty

        public static readonly DependencyProperty Nav1NeedleLineTypeProperty =
        DependencyProperty.RegisterAttached(
          "Nav1NeedleLineType",
          typeof(LineType),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(LineType.Single, new PropertyChangedCallback(OnNav1NeedleLineTypeValueChanged)));


        public LineType Nav1NeedleLineType
        {
            get => (LineType)GetValue(Nav1NeedleLineTypeProperty);
            set => SetValue(Nav1NeedleLineTypeProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static LineType GetNav1NeedleLineType(DependencyObject obj)
        {
            return (LineType)obj.GetValue(Nav1NeedleLineTypeProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetNav1NeedleLineType(DependencyObject obj, LineType value)
        {
            obj.SetValue(Nav1NeedleLineTypeProperty, value);
        }

        private static void OnNav1NeedleLineTypeValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<LineType>(
              (LineType)args.OldValue, (LineType)args.NewValue, Nav1NeedleLineTypeValueChangedEvent);

            control.CreateNav1Needle();

            control.OnNav1NeedleLineTypeValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent Nav1NeedleLineTypeValueChangedEvent = EventManager.RegisterRoutedEvent(
          "Nav1NeedleLineTypeValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<LineType>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnNav1NeedleLineTypeValueChanged(RoutedPropertyChangedEventArgs<LineType> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region Nav12NeedleLineTypeProperty

        public static readonly DependencyProperty Nav12NeedleLineTypeProperty =
        DependencyProperty.RegisterAttached(
          "Nav12NeedleLineType",
          typeof(LineType),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(LineType.Single, new PropertyChangedCallback(OnNav12NeedleLineTypeValueChanged)));


        public LineType Nav12NeedleLineType
        {
            get => (LineType)GetValue(Nav12NeedleLineTypeProperty);
            set => SetValue(Nav12NeedleLineTypeProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static LineType GetNav12NeedleLineType(DependencyObject obj)
        {
            return (LineType)obj.GetValue(Nav12NeedleLineTypeProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetNav12NeedleLineType(DependencyObject obj, LineType value)
        {
            obj.SetValue(Nav12NeedleLineTypeProperty, value);
        }

        private static void OnNav12NeedleLineTypeValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<LineType>(
              (LineType)args.OldValue, (LineType)args.NewValue, Nav12NeedleLineTypeValueChangedEvent);

            control.CreateNav12Needle();

            control.OnNav12NeedleLineTypeValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent Nav12NeedleLineTypeValueChangedEvent = EventManager.RegisterRoutedEvent(
          "Nav12NeedleLineTypeValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<LineType>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnNav12NeedleLineTypeValueChanged(RoutedPropertyChangedEventArgs<LineType> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region Nav1ArrowTypeProperty

        public static readonly DependencyProperty Nav1ArrowTypeProperty =
        DependencyProperty.RegisterAttached(
          "Nav1ArrowType",
          typeof(ArrowType),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(ArrowType.Open, new PropertyChangedCallback(OnNav1ArrowTypeValueChanged)));


        public ArrowType Nav1ArrowType
        {
            get => (ArrowType)GetValue(Nav1ArrowTypeProperty);
            set => SetValue(Nav1ArrowTypeProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static ArrowType GetNav1ArrowType(DependencyObject obj)
        {
            return (ArrowType)obj.GetValue(Nav1ArrowTypeProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetNav1ArrowType(DependencyObject obj, ArrowType value)
        {
            obj.SetValue(Nav1ArrowTypeProperty, value);
        }

        private static void OnNav1ArrowTypeValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<ArrowType>(
              (ArrowType)args.OldValue, (ArrowType)args.NewValue, Nav1ArrowTypeValueChangedEvent);

            control.CreateNav1Needle();

            control.OnNav1ArrowTypeValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent Nav1ArrowTypeValueChangedEvent = EventManager.RegisterRoutedEvent(
          "Nav1ArrowTypeValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<ArrowType>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnNav1ArrowTypeValueChanged(RoutedPropertyChangedEventArgs<ArrowType> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region Nav12ArrowTypeProperty

        public static readonly DependencyProperty Nav12ArrowTypeProperty =
        DependencyProperty.RegisterAttached(
          "Nav12ArrowType",
          typeof(ArrowType),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(ArrowType.Open, new PropertyChangedCallback(OnNav12ArrowTypeValueChanged)));


        public ArrowType Nav12ArrowType
        {
            get => (ArrowType)GetValue(Nav12ArrowTypeProperty);
            set => SetValue(Nav12ArrowTypeProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static ArrowType GetNav12ArrowType(DependencyObject obj)
        {
            return (ArrowType)obj.GetValue(Nav12ArrowTypeProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetNav12ArrowType(DependencyObject obj, ArrowType value)
        {
            obj.SetValue(Nav12ArrowTypeProperty, value);
        }

        private static void OnNav12ArrowTypeValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<ArrowType>(
              (ArrowType)args.OldValue, (ArrowType)args.NewValue, Nav12ArrowTypeValueChangedEvent);

            control.CreateNav12Needle();

            control.OnNav12ArrowTypeValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent Nav12ArrowTypeValueChangedEvent = EventManager.RegisterRoutedEvent(
          "Nav12ArrowTypeValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<ArrowType>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnNav12ArrowTypeValueChanged(RoutedPropertyChangedEventArgs<ArrowType> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region Nav1NeedleLineThicknessProperty

        internal static readonly DependencyProperty Nav1NeedleLineThicknessProperty =
        DependencyProperty.RegisterAttached(
          "Nav1NeedleLineThickness",
          typeof(double),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(2.0, new PropertyChangedCallback(OnNav1NeedleLineThicknessValueChanged)));


        internal double Nav1NeedleLineThickness
        {
            get => (double)GetValue(Nav1NeedleLineThicknessProperty);
            set => SetValue(Nav1NeedleLineThicknessProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        internal static double GetNav1NeedleLineThickness(DependencyObject obj)
        {
            return (double)obj.GetValue(Nav1NeedleLineThicknessProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        internal static void SetNav1NeedleLineThickness(DependencyObject obj, double value)
        {
            obj.SetValue(Nav1NeedleLineThicknessProperty, value);
        }

        private static void OnNav1NeedleLineThicknessValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<double>(
              (double)args.OldValue, (double)args.NewValue, Nav1NeedleLineThicknessValueChangedEvent);

            control.OnNav1NeedleLineThicknessValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent Nav1NeedleLineThicknessValueChangedEvent = EventManager.RegisterRoutedEvent(
          "Nav1NeedleLineThicknessValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<double>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnNav1NeedleLineThicknessValueChanged(RoutedPropertyChangedEventArgs<double> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region Nav12NeedleLineThicknessProperty

        internal static readonly DependencyProperty Nav12NeedleLineThicknessProperty =
        DependencyProperty.RegisterAttached(
          "Nav12NeedleLineThickness",
          typeof(double),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(2.0, new PropertyChangedCallback(OnNav12NeedleLineThicknessValueChanged)));


        internal double Nav12NeedleLineThickness
        {
            get => (double)GetValue(Nav12NeedleLineThicknessProperty);
            set => SetValue(Nav12NeedleLineThicknessProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        internal static double GetNav12NeedleLineThickness(DependencyObject obj)
        {
            return (double)obj.GetValue(Nav12NeedleLineThicknessProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        internal static void SetNav12NeedleLineThickness(DependencyObject obj, double value)
        {
            obj.SetValue(Nav12NeedleLineThicknessProperty, value);
        }

        private static void OnNav12NeedleLineThicknessValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<double>(
              (double)args.OldValue, (double)args.NewValue, Nav12NeedleLineThicknessValueChangedEvent);

            control.OnNav12NeedleLineThicknessValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent Nav12NeedleLineThicknessValueChangedEvent = EventManager.RegisterRoutedEvent(
          "Nav12NeedleLineThicknessValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<double>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnNav12NeedleLineThicknessValueChanged(RoutedPropertyChangedEventArgs<double> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region Nav1DataProperty

        internal static readonly DependencyProperty Nav1DataProperty =
        DependencyProperty.RegisterAttached(
          "Nav1Data",
          typeof(System.Windows.Media.Geometry),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnNav1DataValueChanged)));


        internal System.Windows.Media.Geometry Nav1Data
        {
            get => (System.Windows.Media.Geometry)GetValue(Nav1DataProperty);
            set => SetValue(Nav1DataProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        internal static System.Windows.Media.Geometry GetNav1Data(DependencyObject obj)
        {
            return (System.Windows.Media.Geometry)obj.GetValue(Nav1DataProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        internal static void SetNav1Data(DependencyObject obj, System.Windows.Media.Geometry value)
        {
            obj.SetValue(Nav1DataProperty, value);
        }

        private static void OnNav1DataValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<System.Windows.Media.Geometry>(
              (System.Windows.Media.Geometry)args.OldValue, (System.Windows.Media.Geometry)args.NewValue, Nav1DataValueChangedEvent);

            control.OnNav1DataValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent Nav1DataValueChangedEvent = EventManager.RegisterRoutedEvent(
          "Nav1DataValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<System.Windows.Media.Geometry>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnNav1DataValueChanged(RoutedPropertyChangedEventArgs<System.Windows.Media.Geometry> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region Nav12DataProperty

        internal static readonly DependencyProperty Nav12DataProperty =
        DependencyProperty.RegisterAttached(
          "Nav12Data",
          typeof(System.Windows.Media.Geometry),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnNav12DataValueChanged)));


        internal System.Windows.Media.Geometry Nav12Data
        {
            get => (System.Windows.Media.Geometry)GetValue(Nav12DataProperty);
            set => SetValue(Nav12DataProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        internal static System.Windows.Media.Geometry GetNav12Data(DependencyObject obj)
        {
            return (System.Windows.Media.Geometry)obj.GetValue(Nav12DataProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        internal static void SetNav12Data(DependencyObject obj, System.Windows.Media.Geometry value)
        {
            obj.SetValue(Nav12DataProperty, value);
        }

        private static void OnNav12DataValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<System.Windows.Media.Geometry>(
              (System.Windows.Media.Geometry)args.OldValue, (System.Windows.Media.Geometry)args.NewValue, Nav12DataValueChangedEvent);

            control.OnNav12DataValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent Nav12DataValueChangedEvent = EventManager.RegisterRoutedEvent(
          "Nav12DataValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<System.Windows.Media.Geometry>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnNav12DataValueChanged(RoutedPropertyChangedEventArgs<System.Windows.Media.Geometry> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region Nav2VisibilityProperty

        public static readonly DependencyProperty Nav2VisibilityProperty =
        DependencyProperty.RegisterAttached(
          "Nav2Visibility",
          typeof(Visibility),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(Visibility.Hidden, new PropertyChangedCallback(OnNav2VisibilityValueChanged)));


        public Visibility Nav2Visibility
        {
            get => (Visibility)GetValue(Nav2VisibilityProperty);
            set => SetValue(Nav2VisibilityProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static Visibility GetNav2Visibility(DependencyObject obj)
        {
            return (Visibility)obj.GetValue(Nav2VisibilityProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetNav2Visibility(DependencyObject obj, Visibility value)
        {
            obj.SetValue(Nav2VisibilityProperty, value);
        }

        private static void OnNav2VisibilityValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<Visibility>(
              (Visibility)args.OldValue, (Visibility)args.NewValue, Nav2VisibilityValueChangedEvent);

            if (((Visibility)args.NewValue) == Visibility.Visible) control.CreateNav2Needle();
            control.OnNav2VisibilityValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent Nav2VisibilityValueChangedEvent = EventManager.RegisterRoutedEvent(
          "Nav2VisibilityValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<Visibility>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnNav2VisibilityValueChanged(RoutedPropertyChangedEventArgs<Visibility> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region Nav22VisibilityProperty

        public static readonly DependencyProperty Nav22VisibilityProperty =
        DependencyProperty.RegisterAttached(
          "Nav22Visibility",
          typeof(Visibility),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(Visibility.Hidden, new PropertyChangedCallback(OnNav22VisibilityValueChanged)));


        public Visibility Nav22Visibility
        {
            get => (Visibility)GetValue(Nav22VisibilityProperty);
            set => SetValue(Nav22VisibilityProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static Visibility GetNav22Visibility(DependencyObject obj)
        {
            return (Visibility)obj.GetValue(Nav22VisibilityProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetNav22Visibility(DependencyObject obj, Visibility value)
        {
            obj.SetValue(Nav22VisibilityProperty, value);
        }

        private static void OnNav22VisibilityValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<Visibility>(
              (Visibility)args.OldValue, (Visibility)args.NewValue, Nav22VisibilityValueChangedEvent);

            if (((Visibility)args.NewValue) == Visibility.Visible) control.CreateNav22Needle();
            control.OnNav22VisibilityValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent Nav22VisibilityValueChangedEvent = EventManager.RegisterRoutedEvent(
          "Nav22VisibilityValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<Visibility>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnNav22VisibilityValueChanged(RoutedPropertyChangedEventArgs<Visibility> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region Nav2NameProperty

        public static readonly DependencyProperty Nav2NameProperty =
        DependencyProperty.RegisterAttached(
          "Nav2Name",
          typeof(string),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(string.Empty, new PropertyChangedCallback(OnNav2NameValueChanged)));


        public string Nav2Name
        {
            get => (string)GetValue(Nav2NameProperty);
            set => SetValue(Nav2NameProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static string GetNav2Name(DependencyObject obj)
        {
            return (string)obj.GetValue(Nav2NameProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetNav2Name(DependencyObject obj, string value)
        {
            obj.SetValue(Nav2NameProperty, value);
        }

        private static void OnNav2NameValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<string>(
              (string)args.OldValue, (string)args.NewValue, Nav2NameValueChangedEvent);

            control.OnNav2NameValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent Nav2NameValueChangedEvent = EventManager.RegisterRoutedEvent(
          "Nav2NameValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<string>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnNav2NameValueChanged(RoutedPropertyChangedEventArgs<string> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region Nav22NameProperty

        public static readonly DependencyProperty Nav22NameProperty =
        DependencyProperty.RegisterAttached(
          "Nav22Name",
          typeof(string),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(string.Empty, new PropertyChangedCallback(OnNav22NameValueChanged)));


        public string Nav22Name
        {
            get => (string)GetValue(Nav22NameProperty);
            set => SetValue(Nav22NameProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static string GetNav22Name(DependencyObject obj)
        {
            return (string)obj.GetValue(Nav22NameProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetNav22Name(DependencyObject obj, string value)
        {
            obj.SetValue(Nav22NameProperty, value);
        }

        private static void OnNav22NameValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<string>(
              (string)args.OldValue, (string)args.NewValue, Nav22NameValueChangedEvent);

            control.OnNav22NameValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent Nav22NameValueChangedEvent = EventManager.RegisterRoutedEvent(
          "Nav22NameValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<string>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnNav22NameValueChanged(RoutedPropertyChangedEventArgs<string> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region Nav2BearingProperty

        public static readonly DependencyProperty Nav2BearingProperty =
        DependencyProperty.RegisterAttached(
          "Nav2Bearing",
          typeof(double),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback(OnNav2BearingValueChanged)));


        public double Nav2Bearing
        {
            get => (double)GetValue(Nav2BearingProperty);
            set => SetValue(Nav2BearingProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static double GetNav2Bearing(DependencyObject obj)
        {
            return (double)obj.GetValue(Nav2BearingProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetNav2Bearing(DependencyObject obj, double value)
        {
            obj.SetValue(Nav2BearingProperty, value);
        }

        private static void OnNav2BearingValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<double>(
              (double)args.OldValue, (double)args.NewValue, Nav2BearingValueChangedEvent);

            control.OnNav2BearingValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent Nav2BearingValueChangedEvent = EventManager.RegisterRoutedEvent(
          "Nav2BearingValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<double>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnNav2BearingValueChanged(RoutedPropertyChangedEventArgs<double> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region Nav22BearingProperty

        public static readonly DependencyProperty Nav22BearingProperty =
        DependencyProperty.RegisterAttached(
          "Nav22Bearing",
          typeof(double),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback(OnNav22BearingValueChanged)));


        public double Nav22Bearing
        {
            get => (double)GetValue(Nav22BearingProperty);
            set => SetValue(Nav22BearingProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static double GetNav22Bearing(DependencyObject obj)
        {
            return (double)obj.GetValue(Nav22BearingProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetNav22Bearing(DependencyObject obj, double value)
        {
            obj.SetValue(Nav22BearingProperty, value);
        }

        private static void OnNav22BearingValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<double>(
              (double)args.OldValue, (double)args.NewValue, Nav22BearingValueChangedEvent);

            control.OnNav22BearingValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent Nav22BearingValueChangedEvent = EventManager.RegisterRoutedEvent(
          "Nav22BearingValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<double>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnNav22BearingValueChanged(RoutedPropertyChangedEventArgs<double> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region Nav2ColorProperty

        public static readonly DependencyProperty Nav2ColorProperty =
        DependencyProperty.RegisterAttached(
          "Nav2Color",
          typeof(Brush),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(new SolidColorBrush(Colors.White), new PropertyChangedCallback(OnNav2ColorValueChanged)));


        public Brush Nav2Color
        {
            get => (Brush)GetValue(Nav2ColorProperty);
            set => SetValue(Nav2ColorProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static Brush GetNav2Color(DependencyObject obj)
        {
            return (Brush)obj.GetValue(Nav2ColorProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetNav2Color(DependencyObject obj, Brush value)
        {
            obj.SetValue(Nav2ColorProperty, value);
        }

        private static void OnNav2ColorValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<Brush>(
              (Brush)args.OldValue, (Brush)args.NewValue, Nav2ColorValueChangedEvent);

            control.OnNav2ColorValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent Nav2ColorValueChangedEvent = EventManager.RegisterRoutedEvent(
          "Nav2ColorValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<Brush>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnNav2ColorValueChanged(RoutedPropertyChangedEventArgs<Brush> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region Nav22ColorProperty

        public static readonly DependencyProperty Nav22ColorProperty =
        DependencyProperty.RegisterAttached(
          "Nav22Color",
          typeof(Brush),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(new SolidColorBrush(Colors.White), new PropertyChangedCallback(OnNav22ColorValueChanged)));


        public Brush Nav22Color
        {
            get => (Brush)GetValue(Nav22ColorProperty);
            set => SetValue(Nav22ColorProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static Brush GetNav22Color(DependencyObject obj)
        {
            return (Brush)obj.GetValue(Nav22ColorProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetNav22Color(DependencyObject obj, Brush value)
        {
            obj.SetValue(Nav22ColorProperty, value);
        }

        private static void OnNav22ColorValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<Brush>(
              (Brush)args.OldValue, (Brush)args.NewValue, Nav22ColorValueChangedEvent);

            control.OnNav22ColorValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent Nav22ColorValueChangedEvent = EventManager.RegisterRoutedEvent(
          "Nav22ColorValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<Brush>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnNav22ColorValueChanged(RoutedPropertyChangedEventArgs<Brush> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region Nav2NeedleLineTypeProperty

        public static readonly DependencyProperty Nav2NeedleLineTypeProperty =
        DependencyProperty.RegisterAttached(
          "Nav2NeedleLineType",
          typeof(LineType),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(LineType.Single, new PropertyChangedCallback(OnNav2NeedleLineTypeValueChanged)));


        public LineType Nav2NeedleLineType
        {
            get => (LineType)GetValue(Nav2NeedleLineTypeProperty);
            set => SetValue(Nav2NeedleLineTypeProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static LineType GetNav2NeedleLineType(DependencyObject obj)
        {
            return (LineType)obj.GetValue(Nav2NeedleLineTypeProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetNav2NeedleLineType(DependencyObject obj, LineType value)
        {
            obj.SetValue(Nav2NeedleLineTypeProperty, value);
        }

        private static void OnNav2NeedleLineTypeValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<LineType>(
              (LineType)args.OldValue, (LineType)args.NewValue, Nav2NeedleLineTypeValueChangedEvent);

            control.CreateNav2Needle();

            control.OnNav2NeedleLineTypeValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent Nav2NeedleLineTypeValueChangedEvent = EventManager.RegisterRoutedEvent(
          "Nav2NeedleLineTypeValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<LineType>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnNav2NeedleLineTypeValueChanged(RoutedPropertyChangedEventArgs<LineType> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region Nav22NeedleLineTypeProperty

        public static readonly DependencyProperty Nav22NeedleLineTypeProperty =
        DependencyProperty.RegisterAttached(
          "Nav22NeedleLineType",
          typeof(LineType),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(LineType.Single, new PropertyChangedCallback(OnNav22NeedleLineTypeValueChanged)));


        public LineType Nav22NeedleLineType
        {
            get => (LineType)GetValue(Nav22NeedleLineTypeProperty);
            set => SetValue(Nav22NeedleLineTypeProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static LineType GetNav22NeedleLineType(DependencyObject obj)
        {
            return (LineType)obj.GetValue(Nav22NeedleLineTypeProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetNav22NeedleLineType(DependencyObject obj, LineType value)
        {
            obj.SetValue(Nav22NeedleLineTypeProperty, value);
        }

        private static void OnNav22NeedleLineTypeValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<LineType>(
              (LineType)args.OldValue, (LineType)args.NewValue, Nav22NeedleLineTypeValueChangedEvent);

            control.CreateNav22Needle();

            control.OnNav22NeedleLineTypeValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent Nav22NeedleLineTypeValueChangedEvent = EventManager.RegisterRoutedEvent(
          "Nav22NeedleLineTypeValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<LineType>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnNav22NeedleLineTypeValueChanged(RoutedPropertyChangedEventArgs<LineType> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region Nav2ArrowTypeProperty

        public static readonly DependencyProperty Nav2ArrowTypeProperty =
        DependencyProperty.RegisterAttached(
          "Nav2ArrowType",
          typeof(ArrowType),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(ArrowType.Open, new PropertyChangedCallback(OnNav2ArrowTypeValueChanged)));


        public ArrowType Nav2ArrowType
        {
            get => (ArrowType)GetValue(Nav2ArrowTypeProperty);
            set => SetValue(Nav2ArrowTypeProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static ArrowType GetNav2ArrowType(DependencyObject obj)
        {
            return (ArrowType)obj.GetValue(Nav2ArrowTypeProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetNav2ArrowType(DependencyObject obj, ArrowType value)
        {
            obj.SetValue(Nav2ArrowTypeProperty, value);
        }

        private static void OnNav2ArrowTypeValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<ArrowType>(
              (ArrowType)args.OldValue, (ArrowType)args.NewValue, Nav2ArrowTypeValueChangedEvent);

            control.CreateNav2Needle();

            control.OnNav2ArrowTypeValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent Nav2ArrowTypeValueChangedEvent = EventManager.RegisterRoutedEvent(
          "Nav2ArrowTypeValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<ArrowType>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnNav2ArrowTypeValueChanged(RoutedPropertyChangedEventArgs<ArrowType> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region Nav22ArrowTypeProperty

        public static readonly DependencyProperty Nav22ArrowTypeProperty =
        DependencyProperty.RegisterAttached(
          "Nav22ArrowType",
          typeof(ArrowType),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(ArrowType.Open, new PropertyChangedCallback(OnNav22ArrowTypeValueChanged)));


        public ArrowType Nav22ArrowType
        {
            get => (ArrowType)GetValue(Nav22ArrowTypeProperty);
            set => SetValue(Nav22ArrowTypeProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static ArrowType GetNav22ArrowType(DependencyObject obj)
        {
            return (ArrowType)obj.GetValue(Nav22ArrowTypeProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetNav22ArrowType(DependencyObject obj, ArrowType value)
        {
            obj.SetValue(Nav22ArrowTypeProperty, value);
        }

        private static void OnNav22ArrowTypeValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<ArrowType>(
              (ArrowType)args.OldValue, (ArrowType)args.NewValue, Nav22ArrowTypeValueChangedEvent);

            control.CreateNav22Needle();

            control.OnNav22ArrowTypeValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent Nav22ArrowTypeValueChangedEvent = EventManager.RegisterRoutedEvent(
          "Nav22ArrowTypeValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<ArrowType>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnNav22ArrowTypeValueChanged(RoutedPropertyChangedEventArgs<ArrowType> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region Nav2NeedleLineThicknessProperty

        internal static readonly DependencyProperty Nav2NeedleLineThicknessProperty =
        DependencyProperty.RegisterAttached(
          "Nav2NeedleLineThickness",
          typeof(double),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(2.0, new PropertyChangedCallback(OnNav2NeedleLineThicknessValueChanged)));


        internal double Nav2NeedleLineThickness
        {
            get => (double)GetValue(Nav2NeedleLineThicknessProperty);
            set => SetValue(Nav2NeedleLineThicknessProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        internal static double GetNav2NeedleLineThickness(DependencyObject obj)
        {
            return (double)obj.GetValue(Nav2NeedleLineThicknessProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        internal static void SetNav2NeedleLineThickness(DependencyObject obj, double value)
        {
            obj.SetValue(Nav2NeedleLineThicknessProperty, value);
        }

        private static void OnNav2NeedleLineThicknessValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<double>(
              (double)args.OldValue, (double)args.NewValue, Nav2NeedleLineThicknessValueChangedEvent);

            control.OnNav2NeedleLineThicknessValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent Nav2NeedleLineThicknessValueChangedEvent = EventManager.RegisterRoutedEvent(
          "Nav2NeedleLineThicknessValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<double>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnNav2NeedleLineThicknessValueChanged(RoutedPropertyChangedEventArgs<double> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region Nav22NeedleLineThicknessProperty

        internal static readonly DependencyProperty Nav22NeedleLineThicknessProperty =
        DependencyProperty.RegisterAttached(
          "Nav22NeedleLineThickness",
          typeof(double),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(2.0, new PropertyChangedCallback(OnNav22NeedleLineThicknessValueChanged)));


        internal double Nav22NeedleLineThickness
        {
            get => (double)GetValue(Nav22NeedleLineThicknessProperty);
            set => SetValue(Nav22NeedleLineThicknessProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        internal static double GetNav22NeedleLineThickness(DependencyObject obj)
        {
            return (double)obj.GetValue(Nav22NeedleLineThicknessProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        internal static void SetNav22NeedleLineThickness(DependencyObject obj, double value)
        {
            obj.SetValue(Nav22NeedleLineThicknessProperty, value);
        }

        private static void OnNav22NeedleLineThicknessValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<double>(
              (double)args.OldValue, (double)args.NewValue, Nav22NeedleLineThicknessValueChangedEvent);

            control.OnNav22NeedleLineThicknessValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent Nav22NeedleLineThicknessValueChangedEvent = EventManager.RegisterRoutedEvent(
          "Nav22NeedleLineThicknessValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<double>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnNav22NeedleLineThicknessValueChanged(RoutedPropertyChangedEventArgs<double> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region Nav2DataProperty

        internal static readonly DependencyProperty Nav2DataProperty =
        DependencyProperty.RegisterAttached(
          "Nav2Data",
          typeof(System.Windows.Media.Geometry),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnNav2DataValueChanged)));


        internal System.Windows.Media.Geometry Nav2Data
        {
            get => (System.Windows.Media.Geometry)GetValue(Nav2DataProperty);
            set => SetValue(Nav2DataProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        internal static System.Windows.Media.Geometry GetNav2Data(DependencyObject obj)
        {
            return (System.Windows.Media.Geometry)obj.GetValue(Nav2DataProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        internal static void SetNav2Data(DependencyObject obj, System.Windows.Media.Geometry value)
        {
            obj.SetValue(Nav2DataProperty, value);
        }

        private static void OnNav2DataValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<System.Windows.Media.Geometry>(
              (System.Windows.Media.Geometry)args.OldValue, (System.Windows.Media.Geometry)args.NewValue, Nav2DataValueChangedEvent);

            control.OnNav2DataValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent Nav2DataValueChangedEvent = EventManager.RegisterRoutedEvent(
          "Nav2DataValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<System.Windows.Media.Geometry>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnNav2DataValueChanged(RoutedPropertyChangedEventArgs<System.Windows.Media.Geometry> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region Nav22DataProperty

        internal static readonly DependencyProperty Nav22DataProperty =
        DependencyProperty.RegisterAttached(
          "Nav22Data",
          typeof(System.Windows.Media.Geometry),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnNav22DataValueChanged)));


        internal System.Windows.Media.Geometry Nav22Data
        {
            get => (System.Windows.Media.Geometry)GetValue(Nav22DataProperty);
            set => SetValue(Nav22DataProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        internal static System.Windows.Media.Geometry GetNav22Data(DependencyObject obj)
        {
            return (System.Windows.Media.Geometry)obj.GetValue(Nav22DataProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        internal static void SetNav22Data(DependencyObject obj, System.Windows.Media.Geometry value)
        {
            obj.SetValue(Nav22DataProperty, value);
        }

        private static void OnNav22DataValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<System.Windows.Media.Geometry>(
              (System.Windows.Media.Geometry)args.OldValue, (System.Windows.Media.Geometry)args.NewValue, Nav22DataValueChangedEvent);

            control.OnNav22DataValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent Nav22DataValueChangedEvent = EventManager.RegisterRoutedEvent(
          "Nav22DataValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<System.Windows.Media.Geometry>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnNav22DataValueChanged(RoutedPropertyChangedEventArgs<System.Windows.Media.Geometry> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region Nav3VisibilityProperty

        public static readonly DependencyProperty Nav3VisibilityProperty =
        DependencyProperty.RegisterAttached(
          "Nav3Visibility",
          typeof(Visibility),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(Visibility.Hidden, new PropertyChangedCallback(OnNav3VisibilityValueChanged)));


        public Visibility Nav3Visibility
        {
            get => (Visibility)GetValue(Nav3VisibilityProperty);
            set => SetValue(Nav3VisibilityProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static Visibility GetNav3Visibility(DependencyObject obj)
        {
            return (Visibility)obj.GetValue(Nav3VisibilityProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetNav3Visibility(DependencyObject obj, Visibility value)
        {
            obj.SetValue(Nav3VisibilityProperty, value);
        }

        private static void OnNav3VisibilityValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<Visibility>(
              (Visibility)args.OldValue, (Visibility)args.NewValue, Nav3VisibilityValueChangedEvent);

            if (((Visibility)args.NewValue) == Visibility.Visible) control.CreateNav3Needle();
            control.OnNav3VisibilityValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent Nav3VisibilityValueChangedEvent = EventManager.RegisterRoutedEvent(
          "Nav3VisibilityValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<Visibility>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnNav3VisibilityValueChanged(RoutedPropertyChangedEventArgs<Visibility> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region Nav32VisibilityProperty

        public static readonly DependencyProperty Nav32VisibilityProperty =
        DependencyProperty.RegisterAttached(
          "Nav32Visibility",
          typeof(Visibility),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(Visibility.Hidden, new PropertyChangedCallback(OnNav32VisibilityValueChanged)));


        public Visibility Nav32Visibility
        {
            get => (Visibility)GetValue(Nav32VisibilityProperty);
            set => SetValue(Nav32VisibilityProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static Visibility GetNav32Visibility(DependencyObject obj)
        {
            return (Visibility)obj.GetValue(Nav32VisibilityProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetNav32Visibility(DependencyObject obj, Visibility value)
        {
            obj.SetValue(Nav32VisibilityProperty, value);
        }

        private static void OnNav32VisibilityValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<Visibility>(
              (Visibility)args.OldValue, (Visibility)args.NewValue, Nav32VisibilityValueChangedEvent);

            if (((Visibility)args.NewValue) == Visibility.Visible) control.CreateNav32Needle();
            control.OnNav32VisibilityValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent Nav32VisibilityValueChangedEvent = EventManager.RegisterRoutedEvent(
          "Nav32VisibilityValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<Visibility>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnNav32VisibilityValueChanged(RoutedPropertyChangedEventArgs<Visibility> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region Nav3NameProperty

        public static readonly DependencyProperty Nav3NameProperty =
        DependencyProperty.RegisterAttached(
          "Nav3Name",
          typeof(string),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(string.Empty, new PropertyChangedCallback(OnNav3NameValueChanged)));


        public string Nav3Name
        {
            get => (string)GetValue(Nav3NameProperty);
            set => SetValue(Nav3NameProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static string GetNav3Name(DependencyObject obj)
        {
            return (string)obj.GetValue(Nav3NameProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetNav3Name(DependencyObject obj, string value)
        {
            obj.SetValue(Nav3NameProperty, value);
        }

        private static void OnNav3NameValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<string>(
              (string)args.OldValue, (string)args.NewValue, Nav3NameValueChangedEvent);

            control.OnNav3NameValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent Nav3NameValueChangedEvent = EventManager.RegisterRoutedEvent(
          "Nav3NameValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<string>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnNav3NameValueChanged(RoutedPropertyChangedEventArgs<string> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region Nav32NameProperty

        public static readonly DependencyProperty Nav32NameProperty =
        DependencyProperty.RegisterAttached(
          "Nav32Name",
          typeof(string),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(string.Empty, new PropertyChangedCallback(OnNav32NameValueChanged)));


        public string Nav32Name
        {
            get => (string)GetValue(Nav32NameProperty);
            set => SetValue(Nav32NameProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static string GetNav32Name(DependencyObject obj)
        {
            return (string)obj.GetValue(Nav32NameProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetNav32Name(DependencyObject obj, string value)
        {
            obj.SetValue(Nav32NameProperty, value);
        }

        private static void OnNav32NameValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<string>(
              (string)args.OldValue, (string)args.NewValue, Nav32NameValueChangedEvent);

            control.OnNav32NameValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent Nav32NameValueChangedEvent = EventManager.RegisterRoutedEvent(
          "Nav32NameValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<string>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnNav32NameValueChanged(RoutedPropertyChangedEventArgs<string> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region Nav3BearingProperty

        public static readonly DependencyProperty Nav3BearingProperty =
        DependencyProperty.RegisterAttached(
          "Nav3Bearing",
          typeof(double),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback(OnNav3BearingValueChanged)));


        public double Nav3Bearing
        {
            get => (double)GetValue(Nav3BearingProperty);
            set => SetValue(Nav3BearingProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static double GetNav3Bearing(DependencyObject obj)
        {
            return (double)obj.GetValue(Nav3BearingProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetNav3Bearing(DependencyObject obj, double value)
        {
            obj.SetValue(Nav3BearingProperty, value);
        }

        private static void OnNav3BearingValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<double>(
              (double)args.OldValue, (double)args.NewValue, Nav3BearingValueChangedEvent);

            control.OnNav3BearingValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent Nav3BearingValueChangedEvent = EventManager.RegisterRoutedEvent(
          "Nav3BearingValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<double>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnNav3BearingValueChanged(RoutedPropertyChangedEventArgs<double> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region Nav32BearingProperty

        public static readonly DependencyProperty Nav32BearingProperty =
        DependencyProperty.RegisterAttached(
          "Nav32Bearing",
          typeof(double),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback(OnNav32BearingValueChanged)));


        public double Nav32Bearing
        {
            get => (double)GetValue(Nav32BearingProperty);
            set => SetValue(Nav32BearingProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static double GetNav32Bearing(DependencyObject obj)
        {
            return (double)obj.GetValue(Nav32BearingProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetNav32Bearing(DependencyObject obj, double value)
        {
            obj.SetValue(Nav32BearingProperty, value);
        }

        private static void OnNav32BearingValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<double>(
              (double)args.OldValue, (double)args.NewValue, Nav32BearingValueChangedEvent);

            control.OnNav32BearingValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent Nav32BearingValueChangedEvent = EventManager.RegisterRoutedEvent(
          "Nav32BearingValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<double>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnNav32BearingValueChanged(RoutedPropertyChangedEventArgs<double> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region Nav3ColorProperty

        public static readonly DependencyProperty Nav3ColorProperty =
        DependencyProperty.RegisterAttached(
          "Nav3Color",
          typeof(Brush),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(new SolidColorBrush(Colors.White), new PropertyChangedCallback(OnNav3ColorValueChanged)));


        public Brush Nav3Color
        {
            get => (Brush)GetValue(Nav3ColorProperty);
            set => SetValue(Nav3ColorProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static Brush GetNav3Color(DependencyObject obj)
        {
            return (Brush)obj.GetValue(Nav3ColorProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetNav3Color(DependencyObject obj, Brush value)
        {
            obj.SetValue(Nav3ColorProperty, value);
        }

        private static void OnNav3ColorValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<Brush>(
              (Brush)args.OldValue, (Brush)args.NewValue, Nav3ColorValueChangedEvent);

            control.OnNav3ColorValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent Nav3ColorValueChangedEvent = EventManager.RegisterRoutedEvent(
          "Nav3ColorValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<Brush>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnNav3ColorValueChanged(RoutedPropertyChangedEventArgs<Brush> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region Nav32ColorProperty

        public static readonly DependencyProperty Nav32ColorProperty =
        DependencyProperty.RegisterAttached(
          "Nav32Color",
          typeof(Brush),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(new SolidColorBrush(Colors.White), new PropertyChangedCallback(OnNav32ColorValueChanged)));


        public Brush Nav32Color
        {
            get => (Brush)GetValue(Nav32ColorProperty);
            set => SetValue(Nav32ColorProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static Brush GetNav32Color(DependencyObject obj)
        {
            return (Brush)obj.GetValue(Nav32ColorProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetNav32Color(DependencyObject obj, Brush value)
        {
            obj.SetValue(Nav32ColorProperty, value);
        }

        private static void OnNav32ColorValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<Brush>(
              (Brush)args.OldValue, (Brush)args.NewValue, Nav32ColorValueChangedEvent);

            control.OnNav32ColorValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent Nav32ColorValueChangedEvent = EventManager.RegisterRoutedEvent(
          "Nav32ColorValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<Brush>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnNav32ColorValueChanged(RoutedPropertyChangedEventArgs<Brush> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region Nav3NeedleLineTypeProperty

        public static readonly DependencyProperty Nav3NeedleLineTypeProperty =
        DependencyProperty.RegisterAttached(
          "Nav3NeedleLineType",
          typeof(LineType),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(LineType.Single, new PropertyChangedCallback(OnNav3NeedleLineTypeValueChanged)));


        public LineType Nav3NeedleLineType
        {
            get => (LineType)GetValue(Nav3NeedleLineTypeProperty);
            set => SetValue(Nav3NeedleLineTypeProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static LineType GetNav3NeedleLineType(DependencyObject obj)
        {
            return (LineType)obj.GetValue(Nav3NeedleLineTypeProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetNav3NeedleLineType(DependencyObject obj, LineType value)
        {
            obj.SetValue(Nav3NeedleLineTypeProperty, value);
        }

        private static void OnNav3NeedleLineTypeValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<LineType>(
              (LineType)args.OldValue, (LineType)args.NewValue, Nav3NeedleLineTypeValueChangedEvent);

            control.CreateNav3Needle();

            control.OnNav3NeedleLineTypeValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent Nav3NeedleLineTypeValueChangedEvent = EventManager.RegisterRoutedEvent(
          "Nav3NeedleLineTypeValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<LineType>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnNav3NeedleLineTypeValueChanged(RoutedPropertyChangedEventArgs<LineType> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region Nav32NeedleLineTypeProperty

        public static readonly DependencyProperty Nav32NeedleLineTypeProperty =
        DependencyProperty.RegisterAttached(
          "Nav32NeedleLineType",
          typeof(LineType),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(LineType.Single, new PropertyChangedCallback(OnNav32NeedleLineTypeValueChanged)));


        public LineType Nav32NeedleLineType
        {
            get => (LineType)GetValue(Nav32NeedleLineTypeProperty);
            set => SetValue(Nav32NeedleLineTypeProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static LineType GetNav32NeedleLineType(DependencyObject obj)
        {
            return (LineType)obj.GetValue(Nav32NeedleLineTypeProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetNav32NeedleLineType(DependencyObject obj, LineType value)
        {
            obj.SetValue(Nav32NeedleLineTypeProperty, value);
        }

        private static void OnNav32NeedleLineTypeValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<LineType>(
              (LineType)args.OldValue, (LineType)args.NewValue, Nav32NeedleLineTypeValueChangedEvent);

            control.CreateNav32Needle();

            control.OnNav32NeedleLineTypeValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent Nav32NeedleLineTypeValueChangedEvent = EventManager.RegisterRoutedEvent(
          "Nav32NeedleLineTypeValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<LineType>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnNav32NeedleLineTypeValueChanged(RoutedPropertyChangedEventArgs<LineType> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region Nav3ArrowTypeProperty

        public static readonly DependencyProperty Nav3ArrowTypeProperty =
        DependencyProperty.RegisterAttached(
          "Nav3ArrowType",
          typeof(ArrowType),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(ArrowType.Open, new PropertyChangedCallback(OnNav3ArrowTypeValueChanged)));


        public ArrowType Nav3ArrowType
        {
            get => (ArrowType)GetValue(Nav3ArrowTypeProperty);
            set => SetValue(Nav3ArrowTypeProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static ArrowType GetNav3ArrowType(DependencyObject obj)
        {
            return (ArrowType)obj.GetValue(Nav3ArrowTypeProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetNav3ArrowType(DependencyObject obj, ArrowType value)
        {
            obj.SetValue(Nav3ArrowTypeProperty, value);
        }

        private static void OnNav3ArrowTypeValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<ArrowType>(
              (ArrowType)args.OldValue, (ArrowType)args.NewValue, Nav3ArrowTypeValueChangedEvent);

            control.CreateNav3Needle();

            control.OnNav3ArrowTypeValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent Nav3ArrowTypeValueChangedEvent = EventManager.RegisterRoutedEvent(
          "Nav3ArrowTypeValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<ArrowType>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnNav3ArrowTypeValueChanged(RoutedPropertyChangedEventArgs<ArrowType> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region Nav32ArrowTypeProperty

        public static readonly DependencyProperty Nav32ArrowTypeProperty =
        DependencyProperty.RegisterAttached(
          "Nav32ArrowType",
          typeof(ArrowType),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(ArrowType.Open, new PropertyChangedCallback(OnNav32ArrowTypeValueChanged)));


        public ArrowType Nav32ArrowType
        {
            get => (ArrowType)GetValue(Nav32ArrowTypeProperty);
            set => SetValue(Nav32ArrowTypeProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static ArrowType GetNav32ArrowType(DependencyObject obj)
        {
            return (ArrowType)obj.GetValue(Nav32ArrowTypeProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetNav32ArrowType(DependencyObject obj, ArrowType value)
        {
            obj.SetValue(Nav32ArrowTypeProperty, value);
        }

        private static void OnNav32ArrowTypeValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<ArrowType>(
              (ArrowType)args.OldValue, (ArrowType)args.NewValue, Nav32ArrowTypeValueChangedEvent);

            control.CreateNav32Needle();

            control.OnNav32ArrowTypeValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent Nav32ArrowTypeValueChangedEvent = EventManager.RegisterRoutedEvent(
          "Nav32ArrowTypeValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<ArrowType>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnNav32ArrowTypeValueChanged(RoutedPropertyChangedEventArgs<ArrowType> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region Nav3NeedleLineThicknessProperty

        internal static readonly DependencyProperty Nav3NeedleLineThicknessProperty =
        DependencyProperty.RegisterAttached(
          "Nav3NeedleLineThickness",
          typeof(double),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(2.0, new PropertyChangedCallback(OnNav3NeedleLineThicknessValueChanged)));


        internal double Nav3NeedleLineThickness
        {
            get => (double)GetValue(Nav3NeedleLineThicknessProperty);
            set => SetValue(Nav3NeedleLineThicknessProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        internal static double GetNav3NeedleLineThickness(DependencyObject obj)
        {
            return (double)obj.GetValue(Nav3NeedleLineThicknessProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        internal static void SetNav3NeedleLineThickness(DependencyObject obj, double value)
        {
            obj.SetValue(Nav3NeedleLineThicknessProperty, value);
        }

        private static void OnNav3NeedleLineThicknessValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<double>(
              (double)args.OldValue, (double)args.NewValue, Nav3NeedleLineThicknessValueChangedEvent);

            control.OnNav3NeedleLineThicknessValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent Nav3NeedleLineThicknessValueChangedEvent = EventManager.RegisterRoutedEvent(
          "Nav3NeedleLineThicknessValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<double>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnNav3NeedleLineThicknessValueChanged(RoutedPropertyChangedEventArgs<double> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region Nav32NeedleLineThicknessProperty

        internal static readonly DependencyProperty Nav32NeedleLineThicknessProperty =
        DependencyProperty.RegisterAttached(
          "Nav32NeedleLineThickness",
          typeof(double),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(2.0, new PropertyChangedCallback(OnNav32NeedleLineThicknessValueChanged)));


        internal double Nav32NeedleLineThickness
        {
            get => (double)GetValue(Nav32NeedleLineThicknessProperty);
            set => SetValue(Nav32NeedleLineThicknessProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        internal static double GetNav32NeedleLineThickness(DependencyObject obj)
        {
            return (double)obj.GetValue(Nav32NeedleLineThicknessProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        internal static void SetNav32NeedleLineThickness(DependencyObject obj, double value)
        {
            obj.SetValue(Nav32NeedleLineThicknessProperty, value);
        }

        private static void OnNav32NeedleLineThicknessValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<double>(
              (double)args.OldValue, (double)args.NewValue, Nav32NeedleLineThicknessValueChangedEvent);

            control.OnNav32NeedleLineThicknessValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent Nav32NeedleLineThicknessValueChangedEvent = EventManager.RegisterRoutedEvent(
          "Nav32NeedleLineThicknessValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<double>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnNav32NeedleLineThicknessValueChanged(RoutedPropertyChangedEventArgs<double> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region Nav3DataProperty

        internal static readonly DependencyProperty Nav3DataProperty =
        DependencyProperty.RegisterAttached(
          "Nav3Data",
          typeof(System.Windows.Media.Geometry),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnNav3DataValueChanged)));


        internal System.Windows.Media.Geometry Nav3Data
        {
            get => (System.Windows.Media.Geometry)GetValue(Nav3DataProperty);
            set => SetValue(Nav3DataProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        internal static System.Windows.Media.Geometry GetNav3Data(DependencyObject obj)
        {
            return (System.Windows.Media.Geometry)obj.GetValue(Nav3DataProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        internal static void SetNav3Data(DependencyObject obj, System.Windows.Media.Geometry value)
        {
            obj.SetValue(Nav3DataProperty, value);
        }

        private static void OnNav3DataValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<System.Windows.Media.Geometry>(
              (System.Windows.Media.Geometry)args.OldValue, (System.Windows.Media.Geometry)args.NewValue, Nav3DataValueChangedEvent);

            control.OnNav3DataValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent Nav3DataValueChangedEvent = EventManager.RegisterRoutedEvent(
          "Nav3DataValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<System.Windows.Media.Geometry>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnNav3DataValueChanged(RoutedPropertyChangedEventArgs<System.Windows.Media.Geometry> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region Nav32DataProperty

        internal static readonly DependencyProperty Nav32DataProperty =
        DependencyProperty.RegisterAttached(
          "Nav32Data",
          typeof(System.Windows.Media.Geometry),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnNav32DataValueChanged)));


        internal System.Windows.Media.Geometry Nav32Data
        {
            get => (System.Windows.Media.Geometry)GetValue(Nav32DataProperty);
            set => SetValue(Nav32DataProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        internal static System.Windows.Media.Geometry GetNav32Data(DependencyObject obj)
        {
            return (System.Windows.Media.Geometry)obj.GetValue(Nav32DataProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        internal static void SetNav32Data(DependencyObject obj, System.Windows.Media.Geometry value)
        {
            obj.SetValue(Nav3DataProperty, value);
        }

        private static void OnNav32DataValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<System.Windows.Media.Geometry>(
              (System.Windows.Media.Geometry)args.OldValue, (System.Windows.Media.Geometry)args.NewValue, Nav32DataValueChangedEvent);

            control.OnNav32DataValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent Nav32DataValueChangedEvent = EventManager.RegisterRoutedEvent(
          "Nav32DataValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<System.Windows.Media.Geometry>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnNav32DataValueChanged(RoutedPropertyChangedEventArgs<System.Windows.Media.Geometry> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region Nav4VisibilityProperty

        public static readonly DependencyProperty Nav4VisibilityProperty =
        DependencyProperty.RegisterAttached(
          "Nav4Visibility",
          typeof(Visibility),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(Visibility.Hidden, new PropertyChangedCallback(OnNav4VisibilityValueChanged)));


        public Visibility Nav4Visibility
        {
            get => (Visibility)GetValue(Nav4VisibilityProperty);
            set => SetValue(Nav4VisibilityProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static Visibility GetNav4Visibility(DependencyObject obj)
        {
            return (Visibility)obj.GetValue(Nav4VisibilityProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetNav4Visibility(DependencyObject obj, Visibility value)
        {
            obj.SetValue(Nav4VisibilityProperty, value);
        }

        private static void OnNav4VisibilityValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<Visibility>(
              (Visibility)args.OldValue, (Visibility)args.NewValue, Nav4VisibilityValueChangedEvent);

            if (((Visibility)args.NewValue) == Visibility.Visible) control.CreateNav4Needle();

            control.OnNav4VisibilityValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent Nav4VisibilityValueChangedEvent = EventManager.RegisterRoutedEvent(
          "Nav4VisibilityValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<Visibility>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnNav4VisibilityValueChanged(RoutedPropertyChangedEventArgs<Visibility> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region Nav42VisibilityProperty

        public static readonly DependencyProperty Nav42VisibilityProperty =
        DependencyProperty.RegisterAttached(
          "Nav42Visibility",
          typeof(Visibility),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(Visibility.Hidden, new PropertyChangedCallback(OnNav42VisibilityValueChanged)));


        public Visibility Nav42Visibility
        {
            get => (Visibility)GetValue(Nav42VisibilityProperty);
            set => SetValue(Nav42VisibilityProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static Visibility GetNav42Visibility(DependencyObject obj)
        {
            return (Visibility)obj.GetValue(Nav42VisibilityProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetNav42Visibility(DependencyObject obj, Visibility value)
        {
            obj.SetValue(Nav42VisibilityProperty, value);
        }

        private static void OnNav42VisibilityValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<Visibility>(
              (Visibility)args.OldValue, (Visibility)args.NewValue, Nav42VisibilityValueChangedEvent);


            if (((Visibility)args.NewValue) == Visibility.Visible) control.CreateNav42Needle();
            control.OnNav42VisibilityValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent Nav42VisibilityValueChangedEvent = EventManager.RegisterRoutedEvent(
          "Nav42VisibilityValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<Visibility>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnNav42VisibilityValueChanged(RoutedPropertyChangedEventArgs<Visibility> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region Nav1NameVisibilityProperty

        public static readonly DependencyProperty Nav1NameVisibilityProperty =
        DependencyProperty.RegisterAttached(
          "Nav1NameVisibility",
          typeof(Visibility),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(Visibility.Hidden, new PropertyChangedCallback(OnNav1NameVisibilityValueChanged)));


        public Visibility Nav1NameVisibility
        {
            get => (Visibility)GetValue(Nav1NameVisibilityProperty);
            set => SetValue(Nav1NameVisibilityProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static Visibility GetNav1NameVisibility(DependencyObject obj)
        {
            return (Visibility)obj.GetValue(Nav1NameVisibilityProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetNav1NameVisibility(DependencyObject obj, Visibility value)
        {
            obj.SetValue(Nav1NameVisibilityProperty, value);
        }

        private static void OnNav1NameVisibilityValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<Visibility>(
              (Visibility)args.OldValue, (Visibility)args.NewValue, Nav1NameVisibilityValueChangedEvent);

            if (((Visibility)args.NewValue) == Visibility.Visible) control.CreateNav1Needle();
            control.OnNav1NameVisibilityValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent Nav1NameVisibilityValueChangedEvent = EventManager.RegisterRoutedEvent(
          "Nav1NameVisibilityValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<Visibility>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnNav1NameVisibilityValueChanged(RoutedPropertyChangedEventArgs<Visibility> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region Nav2NameVisibilityProperty

        public static readonly DependencyProperty Nav2NameVisibilityProperty =
        DependencyProperty.RegisterAttached(
          "Nav2NameVisibility",
          typeof(Visibility),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(Visibility.Hidden, new PropertyChangedCallback(OnNav2NameVisibilityValueChanged)));


        public Visibility Nav2NameVisibility
        {
            get => (Visibility)GetValue(Nav2NameVisibilityProperty);
            set => SetValue(Nav2NameVisibilityProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static Visibility GetNav2NameVisibility(DependencyObject obj)
        {
            return (Visibility)obj.GetValue(Nav2NameVisibilityProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetNav2NameVisibility(DependencyObject obj, Visibility value)
        {
            obj.SetValue(Nav2NameVisibilityProperty, value);
        }

        private static void OnNav2NameVisibilityValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<Visibility>(
              (Visibility)args.OldValue, (Visibility)args.NewValue, Nav2NameVisibilityValueChangedEvent);

            if (((Visibility)args.NewValue) == Visibility.Visible) control.CreateNav2Needle();
            control.OnNav2NameVisibilityValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent Nav2NameVisibilityValueChangedEvent = EventManager.RegisterRoutedEvent(
          "Nav2NameVisibilityValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<Visibility>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnNav2NameVisibilityValueChanged(RoutedPropertyChangedEventArgs<Visibility> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region Nav3NameVisibilityProperty

        public static readonly DependencyProperty Nav3NameVisibilityProperty =
        DependencyProperty.RegisterAttached(
          "Nav3NameVisibility",
          typeof(Visibility),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(Visibility.Hidden, new PropertyChangedCallback(OnNav3NameVisibilityValueChanged)));


        public Visibility Nav3NameVisibility
        {
            get => (Visibility)GetValue(Nav3NameVisibilityProperty);
            set => SetValue(Nav3NameVisibilityProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static Visibility GetNav3NameVisibility(DependencyObject obj)
        {
            return (Visibility)obj.GetValue(Nav3NameVisibilityProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetNav3NameVisibility(DependencyObject obj, Visibility value)
        {
            obj.SetValue(Nav3NameVisibilityProperty, value);
        }

        private static void OnNav3NameVisibilityValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<Visibility>(
              (Visibility)args.OldValue, (Visibility)args.NewValue, Nav3NameVisibilityValueChangedEvent);

            if (((Visibility)args.NewValue) == Visibility.Visible) control.CreateNav3Needle();
            control.OnNav3NameVisibilityValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent Nav3NameVisibilityValueChangedEvent = EventManager.RegisterRoutedEvent(
          "Nav3NameVisibilityValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<Visibility>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnNav3NameVisibilityValueChanged(RoutedPropertyChangedEventArgs<Visibility> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region Nav4NameVisibilityProperty

        public static readonly DependencyProperty Nav4NameVisibilityProperty =
        DependencyProperty.RegisterAttached(
          "Nav4NameVisibility",
          typeof(Visibility),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(Visibility.Hidden, new PropertyChangedCallback(OnNav4NameVisibilityValueChanged)));


        public Visibility Nav4NameVisibility
        {
            get => (Visibility)GetValue(Nav4NameVisibilityProperty);
            set => SetValue(Nav4NameVisibilityProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static Visibility GetNav4NameVisibility(DependencyObject obj)
        {
            return (Visibility)obj.GetValue(Nav4NameVisibilityProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetNav4NameVisibility(DependencyObject obj, Visibility value)
        {
            obj.SetValue(Nav4NameVisibilityProperty, value);
        }

        private static void OnNav4NameVisibilityValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<Visibility>(
              (Visibility)args.OldValue, (Visibility)args.NewValue, Nav4NameVisibilityValueChangedEvent);

            if (((Visibility)args.NewValue) == Visibility.Visible) control.CreateNav4Needle();
            control.OnNav4NameVisibilityValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent Nav4NameVisibilityValueChangedEvent = EventManager.RegisterRoutedEvent(
          "Nav4NameVisibilityValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<Visibility>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnNav4NameVisibilityValueChanged(RoutedPropertyChangedEventArgs<Visibility> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region Nav4NameProperty

        public static readonly DependencyProperty Nav4NameProperty =
        DependencyProperty.RegisterAttached(
          "Nav4Name",
          typeof(string),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(string.Empty, new PropertyChangedCallback(OnNav4NameValueChanged)));


        public string Nav4Name
        {
            get => (string)GetValue(Nav4NameProperty);
            set => SetValue(Nav4NameProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static string GetNav4Name(DependencyObject obj)
        {
            return (string)obj.GetValue(Nav4NameProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetNav4Name(DependencyObject obj, string value)
        {
            obj.SetValue(Nav4NameProperty, value);
        }

        private static void OnNav4NameValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<string>(
              (string)args.OldValue, (string)args.NewValue, Nav4NameValueChangedEvent);

            control.OnNav4NameValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent Nav4NameValueChangedEvent = EventManager.RegisterRoutedEvent(
          "Nav4NameValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<string>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnNav4NameValueChanged(RoutedPropertyChangedEventArgs<string> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region Nav42NameProperty

        public static readonly DependencyProperty Nav42NameProperty =
        DependencyProperty.RegisterAttached(
          "Nav42Name",
          typeof(string),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(string.Empty, new PropertyChangedCallback(OnNav42NameValueChanged)));


        public string Nav42Name
        {
            get => (string)GetValue(Nav42NameProperty);
            set => SetValue(Nav42NameProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static string GetNav42Name(DependencyObject obj)
        {
            return (string)obj.GetValue(Nav42NameProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetNav42Name(DependencyObject obj, string value)
        {
            obj.SetValue(Nav42NameProperty, value);
        }

        private static void OnNav42NameValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<string>(
              (string)args.OldValue, (string)args.NewValue, Nav42NameValueChangedEvent);

            control.OnNav42NameValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent Nav42NameValueChangedEvent = EventManager.RegisterRoutedEvent(
          "Nav42NameValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<string>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnNav42NameValueChanged(RoutedPropertyChangedEventArgs<string> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region Nav4BearingProperty

        public static readonly DependencyProperty Nav4BearingProperty =
        DependencyProperty.RegisterAttached(
          "Nav4Bearing",
          typeof(double),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback(OnNav4BearingValueChanged)));


        public double Nav4Bearing
        {
            get => (double)GetValue(Nav4BearingProperty);
            set => SetValue(Nav4BearingProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static double GetNav4Bearing(DependencyObject obj)
        {
            return (double)obj.GetValue(Nav4BearingProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetNav4Bearing(DependencyObject obj, double value)
        {
            obj.SetValue(Nav4BearingProperty, value);
        }

        private static void OnNav4BearingValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<double>(
              (double)args.OldValue, (double)args.NewValue, Nav4BearingValueChangedEvent);

            control.OnNav4BearingValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent Nav4BearingValueChangedEvent = EventManager.RegisterRoutedEvent(
          "Nav4BearingValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<double>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnNav4BearingValueChanged(RoutedPropertyChangedEventArgs<double> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region Nav42BearingProperty

        public static readonly DependencyProperty Nav42BearingProperty =
        DependencyProperty.RegisterAttached(
          "Nav42Bearing",
          typeof(double),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback(OnNav42BearingValueChanged)));


        public double Nav42Bearing
        {
            get => (double)GetValue(Nav42BearingProperty);
            set => SetValue(Nav42BearingProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static double GetNav42Bearing(DependencyObject obj)
        {
            return (double)obj.GetValue(Nav42BearingProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetNav42Bearing(DependencyObject obj, double value)
        {
            obj.SetValue(Nav42BearingProperty, value);
        }

        private static void OnNav42BearingValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<double>(
              (double)args.OldValue, (double)args.NewValue, Nav42BearingValueChangedEvent);

            control.OnNav42BearingValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent Nav42BearingValueChangedEvent = EventManager.RegisterRoutedEvent(
          "Nav42BearingValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<double>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnNav42BearingValueChanged(RoutedPropertyChangedEventArgs<double> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region Nav4ColorProperty

        public static readonly DependencyProperty Nav4ColorProperty =
        DependencyProperty.RegisterAttached(
          "Nav4Color",
          typeof(Brush),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(new SolidColorBrush(Colors.White), new PropertyChangedCallback(OnNav4ColorValueChanged)));


        public Brush Nav4Color
        {
            get => (Brush)GetValue(Nav4ColorProperty);
            set => SetValue(Nav4ColorProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static Brush GetNav4Color(DependencyObject obj)
        {
            return (Brush)obj.GetValue(Nav4ColorProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetNav4Color(DependencyObject obj, Brush value)
        {
            obj.SetValue(Nav4ColorProperty, value);
        }

        private static void OnNav4ColorValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<Brush>(
              (Brush)args.OldValue, (Brush)args.NewValue, Nav4ColorValueChangedEvent);

            control.OnNav4ColorValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent Nav4ColorValueChangedEvent = EventManager.RegisterRoutedEvent(
          "Nav4ColorValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<Brush>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnNav4ColorValueChanged(RoutedPropertyChangedEventArgs<Brush> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region Nav42ColorProperty

        public static readonly DependencyProperty Nav42ColorProperty =
        DependencyProperty.RegisterAttached(
          "Nav42Color",
          typeof(Brush),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(new SolidColorBrush(Colors.White), new PropertyChangedCallback(OnNav42ColorValueChanged)));


        public Brush Nav42Color
        {
            get => (Brush)GetValue(Nav42ColorProperty);
            set => SetValue(Nav42ColorProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static Brush GetNav42Color(DependencyObject obj)
        {
            return (Brush)obj.GetValue(Nav42ColorProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetNav42Color(DependencyObject obj, Brush value)
        {
            obj.SetValue(Nav42ColorProperty, value);
        }

        private static void OnNav42ColorValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<Brush>(
              (Brush)args.OldValue, (Brush)args.NewValue, Nav42ColorValueChangedEvent);

            control.OnNav42ColorValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent Nav42ColorValueChangedEvent = EventManager.RegisterRoutedEvent(
          "Nav42ColorValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<Brush>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnNav42ColorValueChanged(RoutedPropertyChangedEventArgs<Brush> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region Nav4NeedleLineTypeProperty

        public static readonly DependencyProperty Nav4NeedleLineTypeProperty =
        DependencyProperty.RegisterAttached(
          "Nav4NeedleLineType",
          typeof(LineType),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(LineType.Single, new PropertyChangedCallback(OnNav4NeedleLineTypeValueChanged)));


        public LineType Nav4NeedleLineType
        {
            get => (LineType)GetValue(Nav4NeedleLineTypeProperty);
            set => SetValue(Nav4NeedleLineTypeProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static LineType GetNav4NeedleLineType(DependencyObject obj)
        {
            return (LineType)obj.GetValue(Nav4NeedleLineTypeProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetNav4NeedleLineType(DependencyObject obj, LineType value)
        {
            obj.SetValue(Nav4NeedleLineTypeProperty, value);
        }

        private static void OnNav4NeedleLineTypeValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<LineType>(
              (LineType)args.OldValue, (LineType)args.NewValue, Nav4NeedleLineTypeValueChangedEvent);

            control.CreateNav4Needle();

            control.OnNav4NeedleLineTypeValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent Nav4NeedleLineTypeValueChangedEvent = EventManager.RegisterRoutedEvent(
          "Nav4NeedleLineTypeValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<LineType>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnNav4NeedleLineTypeValueChanged(RoutedPropertyChangedEventArgs<LineType> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region Nav42NeedleLineTypeProperty

        public static readonly DependencyProperty Nav42NeedleLineTypeProperty =
        DependencyProperty.RegisterAttached(
          "Nav42NeedleLineType",
          typeof(LineType),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(LineType.Single, new PropertyChangedCallback(OnNav42NeedleLineTypeValueChanged)));


        public LineType Nav42NeedleLineType
        {
            get => (LineType)GetValue(Nav42NeedleLineTypeProperty);
            set => SetValue(Nav42NeedleLineTypeProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static LineType GetNav42NeedleLineType(DependencyObject obj)
        {
            return (LineType)obj.GetValue(Nav42NeedleLineTypeProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetNav42NeedleLineType(DependencyObject obj, LineType value)
        {
            obj.SetValue(Nav42NeedleLineTypeProperty, value);
        }

        private static void OnNav42NeedleLineTypeValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<LineType>(
              (LineType)args.OldValue, (LineType)args.NewValue, Nav42NeedleLineTypeValueChangedEvent);

            control.CreateNav42Needle();

            control.OnNav42NeedleLineTypeValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent Nav42NeedleLineTypeValueChangedEvent = EventManager.RegisterRoutedEvent(
          "Nav42NeedleLineTypeValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<LineType>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnNav42NeedleLineTypeValueChanged(RoutedPropertyChangedEventArgs<LineType> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region Nav4ArrowTypeProperty

        public static readonly DependencyProperty Nav4ArrowTypeProperty =
        DependencyProperty.RegisterAttached(
          "Nav4ArrowType",
          typeof(ArrowType),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(ArrowType.Open, new PropertyChangedCallback(OnNav4ArrowTypeValueChanged)));


        public ArrowType Nav4ArrowType
        {
            get => (ArrowType)GetValue(Nav4ArrowTypeProperty);
            set => SetValue(Nav4ArrowTypeProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static ArrowType GetNav4ArrowType(DependencyObject obj)
        {
            return (ArrowType)obj.GetValue(Nav4ArrowTypeProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetNav4ArrowType(DependencyObject obj, ArrowType value)
        {
            obj.SetValue(Nav4ArrowTypeProperty, value);
        }

        private static void OnNav4ArrowTypeValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<ArrowType>(
              (ArrowType)args.OldValue, (ArrowType)args.NewValue, Nav4ArrowTypeValueChangedEvent);

            control.CreateNav4Needle();

            control.OnNav4ArrowTypeValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent Nav4ArrowTypeValueChangedEvent = EventManager.RegisterRoutedEvent(
          "Nav4ArrowTypeValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<ArrowType>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnNav4ArrowTypeValueChanged(RoutedPropertyChangedEventArgs<ArrowType> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region Nav42ArrowTypeProperty

        public static readonly DependencyProperty Nav42ArrowTypeProperty =
        DependencyProperty.RegisterAttached(
          "Nav42ArrowType",
          typeof(ArrowType),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(ArrowType.Open, new PropertyChangedCallback(OnNav42ArrowTypeValueChanged)));


        public ArrowType Nav42ArrowType
        {
            get => (ArrowType)GetValue(Nav42ArrowTypeProperty);
            set => SetValue(Nav42ArrowTypeProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static ArrowType GetNav42ArrowType(DependencyObject obj)
        {
            return (ArrowType)obj.GetValue(Nav42ArrowTypeProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetNav42ArrowType(DependencyObject obj, ArrowType value)
        {
            obj.SetValue(Nav42ArrowTypeProperty, value);
        }

        private static void OnNav42ArrowTypeValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<ArrowType>(
              (ArrowType)args.OldValue, (ArrowType)args.NewValue, Nav42ArrowTypeValueChangedEvent);

            control.CreateNav42Needle();

            control.OnNav42ArrowTypeValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent Nav42ArrowTypeValueChangedEvent = EventManager.RegisterRoutedEvent(
          "Nav42ArrowTypeValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<ArrowType>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnNav42ArrowTypeValueChanged(RoutedPropertyChangedEventArgs<ArrowType> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region Nav4NeedleLineThicknessProperty

        internal static readonly DependencyProperty Nav4NeedleLineThicknessProperty =
        DependencyProperty.RegisterAttached(
          "Nav4NeedleLineThickness",
          typeof(double),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(2.0, new PropertyChangedCallback(OnNav4NeedleLineThicknessValueChanged)));


        internal double Nav4NeedleLineThickness
        {
            get => (double)GetValue(Nav4NeedleLineThicknessProperty);
            set => SetValue(Nav4NeedleLineThicknessProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        internal static double GetNav4NeedleLineThickness(DependencyObject obj)
        {
            return (double)obj.GetValue(Nav4NeedleLineThicknessProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        internal static void SetNav4NeedleLineThickness(DependencyObject obj, double value)
        {
            obj.SetValue(Nav4NeedleLineThicknessProperty, value);
        }

        private static void OnNav4NeedleLineThicknessValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<double>(
              (double)args.OldValue, (double)args.NewValue, Nav4NeedleLineThicknessValueChangedEvent);

            control.OnNav4NeedleLineThicknessValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent Nav4NeedleLineThicknessValueChangedEvent = EventManager.RegisterRoutedEvent(
          "Nav4NeedleLineThicknessValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<double>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnNav4NeedleLineThicknessValueChanged(RoutedPropertyChangedEventArgs<double> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region Nav42NeedleLineThicknessProperty

        internal static readonly DependencyProperty Nav42NeedleLineThicknessProperty =
        DependencyProperty.RegisterAttached(
          "Nav42NeedleLineThickness",
          typeof(double),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(2.0, new PropertyChangedCallback(OnNav42NeedleLineThicknessValueChanged)));


        internal double Nav42NeedleLineThickness
        {
            get => (double)GetValue(Nav42NeedleLineThicknessProperty);
            set => SetValue(Nav42NeedleLineThicknessProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        internal static double GetNav42NeedleLineThickness(DependencyObject obj)
        {
            return (double)obj.GetValue(Nav42NeedleLineThicknessProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        internal static void SetNav42NeedleLineThickness(DependencyObject obj, double value)
        {
            obj.SetValue(Nav42NeedleLineThicknessProperty, value);
        }

        private static void OnNav42NeedleLineThicknessValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<double>(
              (double)args.OldValue, (double)args.NewValue, Nav42NeedleLineThicknessValueChangedEvent);

            control.OnNav42NeedleLineThicknessValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent Nav42NeedleLineThicknessValueChangedEvent = EventManager.RegisterRoutedEvent(
          "Nav42NeedleLineThicknessValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<double>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnNav42NeedleLineThicknessValueChanged(RoutedPropertyChangedEventArgs<double> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region Nav4DataProperty

        internal static readonly DependencyProperty Nav4DataProperty =
        DependencyProperty.RegisterAttached(
          "Nav4Data",
          typeof(System.Windows.Media.Geometry),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnNav4DataValueChanged)));


        internal System.Windows.Media.Geometry Nav4Data
        {
            get => (System.Windows.Media.Geometry)GetValue(Nav4DataProperty);
            set => SetValue(Nav4DataProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        internal static System.Windows.Media.Geometry GetNav4Data(DependencyObject obj)
        {
            return (System.Windows.Media.Geometry)obj.GetValue(Nav4DataProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        internal static void SetNav4Data(DependencyObject obj, System.Windows.Media.Geometry value)
        {
            obj.SetValue(Nav4DataProperty, value);
        }

        private static void OnNav4DataValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<System.Windows.Media.Geometry>(
              (System.Windows.Media.Geometry)args.OldValue, (System.Windows.Media.Geometry)args.NewValue, Nav4DataValueChangedEvent);

            control.OnNav4DataValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent Nav4DataValueChangedEvent = EventManager.RegisterRoutedEvent(
          "Nav4DataValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<System.Windows.Media.Geometry>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnNav4DataValueChanged(RoutedPropertyChangedEventArgs<System.Windows.Media.Geometry> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region Nav42DataProperty

        internal static readonly DependencyProperty Nav42DataProperty =
        DependencyProperty.RegisterAttached(
          "Nav42Data",
          typeof(System.Windows.Media.Geometry),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnNav42DataValueChanged)));


        internal System.Windows.Media.Geometry Nav42Data
        {
            get => (System.Windows.Media.Geometry)GetValue(Nav42DataProperty);
            set => SetValue(Nav42DataProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        internal static System.Windows.Media.Geometry GetNav42Data(DependencyObject obj)
        {
            return (System.Windows.Media.Geometry)obj.GetValue(Nav42DataProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        internal static void SetNav42Data(DependencyObject obj, System.Windows.Media.Geometry value)
        {
            obj.SetValue(Nav42DataProperty, value);
        }

        private static void OnNav42DataValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<System.Windows.Media.Geometry>(
              (System.Windows.Media.Geometry)args.OldValue, (System.Windows.Media.Geometry)args.NewValue, Nav42DataValueChangedEvent);

            control.OnNav42DataValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent Nav42DataValueChangedEvent = EventManager.RegisterRoutedEvent(
          "Nav42DataValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<System.Windows.Media.Geometry>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnNav42DataValueChanged(RoutedPropertyChangedEventArgs<System.Windows.Media.Geometry> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region CanvasWidthProperty

        public static readonly DependencyProperty CanvasWidthProperty =
        DependencyProperty.RegisterAttached(
          "CanvasWidth",
          typeof(double),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback(OnCanvasWidthValueChanged)));


        public double CanvasWidth
        {
            get => (double)GetValue(CanvasWidthProperty);
            set => SetValue(CanvasWidthProperty, value);
        }



        public bool ShowAllLabels
        {
            get { return (bool)GetValue(ShowAllLabelsProperty); }
            set { SetValue(ShowAllLabelsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ShowAllLabels.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ShowAllLabelsProperty =
            DependencyProperty.Register("ShowAllLabels", typeof(bool), typeof(HsiAdiMarkersUserControl), new PropertyMetadata(true));



        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static double GetCanvasWidth(DependencyObject obj)
        {
            return (double)obj.GetValue(CanvasWidthProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetCanvasWidth(DependencyObject obj, double value)
        {
            obj.SetValue(CanvasWidthProperty, value);
        }

        private static void OnCanvasWidthValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<double>(
              (double)args.OldValue, (double)args.NewValue, CanvasWidthValueChangedEvent);

            control.OnCanvasWidthValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent CanvasWidthValueChangedEvent = EventManager.RegisterRoutedEvent(
          "CanvasWidthValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<double>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnCanvasWidthValueChanged(RoutedPropertyChangedEventArgs<double> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region CanvasMidPointProperty

        public static readonly DependencyProperty CanvasMidPointProperty =
        DependencyProperty.RegisterAttached(
          "CanvasMidPoint",
          typeof(System.Windows.Point),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(new System.Windows.Point(), new PropertyChangedCallback(OnCanvasMidPointValueChanged)));


        public System.Windows.Point CanvasMidPoint
        {
            get => (System.Windows.Point)GetValue(CanvasMidPointProperty);
            internal set => SetValue(CanvasMidPointProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static System.Windows.Point GetCanvasMidPoint(DependencyObject obj)
        {
            return (System.Windows.Point)obj.GetValue(CanvasMidPointProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetCanvasMidPoint(DependencyObject obj, System.Windows.Point value)
        {
            obj.SetValue(CanvasMidPointProperty, value);
        }

        private static void OnCanvasMidPointValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<System.Windows.Point>(
              (System.Windows.Point)args.OldValue, (System.Windows.Point)args.NewValue, CanvasMidPointValueChangedEvent);

            control.OnCanvasMidPointValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent CanvasMidPointValueChangedEvent = EventManager.RegisterRoutedEvent(
          "CanvasMidPointValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<System.Windows.Point>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnCanvasMidPointValueChanged(RoutedPropertyChangedEventArgs<System.Windows.Point> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region CanvasRectProperty

        public static readonly DependencyProperty CanvasRectProperty =
        DependencyProperty.RegisterAttached(
          "CanvasRect",
          typeof(System.Windows.Rect),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(System.Windows.Rect.Empty, new PropertyChangedCallback(OnCanvasRectValueChanged)));


        public System.Windows.Rect CanvasRect
        {
            get => (System.Windows.Rect)GetValue(CanvasRectProperty);
            internal set => SetValue(CanvasRectProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static System.Windows.Rect GetCanvasRect(DependencyObject obj)
        {
            return (System.Windows.Rect)obj.GetValue(CanvasRectProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetCanvasRect(DependencyObject obj, System.Windows.Rect value)
        {
            obj.SetValue(CanvasRectProperty, value);
        }

        private static void OnCanvasRectValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<System.Windows.Rect>(
              (System.Windows.Rect)args.OldValue, (System.Windows.Rect)args.NewValue, CanvasRectValueChangedEvent);

            control.OnCanvasRectValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent CanvasRectValueChangedEvent = EventManager.RegisterRoutedEvent(
          "CanvasRectValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<System.Windows.Rect>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnCanvasRectValueChanged(RoutedPropertyChangedEventArgs<System.Windows.Rect> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region AltitudeProperty

        public static readonly DependencyProperty AltitudeProperty =
        DependencyProperty.RegisterAttached(
          "Altitude",
          typeof(double),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback(OnAltitudeValueChanged)));


        public double Altitude
        {
            get => (double)GetValue(AltitudeProperty);
            set => SetValue(AltitudeProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static double GetAltitude(DependencyObject obj)
        {
            return (double)obj.GetValue(AltitudeProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetAltitude(DependencyObject obj, double value)
        {
            obj.SetValue(AltitudeProperty, value);
        }

        private static void OnAltitudeValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<double>(
              (double)args.OldValue, (double)args.NewValue, AltitudeValueChangedEvent);

            control.OnAltitudeValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent AltitudeValueChangedEvent = EventManager.RegisterRoutedEvent(
          "AltitudeValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<double>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnAltitudeValueChanged(RoutedPropertyChangedEventArgs<double> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region HeadingProperty

        public static readonly DependencyProperty HeadingProperty =
        DependencyProperty.RegisterAttached(
          "Heading",
          typeof(double),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback(OnHeadingValueChanged)));


        public double Heading
        {
            get => (double)GetValue(HeadingProperty);
            set => SetValue(HeadingProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static double GetHeading(DependencyObject obj)
        {
            return (double)obj.GetValue(HeadingProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetHeading(DependencyObject obj, double value)
        {
            obj.SetValue(HeadingProperty, value);
        }

        private static void OnHeadingValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<double>(
              (double)args.OldValue, (double)args.NewValue, HeadingValueChangedEvent);

            control.OnHeadingValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent HeadingValueChangedEvent = EventManager.RegisterRoutedEvent(
          "HeadingValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<double>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnHeadingValueChanged(RoutedPropertyChangedEventArgs<double> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region RollProperty

        public static readonly DependencyProperty RollProperty =
        DependencyProperty.RegisterAttached(
          "Roll",
          typeof(double),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback(OnRollValueChanged)));


        public double Roll
        {
            get => (double)GetValue(RollProperty);
            set => SetValue(RollProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static double GetRoll(DependencyObject obj)
        {
            return (double)obj.GetValue(RollProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetRoll(DependencyObject obj, double value)
        {
            obj.SetValue(RollProperty, value);
        }

        private static void OnRollValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<double>(
              (double)args.OldValue, (double)args.NewValue, RollValueChangedEvent);

            control.OnRollValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent RollValueChangedEvent = EventManager.RegisterRoutedEvent(
          "RollValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<double>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnRollValueChanged(RoutedPropertyChangedEventArgs<double> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region PitchProperty

        public static readonly DependencyProperty PitchProperty =
        DependencyProperty.RegisterAttached(
          "Pitch",
          typeof(double),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback(OnPitchValueChanged)));


        public double Pitch
        {
            get => (double)GetValue(PitchProperty);
            set => SetValue(PitchProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static double GetPitch(DependencyObject obj)
        {
            return (double)obj.GetValue(PitchProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetPitch(DependencyObject obj, double value)
        {
            obj.SetValue(PitchProperty, value);
        }

        private static void OnPitchValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<double>(
              (double)args.OldValue, (double)args.NewValue, PitchValueChangedEvent);

            control.OnPitchValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent PitchValueChangedEvent = EventManager.RegisterRoutedEvent(
          "PitchValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<double>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnPitchValueChanged(RoutedPropertyChangedEventArgs<double> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region YawProperty

        public static readonly DependencyProperty YawProperty =
        DependencyProperty.RegisterAttached(
          "Yaw",
          typeof(double),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback(OnYawValueChanged)));


        public double Yaw
        {
            get => (double)GetValue(YawProperty);
            set => SetValue(YawProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static double GetYaw(DependencyObject obj)
        {
            return (double)obj.GetValue(YawProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetYaw(DependencyObject obj, double value)
        {
            obj.SetValue(YawProperty, value);
        }

        private static void OnYawValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<double>(
              (double)args.OldValue, (double)args.NewValue, YawValueChangedEvent);

            control.OnYawValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent YawValueChangedEvent = EventManager.RegisterRoutedEvent(
          "YawValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<double>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnYawValueChanged(RoutedPropertyChangedEventArgs<double> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region CourseProperty

        public static readonly DependencyProperty CourseProperty =
        DependencyProperty.RegisterAttached(
          "Course",
          typeof(int),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(0, new PropertyChangedCallback(OnCourseValueChanged)));


        public int Course
        {
            get => (int)GetValue(CourseProperty);
            set => SetValue(CourseProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static int GetCourse(DependencyObject obj)
        {
            return (int)obj.GetValue(CourseProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetCourse(DependencyObject obj, int value)
        {
            obj.SetValue(CourseProperty, value);
        }

        private static void OnCourseValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<int>(
              (int)args.OldValue, (int)args.NewValue, CourseValueChangedEvent);

            control.OnCourseValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent CourseValueChangedEvent = EventManager.RegisterRoutedEvent(
          "CourseValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<int>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnCourseValueChanged(RoutedPropertyChangedEventArgs<int> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region DistanceProperty

        public static readonly DependencyProperty DistanceProperty =
        DependencyProperty.RegisterAttached(
          "Distance",
          typeof(double),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback(OnDistanceValueChanged)));


        public double Distance
        {
            get => (double)GetValue(DistanceProperty);
            set => SetValue(DistanceProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static double GetDistance(DependencyObject obj)
        {
            return (double)obj.GetValue(DistanceProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetDistance(DependencyObject obj, double value)
        {
            obj.SetValue(DistanceProperty, value);
        }

        private static void OnDistanceValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<double>(
              (double)args.OldValue, (double)args.NewValue, DistanceValueChangedEvent);

            control.OnDistanceValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent DistanceValueChangedEvent = EventManager.RegisterRoutedEvent(
          "DistanceValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<double>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnDistanceValueChanged(RoutedPropertyChangedEventArgs<double> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region GroundSpeedProperty

        public static readonly DependencyProperty GroundSpeedProperty =
        DependencyProperty.RegisterAttached(
          "GroundSpeed",
          typeof(double),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback(OnGroundSpeedValueChanged)));


        public double GroundSpeed
        {
            get => (double)GetValue(GroundSpeedProperty);
            set => SetValue(GroundSpeedProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static double GetGroundSpeed(DependencyObject obj)
        {
            return (double)obj.GetValue(GroundSpeedProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetGroundSpeed(DependencyObject obj, double value)
        {
            obj.SetValue(GroundSpeedProperty, value);
        }

        private static void OnGroundSpeedValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<double>(
              (double)args.OldValue, (double)args.NewValue, GroundSpeedValueChangedEvent);

            control.OnGroundSpeedValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent GroundSpeedValueChangedEvent = EventManager.RegisterRoutedEvent(
          "GroundSpeedValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<double>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnGroundSpeedValueChanged(RoutedPropertyChangedEventArgs<double> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region OuterMarkerProperty

        public static readonly DependencyProperty OuterMarkerProperty =
        DependencyProperty.RegisterAttached(
          "OuterMarker",
          typeof(bool),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnOuterMarkerValueChanged)));


        public bool OuterMarker
        {
            get => (bool)GetValue(OuterMarkerProperty);
            set => SetValue(OuterMarkerProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static bool GetOuterMarker(DependencyObject obj)
        {
            return (bool)obj.GetValue(OuterMarkerProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetOuterMarker(DependencyObject obj, bool value)
        {
            obj.SetValue(OuterMarkerProperty, value);
        }

        private static void OnOuterMarkerValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<bool>(
              (bool)args.OldValue, (bool)args.NewValue, OuterMarkerValueChangedEvent);

            // Set the Correct color for marker
            if ((bool)args.NewValue)
                obj.SetValue(MarkerOColorBrushProperty, markerOuterBrush);
            else
                obj.SetValue(MarkerOColorBrushProperty, markerOffBrush);

            control.OnOuterMarkerValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent OuterMarkerValueChangedEvent = EventManager.RegisterRoutedEvent(
          "OuterMarkerValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<bool>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnOuterMarkerValueChanged(RoutedPropertyChangedEventArgs<bool> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region MiddleMarkerProperty

        public static readonly DependencyProperty MiddleMarkerProperty =
        DependencyProperty.RegisterAttached(
          "MiddleMarker",
          typeof(bool),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnMiddleMarkerValueChanged)));


        public bool MiddleMarker
        {
            get => (bool)GetValue(MiddleMarkerProperty);
            set => SetValue(MiddleMarkerProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static bool GetMiddleMarker(DependencyObject obj)
        {
            return (bool)obj.GetValue(MiddleMarkerProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetMiddleMarker(DependencyObject obj, bool value)
        {
            obj.SetValue(MiddleMarkerProperty, value);
        }

        private static void OnMiddleMarkerValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<bool>(
              (bool)args.OldValue, (bool)args.NewValue, MiddleMarkerValueChangedEvent);

            // Set the Correct color for marker
            if ((bool)args.NewValue)
                obj.SetValue(MarkerMColorBrushProperty, markerMiddleBrush);
            else
                obj.SetValue(MarkerMColorBrushProperty, markerOffBrush);

            control.OnMiddleMarkerValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent MiddleMarkerValueChangedEvent = EventManager.RegisterRoutedEvent(
          "MiddleMarkerValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<bool>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnMiddleMarkerValueChanged(RoutedPropertyChangedEventArgs<bool> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region InnerMarkerProperty

        public static readonly DependencyProperty InnerMarkerProperty =
        DependencyProperty.RegisterAttached(
          "InnerMarker",
          typeof(bool),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnInnerMarkerValueChanged)));


        public bool InnerMarker
        {
            get => (bool)GetValue(InnerMarkerProperty);
            set => SetValue(InnerMarkerProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static bool GetInnerMarker(DependencyObject obj)
        {
            return (bool)obj.GetValue(InnerMarkerProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetInnerMarker(DependencyObject obj, bool value)
        {
            obj.SetValue(InnerMarkerProperty, value);
        }

        private static void OnInnerMarkerValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<bool>(
              (bool)args.OldValue, (bool)args.NewValue, InnerMarkerValueChangedEvent);

            // Set the Correct color for marker
            if ((bool)args.NewValue)
                obj.SetValue(MarkerIColorBrushProperty, markerInnerBrush);
            else
                obj.SetValue(MarkerIColorBrushProperty, markerOffBrush);

            control.OnInnerMarkerValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent InnerMarkerValueChangedEvent = EventManager.RegisterRoutedEvent(
          "InnerMarkerValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<bool>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnInnerMarkerValueChanged(RoutedPropertyChangedEventArgs<bool> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region MarkerOColorBrushProperty

        internal static readonly DependencyProperty MarkerOColorBrushProperty =
        DependencyProperty.RegisterAttached(
          "MarkerOColorBrush",
          typeof(SolidColorBrush),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(Theme.GetResource(ThemeResourceKey.PrimaryColorBrush9), new PropertyChangedCallback(OnMarkerOColorBrushValueChanged)));


        internal SolidColorBrush MarkerOColorBrush
        {
            get => (SolidColorBrush)GetValue(MarkerOColorBrushProperty);
            set => SetValue(MarkerOColorBrushProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        internal static SolidColorBrush GetMarkerOColorBrush(DependencyObject obj)
        {
            return (SolidColorBrush)obj.GetValue(MarkerOColorBrushProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        internal static void SetMarkerOColorBrush(DependencyObject obj, SolidColorBrush value)
        {
            obj.SetValue(MarkerOColorBrushProperty, value);
        }

        private static void OnMarkerOColorBrushValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<SolidColorBrush>(
              (SolidColorBrush)args.OldValue, (SolidColorBrush)args.NewValue, MarkerOColorBrushValueChangedEvent);

            control.OnMarkerOColorBrushValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        internal static readonly RoutedEvent MarkerOColorBrushValueChangedEvent = EventManager.RegisterRoutedEvent(
          "MarkerOColorBrushValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<SolidColorBrush>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnMarkerOColorBrushValueChanged(RoutedPropertyChangedEventArgs<SolidColorBrush> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region MarkerMColorBrushProperty

        internal static readonly DependencyProperty MarkerMColorBrushProperty =
        DependencyProperty.RegisterAttached(
          "MarkerMColorBrush",
          typeof(SolidColorBrush),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(Theme.GetResource(ThemeResourceKey.PrimaryColorBrush9), new PropertyChangedCallback(OnMarkerMColorBrushValueChanged)));


        internal SolidColorBrush MarkerMColorBrush
        {
            get => (SolidColorBrush)GetValue(MarkerMColorBrushProperty);
            set => SetValue(MarkerMColorBrushProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        internal static SolidColorBrush GetMarkerMColorBrush(DependencyObject obj)
        {
            return (SolidColorBrush)obj.GetValue(MarkerMColorBrushProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        internal static void SetMarkerMColorBrush(DependencyObject obj, SolidColorBrush value)
        {
            obj.SetValue(MarkerMColorBrushProperty, value);
        }

        private static void OnMarkerMColorBrushValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<SolidColorBrush>(
              (SolidColorBrush)args.OldValue, (SolidColorBrush)args.NewValue, MarkerMColorBrushValueChangedEvent);

            control.OnMarkerMColorBrushValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        internal static readonly RoutedEvent MarkerMColorBrushValueChangedEvent = EventManager.RegisterRoutedEvent(
          "MarkerMColorBrushValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<SolidColorBrush>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnMarkerMColorBrushValueChanged(RoutedPropertyChangedEventArgs<SolidColorBrush> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region MarkerIColorBrushProperty

        internal static readonly DependencyProperty MarkerIColorBrushProperty =
        DependencyProperty.RegisterAttached(
          "MarkerIColorBrush",
          typeof(SolidColorBrush),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(Theme.GetResource(ThemeResourceKey.PrimaryColorBrush9), new PropertyChangedCallback(OnMarkerIColorBrushValueChanged)));


        internal SolidColorBrush MarkerIColorBrush
        {
            get => (SolidColorBrush)GetValue(MarkerIColorBrushProperty);
            set => SetValue(MarkerIColorBrushProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        internal static SolidColorBrush GetMarkerIColorBrush(DependencyObject obj)
        {
            return (SolidColorBrush)obj.GetValue(MarkerIColorBrushProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        internal static void SetMarkerIColorBrush(DependencyObject obj, SolidColorBrush value)
        {
            obj.SetValue(MarkerIColorBrushProperty, value);
        }

        private static void OnMarkerIColorBrushValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<SolidColorBrush>(
              (SolidColorBrush)args.OldValue, (SolidColorBrush)args.NewValue, MarkerIColorBrushValueChangedEvent);

            control.OnMarkerIColorBrushValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        internal static readonly RoutedEvent MarkerIColorBrushValueChangedEvent = EventManager.RegisterRoutedEvent(
          "MarkerIColorBrushValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<SolidColorBrush>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnMarkerIColorBrushValueChanged(RoutedPropertyChangedEventArgs<SolidColorBrush> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region PrimaryNeedleDeviationPointCollectionProperty

        internal static readonly DependencyProperty PrimaryNeedleDeviationPointCollectionProperty =
        DependencyProperty.RegisterAttached(
          "PrimaryNeedleDeviationPointCollection",
          typeof(System.Windows.Media.PointCollection),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(new System.Windows.Media.PointCollection(), new PropertyChangedCallback(OnPrimaryNeedleDeviationPointCollectionValueChanged)));


        internal System.Windows.Media.PointCollection PrimaryNeedleDeviationPointCollection
        {
            get => (System.Windows.Media.PointCollection)GetValue(PrimaryNeedleDeviationPointCollectionProperty);
            set => SetValue(PrimaryNeedleDeviationPointCollectionProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        internal static System.Windows.Media.PointCollection GetPrimaryNeedleDeviationPointCollection(DependencyObject obj)
        {
            return (System.Windows.Media.PointCollection)obj.GetValue(PrimaryNeedleDeviationPointCollectionProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        internal static void SetPrimaryNeedleDeviationPointCollection(DependencyObject obj, System.Windows.Media.PointCollection value)
        {
            obj.SetValue(PrimaryNeedleDeviationPointCollectionProperty, value);
        }

        private static void OnPrimaryNeedleDeviationPointCollectionValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<System.Windows.Media.PointCollection>(
              (System.Windows.Media.PointCollection)args.OldValue, (System.Windows.Media.PointCollection)args.NewValue, PrimaryNeedleDeviationPointCollectionValueChangedEvent);

            control.OnPrimaryNeedleDeviationPointCollectionValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent PrimaryNeedleDeviationPointCollectionValueChangedEvent = EventManager.RegisterRoutedEvent(
          "PrimaryNeedleDeviationPointCollectionValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<System.Windows.Media.PointCollection>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnPrimaryNeedleDeviationPointCollectionValueChanged(RoutedPropertyChangedEventArgs<System.Windows.Media.PointCollection> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region PrimaryRefNeedleDeviationPointCollectionProperty

        internal static readonly DependencyProperty PrimaryRefNeedleDeviationPointCollectionProperty =
        DependencyProperty.RegisterAttached(
          "PrimaryRefNeedleDeviationPointCollection",
          typeof(System.Windows.Media.PointCollection),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(new System.Windows.Media.PointCollection(), new PropertyChangedCallback(OnPrimaryRefNeedleDeviationPointCollectionValueChanged)));


        internal System.Windows.Media.PointCollection PrimaryRefNeedleDeviationPointCollection
        {
            get => (System.Windows.Media.PointCollection)GetValue(PrimaryRefNeedleDeviationPointCollectionProperty);
            set => SetValue(PrimaryRefNeedleDeviationPointCollectionProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        internal static System.Windows.Media.PointCollection GetPrimaryRefNeedleDeviationPointCollection(DependencyObject obj)
        {
            return (System.Windows.Media.PointCollection)obj.GetValue(PrimaryRefNeedleDeviationPointCollectionProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        internal static void SetPrimaryRefNeedleDeviationPointCollection(DependencyObject obj, System.Windows.Media.PointCollection value)
        {
            obj.SetValue(PrimaryRefNeedleDeviationPointCollectionProperty, value);
        }

        private static void OnPrimaryRefNeedleDeviationPointCollectionValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<System.Windows.Media.PointCollection>(
              (System.Windows.Media.PointCollection)args.OldValue, (System.Windows.Media.PointCollection)args.NewValue, PrimaryRefNeedleDeviationPointCollectionValueChangedEvent);

            control.OnPrimaryRefNeedleDeviationPointCollectionValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent PrimaryRefNeedleDeviationPointCollectionValueChangedEvent = EventManager.RegisterRoutedEvent(
          "PrimaryRefNeedleDeviationPointCollectionValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<System.Windows.Media.PointCollection>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnPrimaryRefNeedleDeviationPointCollectionValueChanged(RoutedPropertyChangedEventArgs<System.Windows.Media.PointCollection> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region CanvasBottomLeftPointProperty

        internal static readonly DependencyProperty CanvasBottomLeftPointProperty =
        DependencyProperty.RegisterAttached(
          "CanvasBottomLeftPoint",
          typeof(Point),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(new Point(), new PropertyChangedCallback(OnCanvasBottomLeftPointValueChanged)));


        internal Point CanvasBottomLeftPoint
        {
            get => (Point)GetValue(CanvasBottomLeftPointProperty);
            set => SetValue(CanvasBottomLeftPointProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        internal static Point GetCanvasBottomLeftPoint(DependencyObject obj)
        {
            return (Point)obj.GetValue(CanvasBottomLeftPointProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        internal static void SetCanvasBottomLeftPoint(DependencyObject obj, Point value)
        {
            obj.SetValue(CanvasBottomLeftPointProperty, value);
        }

        private static void OnCanvasBottomLeftPointValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<Point>(
              (Point)args.OldValue, (Point)args.NewValue, CanvasBottomLeftPointValueChangedEvent);

            control.OnCanvasBottomLeftPointValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent CanvasBottomLeftPointValueChangedEvent = EventManager.RegisterRoutedEvent(
          "CanvasBottomLeftPointValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<Point>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnCanvasBottomLeftPointValueChanged(RoutedPropertyChangedEventArgs<Point> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region Nav1NeedleFillProperty

        public static readonly DependencyProperty Nav1NeedleFillProperty =
        DependencyProperty.RegisterAttached(
          "Nav1NeedleFill",
          typeof(Brush),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(new SolidColorBrush(System.Windows.Media.Colors.Transparent), new PropertyChangedCallback(OnNav1NeedleFillValueChanged)));


        public Brush Nav1NeedleFill
        {
            get => (Brush)GetValue(Nav1NeedleFillProperty);
            set => SetValue(Nav1NeedleFillProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static Brush GetNav1NeedleFill(DependencyObject obj)
        {
            return (Brush)obj.GetValue(Nav1NeedleFillProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetNav1NeedleFill(DependencyObject obj, Brush value)
        {
            obj.SetValue(Nav1NeedleFillProperty, value);
        }

        private static void OnNav1NeedleFillValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<Brush>(
              (Brush)args.OldValue, (Brush)args.NewValue, Nav1NeedleFillValueChangedEvent);

            control.OnNav1NeedleFillValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent Nav1NeedleFillValueChangedEvent = EventManager.RegisterRoutedEvent(
          "Nav1NeedleFillValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<Brush>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnNav1NeedleFillValueChanged(RoutedPropertyChangedEventArgs<Brush> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region Nav2NeedleFillProperty

        public static readonly DependencyProperty Nav2NeedleFillProperty =
        DependencyProperty.RegisterAttached(
          "Nav2NeedleFill",
          typeof(Brush),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(new SolidColorBrush(System.Windows.Media.Colors.Transparent), new PropertyChangedCallback(OnNav2NeedleFillValueChanged)));


        public Brush Nav2NeedleFill
        {
            get => (Brush)GetValue(Nav2NeedleFillProperty);
            set => SetValue(Nav2NeedleFillProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static Brush GetNav2NeedleFill(DependencyObject obj)
        {
            return (Brush)obj.GetValue(Nav2NeedleFillProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetNav2NeedleFill(DependencyObject obj, Brush value)
        {
            obj.SetValue(Nav2NeedleFillProperty, value);
        }

        private static void OnNav2NeedleFillValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<Brush>(
              (Brush)args.OldValue, (Brush)args.NewValue, Nav2NeedleFillValueChangedEvent);

            control.OnNav2NeedleFillValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent Nav2NeedleFillValueChangedEvent = EventManager.RegisterRoutedEvent(
          "Nav2NeedleFillValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<Brush>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnNav2NeedleFillValueChanged(RoutedPropertyChangedEventArgs<Brush> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region Nav3NeedleFillProperty

        public static readonly DependencyProperty Nav3NeedleFillProperty =
        DependencyProperty.RegisterAttached(
          "Nav3NeedleFill",
          typeof(Brush),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(new SolidColorBrush(System.Windows.Media.Colors.Transparent), new PropertyChangedCallback(OnNav3NeedleFillValueChanged)));


        public Brush Nav3NeedleFill
        {
            get => (Brush)GetValue(Nav3NeedleFillProperty);
            set => SetValue(Nav3NeedleFillProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static Brush GetNav3NeedleFill(DependencyObject obj)
        {
            return (Brush)obj.GetValue(Nav3NeedleFillProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetNav3NeedleFill(DependencyObject obj, Brush value)
        {
            obj.SetValue(Nav3NeedleFillProperty, value);
        }

        private static void OnNav3NeedleFillValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<Brush>(
              (Brush)args.OldValue, (Brush)args.NewValue, Nav3NeedleFillValueChangedEvent);

            control.OnNav3NeedleFillValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent Nav3NeedleFillValueChangedEvent = EventManager.RegisterRoutedEvent(
          "Nav3NeedleFillValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<Brush>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnNav3NeedleFillValueChanged(RoutedPropertyChangedEventArgs<Brush> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region Nav4NeedleFillProperty

        public static readonly DependencyProperty Nav4NeedleFillProperty =
        DependencyProperty.RegisterAttached(
          "Nav4NeedleFill",
          typeof(Brush),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(new SolidColorBrush(System.Windows.Media.Colors.Transparent), new PropertyChangedCallback(OnNav4NeedleFillValueChanged)));


        public Brush Nav4NeedleFill
        {
            get => (Brush)GetValue(Nav4NeedleFillProperty);
            set => SetValue(Nav4NeedleFillProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static Brush GetNav4NeedleFill(DependencyObject obj)
        {
            return (Brush)obj.GetValue(Nav4NeedleFillProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetNav4NeedleFill(DependencyObject obj, Brush value)
        {
            obj.SetValue(Nav4NeedleFillProperty, value);
        }

        private static void OnNav4NeedleFillValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<Brush>(
              (Brush)args.OldValue, (Brush)args.NewValue, Nav4NeedleFillValueChangedEvent);

            control.OnNav4NeedleFillValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent Nav4NeedleFillValueChangedEvent = EventManager.RegisterRoutedEvent(
          "Nav4NeedleFillValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<Brush>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnNav4NeedleFillValueChanged(RoutedPropertyChangedEventArgs<Brush> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region Nav12NeedleFillProperty

        public static readonly DependencyProperty Nav12NeedleFillProperty =
        DependencyProperty.RegisterAttached(
          "Nav12NeedleFill",
          typeof(Brush),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(new SolidColorBrush(System.Windows.Media.Colors.Transparent), new PropertyChangedCallback(OnNav12NeedleFillValueChanged)));


        public Brush Nav12NeedleFill
        {
            get => (Brush)GetValue(Nav12NeedleFillProperty);
            set => SetValue(Nav12NeedleFillProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static Brush GetNav12NeedleFill(DependencyObject obj)
        {
            return (Brush)obj.GetValue(Nav12NeedleFillProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetNav12NeedleFill(DependencyObject obj, Brush value)
        {
            obj.SetValue(Nav12NeedleFillProperty, value);
        }

        private static void OnNav12NeedleFillValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<Brush>(
              (Brush)args.OldValue, (Brush)args.NewValue, Nav12NeedleFillValueChangedEvent);

            control.OnNav12NeedleFillValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent Nav12NeedleFillValueChangedEvent = EventManager.RegisterRoutedEvent(
          "Nav12NeedleFillValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<Brush>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnNav12NeedleFillValueChanged(RoutedPropertyChangedEventArgs<Brush> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region Nav22NeedleFillProperty

        public static readonly DependencyProperty Nav22NeedleFillProperty =
        DependencyProperty.RegisterAttached(
          "Nav22NeedleFill",
          typeof(Brush),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(new SolidColorBrush(System.Windows.Media.Colors.Transparent), new PropertyChangedCallback(OnNav22NeedleFillValueChanged)));


        public Brush Nav22NeedleFill
        {
            get => (Brush)GetValue(Nav22NeedleFillProperty);
            set => SetValue(Nav22NeedleFillProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static Brush GetNav22NeedleFill(DependencyObject obj)
        {
            return (Brush)obj.GetValue(Nav22NeedleFillProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetNav22NeedleFill(DependencyObject obj, Brush value)
        {
            obj.SetValue(Nav22NeedleFillProperty, value);
        }

        private static void OnNav22NeedleFillValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<Brush>(
              (Brush)args.OldValue, (Brush)args.NewValue, Nav22NeedleFillValueChangedEvent);

            control.OnNav22NeedleFillValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent Nav22NeedleFillValueChangedEvent = EventManager.RegisterRoutedEvent(
          "Nav22NeedleFillValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<Brush>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnNav22NeedleFillValueChanged(RoutedPropertyChangedEventArgs<Brush> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region Nav32NeedleFillProperty

        public static readonly DependencyProperty Nav32NeedleFillProperty =
        DependencyProperty.RegisterAttached(
          "Nav32NeedleFill",
          typeof(Brush),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(new SolidColorBrush(System.Windows.Media.Colors.Transparent), new PropertyChangedCallback(OnNav32NeedleFillValueChanged)));


        public Brush Nav32NeedleFill
        {
            get => (Brush)GetValue(Nav32NeedleFillProperty);
            set => SetValue(Nav32NeedleFillProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static Brush GetNav32NeedleFill(DependencyObject obj)
        {
            return (Brush)obj.GetValue(Nav32NeedleFillProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetNav32NeedleFill(DependencyObject obj, Brush value)
        {
            obj.SetValue(Nav32NeedleFillProperty, value);
        }

        private static void OnNav32NeedleFillValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<Brush>(
              (Brush)args.OldValue, (Brush)args.NewValue, Nav32NeedleFillValueChangedEvent);

            control.OnNav32NeedleFillValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent Nav32NeedleFillValueChangedEvent = EventManager.RegisterRoutedEvent(
          "Nav32NeedleFillValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<Brush>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnNav32NeedleFillValueChanged(RoutedPropertyChangedEventArgs<Brush> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region Nav42NeedleFillProperty

        public static readonly DependencyProperty Nav42NeedleFillProperty =
        DependencyProperty.RegisterAttached(
          "Nav42NeedleFill",
          typeof(Brush),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(new SolidColorBrush(System.Windows.Media.Colors.Transparent), new PropertyChangedCallback(OnNav42NeedleFillValueChanged)));


        public Brush Nav42NeedleFill
        {
            get => (Brush)GetValue(Nav42NeedleFillProperty);
            set => SetValue(Nav42NeedleFillProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static Brush GetNav42NeedleFill(DependencyObject obj)
        {
            return (Brush)obj.GetValue(Nav42NeedleFillProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetNav42NeedleFill(DependencyObject obj, Brush value)
        {
            obj.SetValue(Nav42NeedleFillProperty, value);
        }

        private static void OnNav42NeedleFillValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<Brush>(
              (Brush)args.OldValue, (Brush)args.NewValue, Nav42NeedleFillValueChangedEvent);

            control.OnNav42NeedleFillValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent Nav42NeedleFillValueChangedEvent = EventManager.RegisterRoutedEvent(
          "Nav42NeedleFillValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<Brush>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnNav42NeedleFillValueChanged(RoutedPropertyChangedEventArgs<Brush> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region TouchCanvas

        private void Canvas_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (ShowAllLabels)
            {
                TouchCanvasVisibility = !TouchCanvasVisibility;
            }
        }

        public static readonly DependencyProperty TouchCanvasVisibilityProperty =
            DependencyProperty.Register(nameof(TouchCanvasVisibility), typeof(bool), typeof(HsiAdiMarkersUserControl));

        public bool TouchCanvasVisibility
        {
            get => (bool)GetValue(TouchCanvasVisibilityProperty);
            set => SetValue(TouchCanvasVisibilityProperty, value);
        }

        public static readonly DependencyProperty AngleProperty =
            DependencyProperty.Register(nameof(AngleTouchCanvas), typeof(double), typeof(HsiAdiMarkersUserControl));

        public double AngleTouchCanvas
        {
            get => (double)GetValue(AngleProperty);
            set => SetValue(AngleProperty, value);
        }

        private void BorderTouchCanvas_OnMouseMove(object sender, MouseEventArgs e)
        {
            if (_dragStarted)
            {
                var mousePoint = e.GetPosition(this);
                var new_angle = GetStartAngle(mousePoint, borderTouchCanvas);
                _currentAngle = new_angle - _startAngle;
                _currentAngle *= 180 / Math.PI;
                _currentAngle += _totalAngle;

                if (_currentAngle < 359 && _currentAngle > -0)
                {
                    var transformBorderTouchCanvas = new RotateTransform(_currentAngle, borderTouchCanvas.Width / 2, borderTouchCanvas.Height / 2);
                    borderTouchCanvas.RenderTransform = transformBorderTouchCanvas;

                    var transformPrimaryNeedle = new RotateTransform(_currentAngle, canvasPrimaryNeedle.Width / 2, canvasPrimaryNeedle.Height / 2);
                    canvasPrimaryNeedle.RenderTransform = transformPrimaryNeedle;

                    PrimaryNavCourse = transformBorderTouchCanvas.Angle;
                    if (PrimaryNavCourse < 0)
                    {
                        PrimaryNavCourse += 360;
                    }
                }
                else
                {
                    _currentAngle = 0;
                }
            }
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            _dragStarted = false;
            _totalAngle = _currentAngle;
            _totalAngle = PrimaryNavCourse;
        }

        private void BorderTouchCanvas_OnMouseDown(object sender, MouseEventArgs e)
        {
            _dragStarted = true;
            var mousePoint = e.GetPosition(this);
            _startAngle = GetStartAngle(mousePoint, borderTouchCanvas);
        }

        private double GetStartAngle(Point point, Canvas canvas)
        {
            double dx = point.X - canvas.Width / 2;
            double dy = point.Y - canvas.Height / 2;
            return Math.Atan2(dy, dx);
        }

        #endregion


        #region ShowToIndicatorProperty

        public static readonly DependencyProperty ShowToIndicatorProperty =
        DependencyProperty.RegisterAttached(
          "ShowToIndicator",
          typeof(Visibility),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(Visibility.Hidden, new PropertyChangedCallback(OnShowToIndicatorValueChanged)));


        public Visibility ShowToIndicator
        {
            get => (Visibility)GetValue(ShowToIndicatorProperty);
            set => SetValue(ShowToIndicatorProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static Visibility GetShowToIndicator(DependencyObject obj)
        {
            return (Visibility)obj.GetValue(ShowToIndicatorProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetShowToIndicator(DependencyObject obj, Visibility value)
        {
            obj.SetValue(ShowToIndicatorProperty, value);
        }

        private static void OnShowToIndicatorValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<Visibility>(
              (Visibility)args.OldValue, (Visibility)args.NewValue, ShowToIndicatorValueChangedEvent);

            control.OnShowToIndicatorValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent ShowToIndicatorValueChangedEvent = EventManager.RegisterRoutedEvent(
          "ShowToIndicatorValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<Visibility>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnShowToIndicatorValueChanged(RoutedPropertyChangedEventArgs<Visibility> args)
        {
            RaiseEvent(args);
        }
        #endregion

        #region ShowFromIndicatorProperty

        public static readonly DependencyProperty ShowFromIndicatorProperty =
        DependencyProperty.RegisterAttached(
          "ShowFromIndicator",
          typeof(Visibility),
          typeof(HsiAdiMarkersUserControl),
                new FrameworkPropertyMetadata(Visibility.Hidden, new PropertyChangedCallback(OnShowFromIndicatorValueChanged)));


        public Visibility ShowFromIndicator
        {
            get => (Visibility)GetValue(ShowFromIndicatorProperty);
            set => SetValue(ShowFromIndicatorProperty, value);
        }

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static Visibility GetShowFromIndicator(DependencyObject obj)
        {
            return (Visibility)obj.GetValue(ShowFromIndicatorProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetShowFromIndicator(DependencyObject obj, Visibility value)
        {
            obj.SetValue(ShowFromIndicatorProperty, value);
        }

        private static void OnShowFromIndicatorValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (HsiAdiMarkersUserControl)obj;

            var e = new RoutedPropertyChangedEventArgs<Visibility>(
              (Visibility)args.OldValue, (Visibility)args.NewValue, ShowFromIndicatorValueChangedEvent);

            control.OnShowFromIndicatorValueChanged(e);
        }

        /// <summary>
        /// Identifies the ValueChanged routed event.
        /// </summary>
        public static readonly RoutedEvent ShowFromIndicatorValueChangedEvent = EventManager.RegisterRoutedEvent(
          "ShowFromIndicatorValueChanged", RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<Visibility>), typeof(HsiAdiMarkersUserControl));

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnShowFromIndicatorValueChanged(RoutedPropertyChangedEventArgs<Visibility> args)
        {
            RaiseEvent(args);
        }
        #endregion

        public static readonly DependencyProperty ShowNAVsymbolProperty =
            DependencyProperty.Register(
                nameof(ShowNAVsymbol),
                typeof(bool),
                typeof(HsiAdiMarkersUserControl));

        public bool ShowNAVsymbol
        {
            get => (bool)GetValue(ShowNAVsymbolProperty);
            set => SetValue(ShowNAVsymbolProperty, value);
        }

        public static readonly DependencyProperty ShowGPsymbolProperty =
            DependencyProperty.Register(
                nameof(ShowGPsymbol),
                typeof(bool),
                typeof(HsiAdiMarkersUserControl));

        public bool ShowGPsymbol
        {
            get => (bool)GetValue(ShowGPsymbolProperty);
            set => SetValue(ShowGPsymbolProperty, value);
        }
    }
}
