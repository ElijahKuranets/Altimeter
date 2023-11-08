using System;
using System.Windows;
using System.Windows.Media;

namespace UI.WPFCore
{
    public sealed class Theme
    {
        private static readonly string aliceBlue = "#e7edf8";
        private static readonly string linkWater = "#c7d4e1";
        private static readonly string lightGrayishBlue = "#a7b6c8";
        private static readonly string baliHai = "#8799af";
        private static readonly string bermudaGrey = "#6f849c";
        private static readonly string kashmirBlue = "#576f89";
        private static readonly string mostlyDesaturatedDarkBlue = "#4a6179";
        private static readonly string slateBlue = "#3b4e62";
        private static readonly string deepBlue = "#2c3c4d";
        private static readonly string midnightBlue = "#1a2836";

        [ThreadStatic]
        private static ResourceDictionary? resourceDictionary;

        internal static ResourceDictionary ResourceDictionary
        {
            get
            {
                if (resourceDictionary != null)
                {
                    return resourceDictionary;
                }

                resourceDictionary = new ResourceDictionary();
                LoadThemeType(ThemeType.Default);
                return resourceDictionary;
            }
        }

        public static ThemeType ThemeType { get; set; } = ThemeType.Default;

        public static void LoadThemeType(ThemeType themeType)
        {
            ThemeType = themeType;
            // ****************************************
            // Margins and paddings
            // ****************************************
            SetResource(ThemeResourceKey.WindowHeaderMargin.ToString(), new Thickness(16, 0, 16, 0));
            SetResource(ThemeResourceKey.WindowMargin.ToString(), new Thickness(4, 0, 4, 4));
            SetResource(ThemeResourceKey.SecondWindowMargin.ToString(), new Thickness(4, 4, 4, 4));


            // ****************************************
            // Styleguide Primary Colors
            // ****************************************            
            var primaryColor1 = aliceBlue;
            var primaryColor2 = linkWater;
            var primaryColor3 = lightGrayishBlue;
            var primaryColor4 = baliHai;
            var primaryColor5 = bermudaGrey;
            var primaryColor6 = kashmirBlue;
            var primaryColor7 = mostlyDesaturatedDarkBlue;
            var primaryColor8 = slateBlue;
            var primaryColor9 = deepBlue;
            var primaryColor10 = midnightBlue;

            var secondaryColor1 = "#6bb2ff";
            var secondaryColor2 = "#00ffa3";
            var secondaryColor3 = "#ffeb1b";
            var secondaryColor4 = "#ff527b";

            var graphColor1 = "#00e5ff";
            var graphColor2 = "#ff00ff";
            var graphColor3 = "#fe6d6d";

            switch (themeType)
            {
                case ThemeType.Default:
                    {
                        break;
                    }
                case ThemeType.Green:
                    {
                        // Green
                        primaryColor1 = "#eaf0ea";
                        primaryColor2 = "#cad7ca";
                        primaryColor3 = "#a9bda9";
                        primaryColor4 = "#89a289";
                        primaryColor5 = "#718d71";
                        primaryColor6 = "#5c795c";
                        primaryColor7 = "#4e6c4e";
                        primaryColor8 = "#3b593b";
                        primaryColor9 = "#2c472c";
                        primaryColor10 = "#1a311a";

                        break;
                    }
                case ThemeType.Brown:
                    {
                        // Green
                        primaryColor1 = "#9a8478";
                        primaryColor2 = "#8b766b";
                        primaryColor3 = "#7c685e";
                        primaryColor4 = "#6d5b52";
                        primaryColor5 = "#5f4e45";
                        primaryColor6 = "#51413a";
                        primaryColor7 = "#43352e";
                        primaryColor8 = "#362923";
                        primaryColor9 = "#291e19";
                        primaryColor10 = "#1e130c";

                        break;
                    }
                case ThemeType.Frozen:
                    {
                        // Green
                        primaryColor1 = "#e7e9bb";
                        primaryColor2 = "#e0d0a8";
                        primaryColor3 = "#d6b899";
                        primaryColor4 = "#c8a28e";
                        primaryColor5 = "#b68d84";
                        primaryColor6 = "#a17a7b";
                        primaryColor7 = "#896872";
                        primaryColor8 = "#705866";
                        primaryColor9 = "#584959";
                        primaryColor10 = "#403b4a";

                        break;
                    }
                case ThemeType.DarkMode:
                    {
                        // Night mode
                        primaryColor10 = "#0f2027";
                        primaryColor9 = "#13262e";
                        primaryColor8 = "#172d35";
                        primaryColor7 = "#1c333c";
                        primaryColor6 = "#203a43";

                        primaryColor5 = "#223f49";
                        primaryColor4 = "#244450";
                        primaryColor3 = "#274956";
                        primaryColor2 = "#294e5d";

                        primaryColor1 = "#2c5364";

                        // Override secondary colors
                        secondaryColor3 = "#7F740E";

                        graphColor2 = "#7F740E";
                        break;
                    }
            }

            SetResource(ThemeResourceKey.PrimaryColor1.ToString(), ColorFromHex(primaryColor1));
            SetResource(ThemeResourceKey.PrimaryColor2.ToString(), ColorFromHex(primaryColor2));
            SetResource(ThemeResourceKey.PrimaryColor3.ToString(), ColorFromHex(primaryColor3));
            SetResource(ThemeResourceKey.PrimaryColor4.ToString(), ColorFromHex(primaryColor4));
            SetResource(ThemeResourceKey.PrimaryColor5.ToString(), ColorFromHex(primaryColor5));
            SetResource(ThemeResourceKey.PrimaryColor6.ToString(), ColorFromHex(primaryColor6));
            SetResource(ThemeResourceKey.PrimaryColor7.ToString(), ColorFromHex(primaryColor7));
            SetResource(ThemeResourceKey.PrimaryColor8.ToString(), ColorFromHex(primaryColor8));
            SetResource(ThemeResourceKey.PrimaryColor9.ToString(), ColorFromHex(primaryColor9));
            SetResource(ThemeResourceKey.PrimaryColor10.ToString(), ColorFromHex(primaryColor10));


            SetResource(ThemeResourceKey.PrimaryColorBrush1.ToString(), new SolidColorBrush((Color)GetResource(ThemeResourceKey.PrimaryColor1)));
            SetResource(ThemeResourceKey.PrimaryColorBrush2.ToString(), new SolidColorBrush((Color)GetResource(ThemeResourceKey.PrimaryColor2)));
            SetResource(ThemeResourceKey.PrimaryColorBrush3.ToString(), new SolidColorBrush((Color)GetResource(ThemeResourceKey.PrimaryColor3)));
            SetResource(ThemeResourceKey.PrimaryColorBrush4.ToString(), new SolidColorBrush((Color)GetResource(ThemeResourceKey.PrimaryColor4)));
            SetResource(ThemeResourceKey.PrimaryColorBrush5.ToString(), new SolidColorBrush((Color)GetResource(ThemeResourceKey.PrimaryColor5)));
            SetResource(ThemeResourceKey.PrimaryColorBrush6.ToString(), new SolidColorBrush((Color)GetResource(ThemeResourceKey.PrimaryColor6)));
            SetResource(ThemeResourceKey.PrimaryColorBrush7.ToString(), new SolidColorBrush((Color)GetResource(ThemeResourceKey.PrimaryColor7)));
            SetResource(ThemeResourceKey.PrimaryColorBrush8.ToString(), new SolidColorBrush((Color)GetResource(ThemeResourceKey.PrimaryColor8)));
            SetResource(ThemeResourceKey.PrimaryColorBrush9.ToString(), new SolidColorBrush((Color)GetResource(ThemeResourceKey.PrimaryColor9)));
            SetResource(ThemeResourceKey.PrimaryColorBrush10.ToString(), new SolidColorBrush((Color)GetResource(ThemeResourceKey.PrimaryColor10)));

            SetResource(ThemeResourceKey.PrimaryColorBrushFaded1.ToString(), new SolidColorBrush((Color)GetResource(ThemeResourceKey.PrimaryColor1)) { Opacity = 0.3 });
            SetResource(ThemeResourceKey.PrimaryColorBrushFaded2.ToString(), new SolidColorBrush((Color)GetResource(ThemeResourceKey.PrimaryColor2)) { Opacity = 0.3 });
            SetResource(ThemeResourceKey.PrimaryColorBrushFaded3.ToString(), new SolidColorBrush((Color)GetResource(ThemeResourceKey.PrimaryColor3)) { Opacity = 0.3 });
            SetResource(ThemeResourceKey.PrimaryColorBrushFaded4.ToString(), new SolidColorBrush((Color)GetResource(ThemeResourceKey.PrimaryColor4)) { Opacity = 0.3 });
            SetResource(ThemeResourceKey.PrimaryColorBrushFaded5.ToString(), new SolidColorBrush((Color)GetResource(ThemeResourceKey.PrimaryColor5)) { Opacity = 0.3 });
            SetResource(ThemeResourceKey.PrimaryColorBrushFaded6.ToString(), new SolidColorBrush((Color)GetResource(ThemeResourceKey.PrimaryColor6)) { Opacity = 0.3 });
            SetResource(ThemeResourceKey.PrimaryColorBrushFaded7.ToString(), new SolidColorBrush((Color)GetResource(ThemeResourceKey.PrimaryColor7)) { Opacity = 0.3 });
            SetResource(ThemeResourceKey.PrimaryColorBrushFaded8.ToString(), new SolidColorBrush((Color)GetResource(ThemeResourceKey.PrimaryColor8)) { Opacity = 0.3 });
            SetResource(ThemeResourceKey.PrimaryColorBrushFaded9.ToString(), new SolidColorBrush((Color)GetResource(ThemeResourceKey.PrimaryColor9)) { Opacity = 0.3 });
            SetResource(ThemeResourceKey.PrimaryColorBrushFaded10.ToString(), new SolidColorBrush((Color)GetResource(ThemeResourceKey.PrimaryColor10)) { Opacity = 0.3 });

            SetResource(ThemeResourceKey.RegularTextColor.ToString(), new SolidColorBrush(ColorFromHex("#c7d4e1")));
            SetResource(ThemeResourceKey.DynamicTextColor.ToString(), new SolidColorBrush(ColorFromHex("#6bb2ff")));
            SetResource(ThemeResourceKey.ActiveTextColor.ToString(), new SolidColorBrush(ColorFromHex(aliceBlue)));
            SetResource(ThemeResourceKey.InfoTextColor.ToString(), new SolidColorBrush(ColorFromHex("#A0B0C2")));


            // ****************************************
            // Styleguide Secondary Colors
            // ****************************************
            SetResource(ThemeResourceKey.SecondaryColor1.ToString(), ColorFromHex(secondaryColor1));
            SetResource(ThemeResourceKey.SecondaryColor2.ToString(), ColorFromHex(secondaryColor2));
            SetResource(ThemeResourceKey.SecondaryColor3.ToString(), ColorFromHex(secondaryColor3));
            SetResource(ThemeResourceKey.SecondaryColor4.ToString(), ColorFromHex(secondaryColor4));

            SetResource(ThemeResourceKey.SecondaryColorBrush1.ToString(), new SolidColorBrush((Color)GetResource(ThemeResourceKey.SecondaryColor1)));
            SetResource(ThemeResourceKey.SecondaryColorBrush2.ToString(), new SolidColorBrush((Color)GetResource(ThemeResourceKey.SecondaryColor2)));
            SetResource(ThemeResourceKey.SecondaryColorBrush3.ToString(), new SolidColorBrush((Color)GetResource(ThemeResourceKey.SecondaryColor3)));
            SetResource(ThemeResourceKey.SecondaryColorBrush4.ToString(), new SolidColorBrush((Color)GetResource(ThemeResourceKey.SecondaryColor4)));

            SetResource(ThemeResourceKey.FlagColorBrush.ToString(), new SolidColorBrush((Color)GetResource(ThemeResourceKey.SecondaryColor4)));

            SetResource(ThemeResourceKey.GraphColor1.ToString(), ColorFromHex(graphColor1));
            SetResource(ThemeResourceKey.GraphColor2.ToString(), ColorFromHex(graphColor2));
            SetResource(ThemeResourceKey.GraphColor3.ToString(), ColorFromHex(graphColor3));

            SetResource(ThemeResourceKey.GraphColorBrush1.ToString(), new SolidColorBrush((Color)GetResource(ThemeResourceKey.GraphColor1)));
            SetResource(ThemeResourceKey.GraphColorBrush2.ToString(), new SolidColorBrush((Color)GetResource(ThemeResourceKey.GraphColor2)));
            SetResource(ThemeResourceKey.GraphColorBrush3.ToString(), new SolidColorBrush((Color)GetResource(ThemeResourceKey.GraphColor3)));

            // ****************************************H2FontWeight
            // Fonts
            // ****************************************

            //Connect fonts from resources. TTF files are placed in two folders. Fonts/B612 is folder for B612 fontfamily, Fonts/B612Mono is folder for B612 mono.
            //The FontFamolies are created in Themes/Fonts/Fonts.xaml resource dictionary
            var fontsResourceDictionary = new ResourceDictionary
            {
                Source = new Uri("/UI.WPFCore;component/Themes/Fonts.xaml", UriKind.RelativeOrAbsolute)
            };
            var b612 = fontsResourceDictionary["B612"] as FontFamily;
            SetResource(ThemeResourceKey.DefaultFontFamily.ToString(), b612);
            var b612Mono = fontsResourceDictionary["B612Mono"] as FontFamily;
            SetResource(ThemeResourceKey.DefaultMonoFontFamily.ToString(), b612Mono);

            SetResource(ThemeResourceKey.RegularTextFontFamily.ToString(), GetResource(ThemeResourceKey.DefaultFontFamily));
            SetResource(ThemeResourceKey.DynamicTextFontFamily.ToString(), GetResource(ThemeResourceKey.DefaultFontFamily));
            SetResource(ThemeResourceKey.ActiveTextFontFamily.ToString(), GetResource(ThemeResourceKey.DefaultFontFamily));

            SetResource(ThemeResourceKey.ValueTextFontFamily.ToString(), GetResource(ThemeResourceKey.DefaultMonoFontFamily));
            SetResource(ThemeResourceKey.ValueTextFontWeight.ToString(), FontWeights.Regular);

            SetResource(ThemeResourceKey.RegularTextFontWeight.ToString(), FontWeights.Regular);
            SetResource(ThemeResourceKey.DynamicTextFontWeight.ToString(), FontWeights.Regular);
            SetResource(ThemeResourceKey.ActiveTextFontWeight.ToString(), FontWeights.SemiBold);


            SetResource(ThemeResourceKey.WindowTitleFontFamily.ToString(), GetResource(ThemeResourceKey.DefaultFontFamily));
            SetResource(ThemeResourceKey.WindowTitleFontSize.ToString(), 20.0d);
            SetResource(ThemeResourceKey.WindowTitleFontWeight.ToString(), FontWeights.Regular);

            SetResource(ThemeResourceKey.Button1FontFamily.ToString(), GetResource(ThemeResourceKey.DefaultFontFamily));
            SetResource(ThemeResourceKey.Button1FontWeight.ToString(), FontWeights.Regular);
            SetResource(ThemeResourceKey.Button1FontColor.ToString(), GetResource(ThemeResourceKey.PrimaryColorBrush1));
            SetResource(ThemeResourceKey.Button1FontSize.ToString(), 16.0d);

            SetResource(ThemeResourceKey.Button2FontFamily.ToString(), GetResource(ThemeResourceKey.DefaultFontFamily));
            SetResource(ThemeResourceKey.Button2FontWeight.ToString(), FontWeights.Regular);
            SetResource(ThemeResourceKey.Button2FontColor.ToString(), GetResource(ThemeResourceKey.PrimaryColorBrush1));
            SetResource(ThemeResourceKey.Button2FontSize.ToString(), 14.0d);

            SetResource(ThemeResourceKey.UIButtonFontFamily.ToString(), GetResource(ThemeResourceKey.DefaultFontFamily));
            SetResource(ThemeResourceKey.UIButtonFontWeight.ToString(), FontWeights.Regular);
            SetResource(ThemeResourceKey.UIButtonFontColor.ToString(), GetResource(ThemeResourceKey.PrimaryColorBrush2));
            SetResource(ThemeResourceKey.UIButtonFontSize.ToString(), 16.0d);

            SetResource(ThemeResourceKey.H1FontFamily.ToString(), GetResource(ThemeResourceKey.DefaultFontFamily));
            SetResource(ThemeResourceKey.H1FontSize.ToString(), 16.0d);
            SetResource(ThemeResourceKey.H1FontWeight.ToString(), FontWeights.Regular);
            SetResource(ThemeResourceKey.H1FontColor.ToString(), GetResource(ThemeResourceKey.PrimaryColorBrush3));

            SetResource(ThemeResourceKey.H2FontFamily.ToString(), GetResource(ThemeResourceKey.DefaultFontFamily));
            SetResource(ThemeResourceKey.H2FontSize.ToString(), 14.0d);
            SetResource(ThemeResourceKey.H2FontWeight.ToString(), FontWeights.Regular);
            SetResource(ThemeResourceKey.H2FontColor.ToString(), GetResource(ThemeResourceKey.PrimaryColorBrush4));

            SetResource(ThemeResourceKey.H3FontFamily.ToString(), GetResource(ThemeResourceKey.DefaultFontFamily));
            SetResource(ThemeResourceKey.H3FontSize.ToString(), 14.0d);
            SetResource(ThemeResourceKey.H3FontWeight.ToString(), FontWeights.Regular);
            SetResource(ThemeResourceKey.H3FontColor.ToString(), GetResource(ThemeResourceKey.PrimaryColorBrush5));

            SetResource(ThemeResourceKey.H4FontColor.ToString(), GetResource(ThemeResourceKey.PrimaryColorBrush6));

            SetResource(ThemeResourceKey.Body1FontFamily.ToString(), GetResource(ThemeResourceKey.DefaultFontFamily));
            SetResource(ThemeResourceKey.Body1FontSize.ToString(), 18.0d);
            SetResource(ThemeResourceKey.Body1FontWeight.ToString(), FontWeights.Regular);
            SetResource(ThemeResourceKey.Body1FontColor.ToString(), GetResource(ThemeResourceKey.PrimaryColorBrush2));

            SetResource(ThemeResourceKey.Body2FontFamily.ToString(), GetResource(ThemeResourceKey.DefaultFontFamily));
            SetResource(ThemeResourceKey.Body2FontSize.ToString(), 16.0d);
            SetResource(ThemeResourceKey.Body2FontWeight.ToString(), FontWeights.Regular);
            SetResource(ThemeResourceKey.Body2FontColor.ToString(), GetResource(ThemeResourceKey.PrimaryColorBrush2));

            SetResource(ThemeResourceKey.Body3FontFamily.ToString(), GetResource(ThemeResourceKey.DefaultFontFamily));
            SetResource(ThemeResourceKey.Body3FontSize.ToString(), 14.0d);
            SetResource(ThemeResourceKey.Body3FontWeight.ToString(), FontWeights.Regular);
            SetResource(ThemeResourceKey.Body3FontColor.ToString(), GetResource(ThemeResourceKey.PrimaryColorBrush4));

            SetResource(ThemeResourceKey.Values1FontFamily.ToString(), GetResource(ThemeResourceKey.DefaultMonoFontFamily));
            SetResource(ThemeResourceKey.Values1FontSize.ToString(), 18.0d);
            SetResource(ThemeResourceKey.Values1FontWeight.ToString(), FontWeights.Regular);
            SetResource(ThemeResourceKey.Values1FontColor.ToString(), GetResource(ThemeResourceKey.PrimaryColorBrush1));

            SetResource(ThemeResourceKey.Values2FontFamily.ToString(), GetResource(ThemeResourceKey.DefaultMonoFontFamily));
            SetResource(ThemeResourceKey.Values2FontSize.ToString(), 16.0d);
            SetResource(ThemeResourceKey.Values2FontWeight.ToString(), FontWeights.Regular);
            SetResource(ThemeResourceKey.Values2FontColor.ToString(), GetResource(ThemeResourceKey.PrimaryColorBrush2));

            // ****************************************
            // Dimensions
            // ****************************************
            SetResource(ThemeResourceKey.DefaultControlHeight.ToString(), 46.0d);
            SetResource(ThemeResourceKey.DefaultControlWidth.ToString(), 92.0d);
            SetResource(ThemeResourceKey.DefaultSmallControlWidth.ToString(), 46.0d);
            SetResource(ThemeResourceKey.DefaultLargeControlWidth.ToString(), 138.0d);

            // ****************************************
            // Window Colors
            // ****************************************
            SetResource(ThemeResourceKey.WindowHeaderBackground.ToString(), GetResource(ThemeResourceKey.PrimaryColorBrush10));
            //SetResource(ThemeResourceKey.WindowHeaderForeground.ToString(), new SolidColorBrush(ColorFromHex("#FFFFFFFF")));
            SetResource(ThemeResourceKey.WindowHeaderForeground.ToString(), GetResource(ThemeResourceKey.PrimaryColorBrush1));
            SetResource(ThemeResourceKey.WindowActiveBorder.ToString(), GetResource(ThemeResourceKey.PrimaryColorBrush10));
            SetResource(ThemeResourceKey.WindowBorder.ToString(), GetResource(ThemeResourceKey.PrimaryColorBrush10));
            SetResource(ThemeResourceKey.WindowControlMouseOverBackground.ToString(), new SolidColorBrush(ColorFromHex("#FF000000")));

            // ****************************************
            // App Colors
            // ****************************************
            SetResource(ThemeResourceKey.Background.ToString(), GetResource(ThemeResourceKey.PrimaryColorBrush8));
            SetResource(ThemeResourceKey.GlyphForeground.ToString(), GetResource(ThemeResourceKey.PrimaryColorBrush10));

            SetResource(ThemeResourceKey.ControlBackground.ToString(), GetResource(ThemeResourceKey.PrimaryColorBrush5));
            SetResource(ThemeResourceKey.ControlBorder.ToString(), GetResource(ThemeResourceKey.PrimaryColorBrush4));
            SetResource(ThemeResourceKey.ControlForeground.ToString(), GetResource(ThemeResourceKey.PrimaryColorBrush1));
            SetResource(ThemeResourceKey.ControlHighlightBackground.ToString(), GetResource(ThemeResourceKey.PrimaryColorBrush3));

            SetResource(ThemeResourceKey.ControlFocusBorder.ToString(), GetResource(ThemeResourceKey.SecondaryColorBrush1));
            SetResource(ThemeResourceKey.ControlMouseOverBackground.ToString(), GetResource(ThemeResourceKey.PrimaryColorBrush9));
            SetResource(ThemeResourceKey.ControlMouseOverForeground.ToString(), GetResource(ThemeResourceKey.PrimaryColorBrush1));
            SetResource(ThemeResourceKey.ControlMouseOverBorder.ToString(), GetResource(ThemeResourceKey.PrimaryColorBrush9));
            SetResource(ThemeResourceKey.ControlPressedBackground.ToString(), GetResource(ThemeResourceKey.PrimaryColorBrush9));
            SetResource(ThemeResourceKey.ControlPressedBorder.ToString(), GetResource(ThemeResourceKey.PrimaryColorBrush9));

            SetResource(ThemeResourceKey.ListSelectedBackground.ToString(), GetResource(ThemeResourceKey.PrimaryColorBrush8));
            SetResource(ThemeResourceKey.ListSelectedBorder.ToString(), GetResource(ThemeResourceKey.SecondaryColorBrush1));
            SetResource(ThemeResourceKey.ListSelectedForeground.ToString(), GetResource(ThemeResourceKey.PrimaryColorBrush1));
            SetResource(ThemeResourceKey.ListMouseOverBackground.ToString(), GetResource(ThemeResourceKey.PrimaryColorBrush7));
            SetResource(ThemeResourceKey.ListMouseOverBorder.ToString(), GetResource(ThemeResourceKey.PrimaryColorBrush4));

            //colors for different sattelites systems in GNSSkyView
            SetResource(ThemeResourceKey.SatelliteGpsColor.ToString(), new SolidColorBrush(ColorFromHex("#00FFA3")));
            SetResource(ThemeResourceKey.SatelliteSBASColor.ToString(), new SolidColorBrush(ColorFromHex("#FFEB1B")));
            SetResource(ThemeResourceKey.SatelliteGlonassColor.ToString(), new SolidColorBrush(ColorFromHex("#FF537B")));
            SetResource(ThemeResourceKey.SatelliteGalileoColor.ToString(), new SolidColorBrush(ColorFromHex("#6BB3FF")));
            SetResource(ThemeResourceKey.SatelliteQZSSColor.ToString(), new SolidColorBrush(ColorFromHex("#098d5d")));
            SetResource(ThemeResourceKey.SatelliteBeiDouColor.ToString(), new SolidColorBrush(ColorFromHex("#A77FE0")));
            SetResource(ThemeResourceKey.SatelliteMSSColor.ToString(), new SolidColorBrush(ColorFromHex("#BDE17F")));
            SetResource(ThemeResourceKey.SatelliteUnknownColor.ToString(), new SolidColorBrush(ColorFromHex("#0b4e97")));
            SetResource(ThemeResourceKey.SatelliteUnhealthyColor.ToString(), new SolidColorBrush(ColorFromHex("#FF708090")));


            switch (themeType)
            {
                case ThemeType.Default:
                    {
                        SetResource(ThemeResourceKey.ContentBackground.ToString(), GetResource(ThemeResourceKey.PrimaryColorBrush10));
                        break;
                    }
                case ThemeType.Green:
                    {
                        SetResource(ThemeResourceKey.ContentBackground.ToString(), GetResource(ThemeResourceKey.PrimaryColorBrush10));
                        break;
                    }
                case ThemeType.Brown:
                    {
                        SetResource(ThemeResourceKey.ContentBackground.ToString(), GetResource(ThemeResourceKey.PrimaryColorBrush10));
                        break;
                    }
                case ThemeType.Frozen:
                    {
                        SetResource(ThemeResourceKey.ContentBackground.ToString(), GetResource(ThemeResourceKey.PrimaryColorBrush10));
                        break;
                    }
                case ThemeType.DarkMode:
                    {
                        // Override font colors
                        // SetResource(ThemeResourceKey.H1FontColor.ToString(), GetResource(ThemeResourceKey.PrimaryColorBrush3));
                        SetResource(ThemeResourceKey.H1FontColor.ToString(), new SolidColorBrush((Color)ColorFromHex("#BB86FC")));

                        // SetResource(ThemeResourceKey.H2FontColor.ToString(), GetResource(ThemeResourceKey.PrimaryColorBrush4));
                        SetResource(ThemeResourceKey.H2FontColor.ToString(), new SolidColorBrush((Color)ColorFromHex("#7F39FB")));

                        // SetResource(ThemeResourceKey.H3FontColor.ToString(), GetResource(ThemeResourceKey.PrimaryColorBrush5));
                        SetResource(ThemeResourceKey.H3FontColor.ToString(), new SolidColorBrush((Color)ColorFromHex("#6200EE")));

                        // SetResource(ThemeResourceKey.Body1FontColor.ToString(), GetResource(ThemeResourceKey.PrimaryColorBrush2));
                        SetResource(ThemeResourceKey.Body1FontColor.ToString(), new SolidColorBrush((Color)ColorFromHex("#BB86FC")));

                        // SetResource(ThemeResourceKey.Body2FontColor.ToString(), GetResource(ThemeResourceKey.PrimaryColorBrush2));
                        SetResource(ThemeResourceKey.Body2FontColor.ToString(), new SolidColorBrush((Color)ColorFromHex("#BB86FC")));

                        // SetResource(ThemeResourceKey.Values1FontColor.ToString(), GetResource(ThemeResourceKey.PrimaryColorBrush1));
                        SetResource(ThemeResourceKey.Values1FontColor.ToString(), new SolidColorBrush((Color)ColorFromHex("#DBB2FF")));

                        // SetResource(ThemeResourceKey.Values2FontColor.ToString(), GetResource(ThemeResourceKey.PrimaryColorBrush2));
                        SetResource(ThemeResourceKey.Values2FontColor.ToString(), new SolidColorBrush((Color)ColorFromHex("#BB86FC")));

                        // SetResource(ThemeResourceKey.Button1FontColor.ToString(), GetResource(ThemeResourceKey.PrimaryColorBrush1));
                        SetResource(ThemeResourceKey.Button1FontColor.ToString(), new SolidColorBrush((Color)ColorFromHex("#BB86FC")));

                        // SetResource(ThemeResourceKey.Button2FontColor.ToString(), GetResource(ThemeResourceKey.PrimaryColorBrush1));
                        SetResource(ThemeResourceKey.Button2FontColor.ToString(), new SolidColorBrush((Color)ColorFromHex("#BB86FC")));

                        //SetResource(ThemeResourceKey.RegularTextColor.ToString(), new SolidColorBrush(ColorFromHex("#c7d4e1")));
                        //SetResource(ThemeResourceKey.DynamicTextColor.ToString(), new SolidColorBrush(ColorFromHex("#6bb2ff")));
                        //SetResource(ThemeResourceKey.ActiveTextColor.ToString(), new SolidColorBrush(ColorFromHex(aliceBlue)));
                        //SetResource(ThemeResourceKey.InfoTextColor.ToString(), new SolidColorBrush(ColorFromHex("#A0B0C2")));
                        SetResource(ThemeResourceKey.RegularTextColor.ToString(), new SolidColorBrush(ColorFromHex("#BB86FC")));
                        SetResource(ThemeResourceKey.DynamicTextColor.ToString(), new SolidColorBrush(ColorFromHex("#BB86FC")));
                        SetResource(ThemeResourceKey.ActiveTextColor.ToString(), new SolidColorBrush(ColorFromHex("#BB86FC")));
                        SetResource(ThemeResourceKey.InfoTextColor.ToString(), new SolidColorBrush(ColorFromHex("#BB86FC")));

                        break;
                    }
            }
        }

        public static object? GetResource(ThemeResourceKey resourceKey)
        {
            return ResourceDictionary.Contains(resourceKey.ToString()) ?
                ResourceDictionary[resourceKey.ToString()] : null;
        }

        internal static void SetResource(object key, object resource)
        {
            ResourceDictionary[key] = resource;
        }

        internal static Color ColorFromHex(string colorHex)
        {
            return (Color?)ColorConverter.ConvertFromString(colorHex) ?? Colors.Transparent;
        }

        private static LinearGradientBrush GetWindowHeaderGradient()
        {
            return new LinearGradientBrush
            {
                StartPoint = new Point(0, 0.5),
                EndPoint = new Point(1, 0.5),
                GradientStops = new GradientStopCollection
                    {
                        new GradientStop { Offset = 0, Color = ColorFromHex("#FF833AB4") },
                        new GradientStop { Offset = 1, Color = ColorFromHex("#FFFD1D1D") }
                    }
            };
        }

    }
}
