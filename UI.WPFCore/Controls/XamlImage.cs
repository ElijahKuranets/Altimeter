using System.Windows;
using System.Windows.Controls;

namespace UI.WPFCore.Controls
{
    /// <summary>
    /// Obsolete.
    /// </summary>
    public class XamlImage : Control
    {
        static XamlImage()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(XamlImage), 
                new FrameworkPropertyMetadata(typeof(XamlImage)));

            IsTabStopProperty.OverrideMetadata(typeof(XamlImage), 
                new FrameworkPropertyMetadata(false));
        }
    }
}
