using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Altimeter.Sensors
{
    /// <summary>
    /// Interaction logic for SensorAltimeterUserControl.xaml
    /// </summary>
    public partial class SensorAltimeterUserControl : UserControl
    {
        private readonly DispatcherTimer _timer;
        private readonly Altitude _altitude;

        public SensorAltimeterUserControl()
        {
            InitializeComponent();
            _altitude = new Altitude(0);
            AltimeterUserControl.DataContext = _altitude;

            //Start the timer
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(0.025f)
            };
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        #region sensor altimeter dependency properties

        public static readonly DependencyProperty AltimeterHeightProperty =
            DependencyProperty.Register(nameof(SensorAltimeterHeight), typeof(string), typeof(SensorAltimeterUserControl));
        public static readonly DependencyProperty AltimeterWidthProperty =
            DependencyProperty.Register(nameof(SensorAltimeterWidth), typeof(string), typeof(SensorAltimeterUserControl));

        #endregion

        public string SensorAltimeterHeight
        {
            get => (string)GetValue(AltimeterHeightProperty);
            set => SetValue(AltimeterHeightProperty, value);
        }

        public string SensorAltimeterWidth
        {
            get => (string)GetValue(AltimeterWidthProperty);
            set => SetValue(AltimeterWidthProperty, value);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (Math.Abs(_altitude.Score - 10000d) > float.Epsilon)
            {
                _altitude.Score += 1;
            }
        }
    }
}
