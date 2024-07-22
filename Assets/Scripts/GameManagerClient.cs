using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class GameManagerClient : INotifyPropertyChanged
{
    private List<int> privateRack;
    public List<int> PrivateRack
    {
        get => privateRack;
        set
        {
            if (privateRack != value)
            {
                privateRack = value;
                OnPropertyChanged();
            }
        }
    }

    private List<int> displayRack;
    public List<int> DisplayRack
    {
        get => displayRack;
        set
        {
            if (displayRack != value)
            {
                displayRack = value;
                OnPropertyChanged();
            }
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
