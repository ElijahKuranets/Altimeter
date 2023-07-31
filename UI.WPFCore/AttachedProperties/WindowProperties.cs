using System.Windows;

namespace UI.WPFCore.AttachedProperties
{
    public static class WindowProperties
    {
        #region DisplayHeader (Attached Property)
        /// <summary>
        /// Identifies the DisplayHeader property.
        /// </summary>
        public static readonly DependencyProperty DisplayHeaderProperty =
            DependencyProperty.RegisterAttached(
                "DisplayHeader",
                typeof(bool),
                typeof(WindowProperties),
                new FrameworkPropertyMetadata(true));

        /// <summary>
        /// Gets the corner radius.
        /// </summary>
        public static bool GetDisplayHeader(DependencyObject obj)
        {
            return (bool)obj.GetValue(DisplayHeaderProperty);
        }

        /// <summary>
        /// Sets the corner radius.
        /// </summary>
        public static void SetDisplayHeader(DependencyObject obj, bool value)
        {
            obj.SetValue(DisplayHeaderProperty, value);
        }
        #endregion

        #region DisplayHeaderButtons (Attached Property)
        /// <summary>
        /// Identifies the DisplayHeaderButtons property.
        /// </summary>
        public static readonly DependencyProperty DisplayHeaderButtonsProperty =
            DependencyProperty.RegisterAttached(
                "DisplayHeaderButtons",
                typeof(bool),
                typeof(WindowProperties),
                new FrameworkPropertyMetadata(true));

        /// <summary>
        /// Gets the corner radius.
        /// </summary>
        public static bool GetDisplayHeaderButtons(DependencyObject obj)
        {
            return (bool)obj.GetValue(DisplayHeaderButtonsProperty);
        }

        /// <summary>
        /// Sets the corner radius.
        /// </summary>
        public static void SetDisplayHeaderButtons(DependencyObject obj, bool value)
        {
            obj.SetValue(DisplayHeaderButtonsProperty, value);
        }
        #endregion
    }
}