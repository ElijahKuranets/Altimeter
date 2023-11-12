using System;
using System.Windows.Controls;
using System.Windows.Threading;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Dev.Sensors
{
    public partial class SensorAltimeterUserControl : UserControl, INotifyPropertyChanged
    {
        private readonly DispatcherTimer _timer;
        private double _score;

        public event PropertyChangedEventHandler PropertyChanged;

        public SensorAltimeterUserControl()
        {
            InitializeComponent();
            _score = 0;
            AltimeterUserControl.DataContext = this;

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(0.1f)
            };
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        public double Score
        {
            get => _score;
            private set
            {
                if (_score != value)
                {
                    _score = value;
                    OnPropertyChanged(nameof(Score));
                }
            }
        }

        private async void Timer_Tick(object sender, EventArgs e)
        {
            if (Math.Abs(Score - 10000d) > float.Epsilon)
            {
                await Task.Run(() => Score += 1); 
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}