using System.Windows;
using System.Windows.Controls;

namespace UI.WPFCore.AttachedProperties
{
    /// <summary>
    /// Dependency properties for Buttons.
    /// </summary>
    public static class ButtonProperties
    {
        #region CornerRadius (Attached Property)
        /// <summary>
        /// Identifies the CornerRadius property.
        /// </summary>
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.RegisterAttached(
                "CornerRadius",
                typeof(double),
                typeof(ButtonProperties),
                new FrameworkPropertyMetadata(0d));

        /// <summary>
        /// Gets the corner radius.
        /// </summary>
        public static double GetCornerRadius(DependencyObject obj)
        {
            return (double)obj.GetValue(CornerRadiusProperty);
        }

        /// <summary>
        /// Sets the corner radius.
        /// </summary>
        public static void SetCornerRadius(DependencyObject obj, double value)
        {
            obj.SetValue(CornerRadiusProperty, value);
        }
        #endregion

        #region Icon (Attached Property)
        /// <summary>
        /// Identifies the Icon property.
        /// </summary>
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.RegisterAttached(
                "Icon",
                typeof(ControlTemplate),
                typeof(ButtonProperties),
                new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Gets the Icon.
        /// </summary>
        public static ControlTemplate GetIcon(DependencyObject obj)
        {
            return (ControlTemplate)obj.GetValue(IconProperty);
        }

        /// <summary>
        /// Sets the Icon.
        /// </summary>
        public static void SetIcon(DependencyObject obj, ControlTemplate value)
        {
            obj.SetValue(IconProperty, value);
        }

        #endregion
    }
}