using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Nsm.UI.WPF.Converters
{
    /// <summary>
    /// Multiply a double value using a multiplier of type double.
    /// </summary>
    /// <example>
    /// ... Converter={converters:MultiplyValueConverter}, ConverterParameter=0.75 ...
    /// </example>
    [ValueConversion(typeof(double), typeof(double))]
    public class MultiplyValueConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var d = (double)value;
            double.TryParse(parameter.ToString(), out var multiplier);

            return d * multiplier;
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
