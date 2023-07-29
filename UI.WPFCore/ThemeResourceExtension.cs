using System;
using System.Windows;

namespace UI.WPFCore
{
    public sealed class ThemeResourceExtension : DynamicResourceExtension
    {
        public new ThemeResourceKey ResourceKey
        {
            get
            {
                _ = Enum.TryParse(base.ResourceKey.ToString(), out ThemeResourceKey resourceKey);
                return resourceKey;
            }
            set => base.ResourceKey = value.ToString();
        }
    }
}
