using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;

public class TileTrackerClient : INotifyPropertyChanged
{
    public TileTrackerClient(ClassReferences refs)
    {
        refs.TileTrackerClient = this;
    }

    // shared by host
    public List<int> Discard;
    public List<int>[] DisplayRacks;
    public List<int> LocalPrivateRack;

    // only counts shared by host
    public int[] PrivateRackCounts;
    public int WallCount;

    // everything not shared by host (contents of Wall and PrivateRacks)
    public List<int> TilePool;

    public Dictionary<int, List<int>> TileLocations;

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

    // TODO: switch to custom event handlers to track each property separately
    public event PropertyChangedEventHandler PropertyChanged;

    void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void ReceiveGameState(int wallCount, int[] discard, int[] privateRack
        , int[] privateRackCounts, int[][] displayRacks)
    {
        WallCount = wallCount;
        Discard = discard.ToList();
        PrivateRack = privateRack.ToList();
        PrivateRackCounts = privateRackCounts;
        DisplayRacks = displayRacks.Select(arr => arr.ToList()).ToArray();
    }
}
