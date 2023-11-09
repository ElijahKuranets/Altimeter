using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace UI.WPFCore.Converters
{
    /// <summary>
    /// CollapseVisibilityValueConverter will set visibility to Collapse if Visibility is Hidden 
    /// </summary>
    [ValueConversion(typeof(Visibility), typeof(Visibility))]
    public class CollapseVisibilityValueConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var v = (Visibility)value!;
            return v == Visibility.Hidden ? Visibility.Collapsed : v;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
