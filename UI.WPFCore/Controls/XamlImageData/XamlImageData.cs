using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace UI.WPFCore.Controls
{
    public class XamlImageData : Control
    {
        static XamlImageData()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(XamlImageData), new FrameworkPropertyMetadata(typeof(XamlImageData)));
            IsTabStopProperty.OverrideMetadata(typeof(XamlImageData), new FrameworkPropertyMetadata(false));
        }

        public static readonly DependencyProperty DataProperty = DependencyProperty.Register(
            nameof(Data), typeof(Geometry), typeof(XamlImageData), new PropertyMetadata(default(Geometry)));

        public Geometry Data
        {
            get => (Geometry)GetValue(DataProperty);
            set => SetValue(DataProperty, value);
        }
    }
}
