using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;

public class TileTrackerClient
{
    private ClassReferences refs;
    private IMonoWrapper mono;

    // lists owned and shared by the host
    public ObservableCollection<int> Discard = new();
    public List<ObservableCollection<int>> DisplayRacks = new() { new(), new(), new(), new() };
    public ObservableCollection<int> LocalPrivateRack = new();

    // counts shared by host (for lists of hidden tiles)
    public int[] PrivateRackCounts = new int[4];
    public int WallCount;

    // everything not shared by host (contents of Wall and PrivateRacks)
    public List<int> TilePool;

    // initiated in SetupClient
    public TileTrackerClient(ClassReferences refs)
    {
        refs.TileTrackerClient = this;
        this.refs = refs;
        mono = refs.Mono;
        Discard.CollectionChanged += DiscardChanged;
        foreach (ObservableCollection<int> rack in DisplayRacks)
        {
            rack.CollectionChanged += DisplayRacksChanged;
        }
        LocalPrivateRack.CollectionChanged += LocalPrivateRackChanged;
    }


    public void ReceiveGameState(int newWallCount, int[] newDiscard, int[] newPrivateRack
        , int[] newPrivateRackCounts, int[] newDisplayRack0, int[] newDisplayRack1
        , int[] newDisplayRack2, int[] newDisplayRack3)
    {
        int[][] newDisplayRacks = new int[][] { newDisplayRack0, newDisplayRack1, newDisplayRack2, newDisplayRack3 };

        WallCount = newWallCount;
        ApplyAdd(newDiscard, Discard);
        for (int i = 0; i < 4; i++)
        {
            ApplyAdd(newDisplayRacks[i], DisplayRacks[i]);
        }

        if (Changed(newPrivateRack, LocalPrivateRack))
        {
            ReceiveRackUpdate(newPrivateRack);
        }
        PrivateRackCounts = newPrivateRackCounts;

        bool Changed(int[] newList, ObservableCollection<int> curList)
        {
            return !curList.SequenceEqual(new ObservableCollection<int>(newList));
        }

        void ApplyAdd(int[] newList, ObservableCollection<int> curList)
        {
            // check that the already-existing items didn't change
            for (int i = 0; i < curList.Count; i++)
            {
                Debug.Assert(newList[i] == curList[i]);
            }

            for (int i = curList.Count; i < newList.Count(); i++)
            {
                curList.Add(newList[i]);
            }
        }

    }

    public void ReceiveRackUpdate(int[] newRack)
    {
        // we don't want to just reassign the whole list because that makes observable collections harder to use
        // analyze differences between the incoming list and the existing list and apply them.
        // note: we don't care about the order of newRack  or therefore any "move" actions because it's none of the 
        // server's business what order the client keeps their tiles in

        ObservableCollection<int> curRack = LocalPrivateRack;

        // removals
        foreach (int tileId in curRack)
        {
            if (!newRack.Contains(tileId)) curRack.Remove(tileId);
        }

        // additions
        foreach (int tileId in newRack)
        {
            if (!curRack.Contains(tileId)) curRack.Add(tileId);
        }
    }

    void DiscardChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        // the only event that should happen to discard is add (handle calling later)
        Debug.Assert(sender == Discard);
        Debug.Assert(e.Action == NotifyCollectionChangedAction.Add);

        int tileId = (int)e.NewItems[0];
        mono.MoveTile(tileId, MonoObject.Discard);
    }

    void LocalPrivateRackChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        UnityEngine.Debug.Log("TileTrackerClient.LocalPrivateRackChanged");
        // TODO: use Moved when rearranging tiles
        mono.UpdateRack(new List<int>(LocalPrivateRack));
    }

    void DisplayRacksChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        // the only events that should happen to display racks are Add or Replace
        Debug.Assert(DisplayRacks.Contains(sender));
        Debug.Assert(e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Replace);

        // player exposes
        if (e.Action == NotifyCollectionChangedAction.Add)
        {
            ObservableCollection<int> rack = (ObservableCollection<int>)sender;
            int tileId = (int)e.NewItems[0];
            mono.ExposeOtherPlayerTile(DisplayRacks.IndexOf(rack), tileId);
        }

        // player exchanges a joker
        if (e.Action == NotifyCollectionChangedAction.Replace)
        {
            throw new NotImplementedException();
        }

    }


    /*
    void VerifyWallCount(int newWallCount)
    {
        Debug.Assert(newWallCount > WallCount);
    }
    void VerifyDiscard(int[] newDiscard)
    {
        for (int i = 0; i < Discard.Count; i++)
        {
            // the existing Discard list on the client should equal newDiscard up until the newer tile(s)
            Debug.Assert(Discard[i] == newDiscard[i]);
        }
    }
    */
}
