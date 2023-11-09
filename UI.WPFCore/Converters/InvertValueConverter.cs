using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace UI.WPFCore.Converters
{
    /// <summary>
    /// InvertValueConverter converts a double value inverted.
    /// </summary>
    [ValueConversion(typeof(double), typeof(double))]
    public class InvertValueConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var d = (double)value;
            return d * -1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    } 

}
