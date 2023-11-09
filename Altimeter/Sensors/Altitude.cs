using System.ComponentModel;

namespace Dev.Sensors;

public class Altitude : INotifyPropertyChanged
{
    private float _score;

    public float Score
    {
        get => _score;
        set
        {
            _score = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Score)));
        }
    }

    public Altitude(float scr)
    {
        this.Score = scr;
    }

    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged;

    #endregion
}