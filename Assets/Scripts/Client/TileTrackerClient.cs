using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;

public class TileTrackerClient
{
    private readonly ClassReferences refs;
    private readonly IMonoWrapper mono;

    // lists owned and shared by the host, along with their public client counterparts
    List<int> discard = new();
    public IReadOnlyList<int> Discard => discard.AsReadOnly();

    List<int> displayRack0 = new();
    public IReadOnlyList<int> DisplayRack0 => displayRack0.AsReadOnly();

    List<int> displayRack1 = new();
    public IReadOnlyList<int> DisplayRack1 => displayRack1.AsReadOnly();

    List<int> displayRack2 = new();
    public IReadOnlyList<int> DisplayRack2 => displayRack2.AsReadOnly();

    List<int> displayRack3 = new();
    public IReadOnlyList<int> DisplayRack3 => displayRack3.AsReadOnly();

    List<List<int>> displayRacks;
    public List<IReadOnlyList<int>> DisplayRacks;


    // clients can rearrange their own and even add/remove during Charleston, so this will just be a public ObsColl without a readonly counterpart
    public List<int> LocalPrivateRack = new();
    TileLoc localPrivateRackLoc;

    // counts shared by host (for lists of hidden tiles)
    public int[] PrivateRackCounts { get; private set; } = new int[4];
    public int WallCount { get; private set; }

    // dictionary of tile locations that the client knows about
    public Dictionary<int, TileLoc> tileLocations = new();

    // TileLocs to their respective lists
    Dictionary<TileLoc, List<int>> tileLocToListMap;

    // initiated in SetupClient
    public TileTrackerClient(ClassReferences refs)
    {
        refs.TileTrackerClient = this;
        this.refs = refs;
        mono = refs.Mono;

        displayRacks = new() { displayRack0, displayRack1, displayRack2, displayRack3 };
        DisplayRacks = new() { DisplayRack0, DisplayRack1, DisplayRack2, DisplayRack3 };

        localPrivateRackLoc = refs.FManager.LocalPlayer switch
        {
            0 => TileLoc.PrivateRack0,
            1 => TileLoc.PrivateRack1,
            2 => TileLoc.PrivateRack2,
            3 => TileLoc.PrivateRack3,
            _ => throw new Exception("invalid player id")
        };

        tileLocToListMap = new() {
            {TileLoc.Discard, discard},
            {TileLoc.DisplayRack0, displayRacks[0]},
            {TileLoc.DisplayRack1, displayRacks[1]},
            {TileLoc.DisplayRack2, displayRacks[2]},
            {TileLoc.DisplayRack3, displayRacks[3]},
            {localPrivateRackLoc, LocalPrivateRack}
        };
    }


    public void ReceiveGameState(ClientGameState gameState)
    {
        foreach (KeyValuePair<int, LocChange> item in gameState.TileDict)
        {
            int tileId = item.Key;
            TileLoc lastLoc = item.Value.lastLoc;
            TileLoc curLoc = item.Value.curLoc;

            List<int> newList = tileLocToListMap[curLoc];
            newList.Add(tileId);

            tileLocations[tileId] = curLoc;
        }

        WallCount = gameState.WallCount;
        PrivateRackCounts = gameState.PrivateRackCounts;
    }

    public void ReceiveGameState(int newWallCount, int[] newDiscard, int[] newPrivateRack
        , int[] newPrivateRackCounts, int[] newDisplayRack0, int[] newDisplayRack1
        , int[] newDisplayRack2, int[] newDisplayRack3)
    {
        int[][] newDisplayRacks = new int[][] { newDisplayRack0, newDisplayRack1, newDisplayRack2, newDisplayRack3 };



        /*

        WallCount = newWallCount;       // update wall count
        ApplyAdd(newDiscard, discard);  // update discard pile

        // update all display racks
        for (int i = 0; i < 4; i++)
        {
            ApplyAdd(newDisplayRacks[i], displayRacks[i]);
        }

        // update local private rack
        if (Changed(newPrivateRack, LocalPrivateRack))
        {
            ReceiveRackUpdate(newPrivateRack);
        }

        // update counts of other player's private racks
        PrivateRackCounts = newPrivateRackCounts;
        bool Changed(int[] newList, ObservableCollection<int> curList)
        {
            return !curList.SequenceEqual(new ObservableCollection<int>(newList));
        }

        // clear client pass array
        refs.CClient.ClearClientPassArr();

        // helper function for steps above
        void ApplyAdd(int[] newList, ObservableCollection<int> curList)
        {
            // check that the already-existing items didn't change
            for (int i = 0; i < curList.Count; i++)
            {
                UnityEngine.Debug.Assert(newList[i] == curList[i]);
            }

            for (int i = curList.Count; i < newList.Count(); i++)
            {
                curList.Add(newList[i]);
            }
        }
        */
    }

    public void ReceiveRackUpdate(int[] newRack)
    {
        // we don't want to just reassign the whole list because that makes observable collections harder to use
        // analyze differences between the incoming list and the existing list and apply them.
        // note: we don't care about the order of newRack  or therefore any "move" actions because it's none of the 
        // server's business what order the client keeps their tiles in

        List<int> curRack = LocalPrivateRack;

        // removals
        List<int> removeList = new();
        foreach (int tileId in curRack)
        {
            if (!newRack.Contains(tileId)) removeList.Add(tileId);
        }
        foreach (int tileId in removeList)
        {
            curRack.Remove(tileId);
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
        UnityEngine.Debug.Assert(sender == Discard);
        UnityEngine.Debug.Assert(e.Action == NotifyCollectionChangedAction.Add);

        int tileId = (int)e.NewItems[0];
        mono.MoveTile(tileId, MonoObject.Discard);
    }

    void LocalPrivateRackChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        mono.UpdateRack(new List<int>(LocalPrivateRack));
    }

    void DisplayRacksChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        // the only events that should happen to display racks are Add or Replace
        UnityEngine.Debug.Assert(DisplayRacks.Contains(sender));
        UnityEngine.Debug.Assert(e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Replace);

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

    public struct ClientGameState
    {
        public Dictionary<int, LocChange> TileDict;
        public int WallCount;
        public int[] PrivateRackCounts;
    }
}

