﻿using System;
using System.Timers;
using System.Windows;
using System.Windows.Controls;

namespace UI.WPFCore.Themes.Styles
{
    public partial class WindowStyles : ResourceDictionary
    {
        private TextBlock? _txtTimer;

        public WindowStyles()
        {
            InitializeComponent();
        }


        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            var window = (Window)((FrameworkElement)sender).TemplatedParent;

            window.Close();
        }

        private void OnMaximizeRestoreClick(object sender, RoutedEventArgs e)
        {
            var window = (Window)((FrameworkElement)sender).TemplatedParent;

            if (window.WindowState == WindowState.Normal)
            {
                window.WindowState = WindowState.Maximized;
            }
            else
            {
                window.WindowState = WindowState.Normal;
            }
        }

        private void OnMinimizeClick(object sender, RoutedEventArgs e)
        {
            var window = (Window)((FrameworkElement)sender).TemplatedParent;

            window.WindowState = WindowState.Minimized;
        }

        private void OnThemeClick(object sender, RoutedEventArgs e)
        {
            // Cycle the themes when the theme button is pressed.

            var enumCount = Enum.GetNames(typeof(ThemeType)).Length;
            var currentThemeIndex = (int)Theme.ThemeType;

            var newThemeIndex = currentThemeIndex + 1;
            if (newThemeIndex == enumCount-1) newThemeIndex = 0; //Skip dark (-1)

            var theme = (ThemeType)newThemeIndex;

            Theme.LoadThemeType(theme);
        }
        private void OnResizeToHdButtonClick(object sender, RoutedEventArgs e)
        {
            // For debug purpose. Set the window dimensions to the one in G2.
            var window = (Window)((FrameworkElement)sender).TemplatedParent;
            window.Width = 1080;
            window.Height = 1920;
        }
        
        private void txtTime_Loaded(object sender, RoutedEventArgs e)
        {
            _txtTimer = sender as TextBlock;

            var t = new Timer();
            
            t.Interval = 100;
            t.Elapsed += TimeElapsed!;
            t.Start();
        }

        private void TimeElapsed(object sender, ElapsedEventArgs e)
        {
            var txtBlock = sender as TextBlock;

            var time = DateTime.Now.ToShortDateString();
            time += " " + DateTime.Now.ToShortTimeString(); ;

            try
            {
                if (!_txtTimer.Dispatcher.HasShutdownStarted)
                    _txtTimer.Dispatcher.Invoke(() => _txtTimer.Text = time);
            }
            catch { }
        }

        private void ThemeButton_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

        }
    }
}
