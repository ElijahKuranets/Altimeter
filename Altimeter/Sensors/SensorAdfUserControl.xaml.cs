using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using UI.WPFCore;

namespace Dev.Sensors
{
    /// <summary>
    /// Interaction logic for SensorAdfUserControl.xaml
    /// </summary>
    public partial class SensorAdfUserControl : UserControl
    {
        public static int MaxMark = 12;
        private const int Division = 60;
        public const int MaxValue = 360;

        public SensorAdfUserControl()
        {
            InitializeComponent();
            var width = SensorAdfNeedle.Width;
            var height = SensorAdfNeedle.Height;
            DrawCompass(width, height);

            //Arrow.Style = (Style)FindResource("PathStyleBody1");
        }

        public static readonly DependencyProperty RelativeBearingProperty = DependencyProperty.Register(
            nameof(RelativeBearing), typeof(double), typeof(SensorAdfUserControl),
            new PropertyMetadata(999.0, RelativeBearingPropertyChangedCallback));

        public static readonly DependencyProperty MagHeadingProperty = DependencyProperty.Register(
            nameof(MagHeading), typeof(double), typeof(SensorAdfUserControl),
            new PropertyMetadata(999.0, MagHeadingPropertyChangedCallback));

        public double RelativeBearing
        {
            get => (double) GetValue(RelativeBearingProperty);
            set => SetCurrentValue(RelativeBearingProperty, value);
        }

        public double MagHeading
        {
            get => (double) GetValue(MagHeadingProperty);
            set => SetCurrentValue(MagHeadingProperty, value);
        }

        private static void RelativeBearingPropertyChangedCallback(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var control = (SensorAdfUserControl) d;
            var value = (double) e.NewValue;
            if (value > 999.0)
            {
                control.Arrow.Visibility = Visibility.Hidden;
            }
            else
            {
                var arrowAngle = value - 90.0;

                control.Arrow.RenderTransform = new RotateTransform(arrowAngle, 125, 125);
                control.Arrow.Visibility = Visibility.Visible;
            }
        }

        private static void MagHeadingPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (SensorAdfUserControl) d;
            var compassNumbers = control.CompassNumbers;
            var compassTicks = control.CompassTicks;
            var value = (double) e.NewValue;

            if (compassTicks != null)
            {
                compassTicks.RenderTransform =
                    value > 999.0 ? new RotateTransform(0, 125, 125) : new RotateTransform(-value, 125, 125);
            }

            if (compassNumbers != null)
            {
                if (value > 999.0)
                {
                    compassNumbers.Visibility = Visibility.Hidden;
                }
                else
                {
                    compassNumbers.RenderTransform = new RotateTransform(-value, 125, 125);

                    foreach (var c in compassNumbers.Children)
                    {
                        if (c is Border b)
                        {
                            var transGroup = (TransformGroup) b.LayoutTransform;
                            transGroup.Children[0] = new RotateTransform(value, 0, 0);
                        }
                    }

                    compassNumbers.Visibility = Visibility.Visible;
                }
            }
        }

        private void DrawCompass(double w, double h)
        {
            DrawNumbers(CompassNumbers, w, h);
            DrawTicks(CompassTicks, w, h);
        }

        private void DrawTicks(Panel canvas, double w, double h)
        {
            var lineBrush = Theme.GetResource(ThemeResourceKey.PrimaryColorBrush2) as SolidColorBrush;

            for (var i = 0; i < Division; i++)
            {
                var angle = (360.0 / Division) * i;
                double lineLength = i % (Division / MaxMark) == 0 ? 12 : 6;

                // Adjusted the Y1 and Y2 to give some space from the edge
                var line = new Line
                {
                    X1 = w * 0.5,
                    Y1 = 1, // Adjust this value
                    X2 = w * 0.5,
                    Y2 = 1 + lineLength, // Adjust this value
                    StrokeThickness = 1,
                    RenderTransform = new RotateTransform(angle, w / 2, w / 2),
                    Style = (Style)FindResource("LineStyleBody1")
                };
                canvas.Children.Add(line);

                //Arrow.Fill = lineBrush;
                Arrow.Style = (Style) FindResource("PathStyleBody1");
                Arrow.Effect = new DropShadowEffect
                {
                    BlurRadius = 10,
                    Opacity = 0.5,
                    ShadowDepth = 1
                };
            }
        }

        private void DrawNumbers(Panel canvas, double w, double h)
        {
            var lineBrush = Theme.GetResource(ThemeResourceKey.PrimaryColorBrush2) as SolidColorBrush;
            var angle = 0;

            foreach (var str in new[] { "N", "3", "6", "E", "12", "15", "S", "21", "24", "W", "30", "33" })
            {
                var border = new Border
                {
                    Width = 20,
                    Height = 20,
                };
                Canvas.SetLeft(border, w / 2);

                // Adjusted the TranslateTransform to give some space
                var transformGroup = new TransformGroup();
                transformGroup.Children.Add(new TranslateTransform(-10, 14)); // Adjust these values
                transformGroup.Children.Add(new RotateTransform(angle, 0, w / 2));
                border.RenderTransform = transformGroup;
                
                //var label = new Label
                //{
                //    HorizontalAlignment = HorizontalAlignment.Center,
                //    VerticalAlignment = VerticalAlignment.Center,
                //    Content = str,
                //    Style = (Style)FindResource("LabelStyleBody1"),
                //};

                var label = new Label()
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Content = str,
                    //Foreground = lineBrush,
                    Style = (Style)FindResource("LabelStyleBody1"),
                    FontSize = 14
                };

                if (str is "N" or "E" or "S" or "W")
                {
                    label.Foreground = Theme.GetResource(ThemeResourceKey.PrimaryColorBrush9) as SolidColorBrush;
                    label.FontWeight = FontWeights.Bold;
                    label.FontSize = 18;
                }

                var borderTransformGroup = new TransformGroup();
                borderTransformGroup.Children.Add(new RotateTransform(0, 0, 0));
                borderTransformGroup.Children.Add(new RotateTransform(-angle, 0, 0));
                border.LayoutTransform = borderTransformGroup;
                border.Child = label;

                canvas.Children.Add(border);
                angle += 360 / MaxMark;
            }
        }
    }
}