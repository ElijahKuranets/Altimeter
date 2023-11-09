using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace UI.WPFCore.Converters;

public class InvertedBooleanToVisibilityConverter : MarkupExtension, IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        return (bool)value ? Visibility.Collapsed : Visibility.Visible;
    }
    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return this;
    }
}