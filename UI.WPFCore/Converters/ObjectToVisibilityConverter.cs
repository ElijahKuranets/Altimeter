using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace UI.WPFCore.Converters
{
    public class ObjectToVisibilityConverter : MarkupExtension, IValueConverter
    {
        public bool Inverse { get; set; }

        public ObjectToVisibilityConverter()
        {
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (Inverse)
            {
                return value != null ? Visibility.Collapsed : Visibility.Visible;
            } else
            {
                return value == null ? Visibility.Collapsed : Visibility.Visible;
            }
            
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
