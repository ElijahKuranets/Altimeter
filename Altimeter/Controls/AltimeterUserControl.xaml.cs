﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;

namespace Altimeter.Controls
{
    /// <summary>
    /// Interaction logic for AltimeterUserControl.xaml
    /// </summary>
    public partial class AltimeterUserControl : UserControl
    {
        private Path _bigHandPath;
        private Path _smallHandPath;
        private const int Division = 60;
        private const int MaxMark = 10;
        private const int MaxValue = 10000;
        
        public AltimeterUserControl()
        {
            InitializeComponent();
            DrawMetricScale();
        }

        #region altimeter dependency properties registration

        public static readonly DependencyProperty BoardHeightProperty =
            DependencyProperty.Register(nameof(AltimeterHeight), typeof(string), typeof(AltimeterUserControl));
        public static readonly DependencyProperty BoardWidthProperty =
            DependencyProperty.Register(nameof(AltimeterWidth), typeof(string), typeof(AltimeterUserControl));
        public static readonly DependencyProperty AltitudeProperty = DependencyProperty.Register(
            nameof(AltitudeValue), typeof(int), typeof(AltimeterUserControl), new PropertyMetadata(default(int), ValuePropertyChangedCallback));
        #endregion

        #region altimeter properties

        public string AltimeterHeight
        {
            get => (string)GetValue(BoardHeightProperty);
            set => SetValue(BoardHeightProperty, value);
        }
        public string AltimeterWidth
        {
            get => (string)GetValue(BoardWidthProperty);
            set => SetValue(BoardWidthProperty, value);
        }
        public int AltitudeValue
        {
            get => (int)GetValue(AltitudeProperty);
            set => SetCurrentValue(AltitudeProperty, value);
        }
        #endregion

        #region altimeter methods

        private static void ValuePropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (AltimeterUserControl)d;
            var value = (int)e.NewValue;

            var bigHandAngle = value / (MaxValue / 360.0);
            var smallHandAngle = (value % 1000) * ((MaxMark * 360.0) / MaxValue);

            control._bigHandPath.RenderTransform = new RotateTransform(bigHandAngle, 125, 125);
            control._smallHandPath.RenderTransform = new RotateTransform(smallHandAngle, 125, 125);
        }

        private void DrawMetricScale()
        {
            var lineBrush = new SolidColorBrush(Colors.AntiqueWhite);

            var width = Altimeter.Width;
            var height = Altimeter.Height;

            DrawNumbers(width, height);
            DrawTicks(width, height, lineBrush);
            DrawBigHand(lineBrush);
            DrawSmallHand(lineBrush);
        }

        private void DrawTicks(double w, double h, SolidColorBrush lineBrush)
        {
            for (var i = 0; i < Division; i++)
            {
                var angle = (360.0 / Division) * i + 45;
                double longLine = i % (Division / MaxMark) == 0 ? 8 : 6;
                var trace = new RotateTransform(angle, w / 2, w / 2);
                var line = new Line
                {
                    X1 = w / 6.7,
                    Y1 = w / 6.7,
                    X2 = w / 6.7 + longLine,
                    Y2 = w / 6.7 + longLine,
                    StrokeThickness = 1,
                    Stroke = lineBrush,
                    RenderTransform = trace,
                };
                Altimeter.Children.Add(line);
            }
        }

        private void DrawNumbers(double weight, double heigth)
        {
            var angle = 0;

            for (var i = 0; i < MaxMark; i++)
            {
                var border = new Border
                {
                    Width = 20,
                    Height = 20,
                };
                Canvas.SetLeft(border, weight / 2);

                var transformGroup = new TransformGroup();
                transformGroup.Children.Add(new TranslateTransform(-11, 16));
                transformGroup.Children.Add(new RotateTransform(angle, 0, weight / 2));
                border.RenderTransform = transformGroup;

                var label = new Label
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Content = i.ToString()
                };
                border.LayoutTransform = new RotateTransform(-angle, 0, 0);
                border.Child = label;

                Altimeter.Children.Add(border);
                angle += 360 / MaxMark;
            }
        }

        private void DrawSmallHand(SolidColorBrush lineBrush)
        {
            var pathFigure = new PathFigure
            {
                StartPoint = new Point(137, 125)
            };
            pathFigure.Segments.Add(
                new ArcSegment(
                    new Point(113, 125),
                    new Size(10, 10),
                    45,
                    true, /* IsLargeArc */
                    SweepDirection.Clockwise,
                    true /* IsStroked */ ));
            pathFigure.Segments.Add(
                new LineSegment(
                    new Point(125, 0),
                    true /* IsStroked */ ));
            pathFigure.Segments.Add(
                new LineSegment(
                    new Point(137, 125),
                    true /* IsStroked */ ));

            var pathGeometry = new PathGeometry();
            pathGeometry.Figures.Add(pathFigure);

            _smallHandPath = new Path
            {
                Name = "smallHand",
                Width = 250,
                Height = 250,
                Fill = lineBrush,
                StrokeThickness = 1,
                Data = pathGeometry,
                Effect = new DropShadowEffect
                {
                    BlurRadius = 10,
                    Opacity = 0.5,
                    ShadowDepth = 1
                }
            };

            Altimeter.Children.Add(_smallHandPath);
        }

        private void DrawBigHand(SolidColorBrush lineBrush)
        {
            var pathFigure = new PathFigure { StartPoint = new Point(135, 125) };
            pathFigure.Segments.Add(
                new ArcSegment(
                    new Point(115, 125),
                    new Size(10, 10),
                    45,
                    true, /* IsLargeArc */
                    SweepDirection.Clockwise,
                    true /* IsStroked */ ));
            pathFigure.Segments.Add(
                new LineSegment(
                    new Point(115, 70),
                    true /* IsStroked */ ));
            pathFigure.Segments.Add(
                new LineSegment(
                    new Point(125, 40),
                    true /* IsStroked */ ));
            pathFigure.Segments.Add(
                new LineSegment(
                    new Point(135, 70),
                    true /* IsStroked */ ));
            pathFigure.Segments.Add(
                new LineSegment(
                    new Point(135, 125),
                    true /* IsStroked */ ));

            var pathGeometry = new PathGeometry();
            pathGeometry.Figures.Add(pathFigure);

            _bigHandPath = new Path
            {
                Name = "bigHand",
                Width = 250,
                Height = 250,
                Fill = lineBrush,
                StrokeThickness = 1,
                Data = pathGeometry,
                Effect = new DropShadowEffect
                {
                    BlurRadius = 10,
                    Opacity = 0.5,
                    ShadowDepth = 1
                }
            };

            Altimeter.Children.Add(_bigHandPath);
        }

        #endregion
    }
}
