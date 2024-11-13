using System.Collections.Generic;
using System.Linq;
using System.Text;


public class TileTrackerServer
{
    readonly ClassReferences refs;

    // TileLocations
    List<int> wall = new();
    public IReadOnlyList<int> Wall => wall.AsReadOnly();

    List<int> discard = new();
    public IReadOnlyList<int> Discard => discard.AsReadOnly();

    List<List<int>> privateRacks = new() { new(), new(), new(), new() };
    public List<IReadOnlyList<int>> PrivateRacks = new();
    public IReadOnlyList<int> ActivePrivateRack => PrivateRacks[refs.FManager.ActivePlayer];
    public TileLoc ActivePrivateRackLoc => PrivateRackLocations[refs.FManager.ActivePlayer];

    List<List<int>> displayRacks = new() { new(), new(), new(), new() };
    public List<IReadOnlyList<int>> DisplayRacks = new();

    Dictionary<int, LocChange> tileLocations = new();
    public IReadOnlyDictionary<int, LocChange> TileLocations => tileLocations;

    Dictionary<TileLoc, List<int>> TileLocToListMap;
    public List<TileLoc> PrivateRackLocations = new()
    {
        TileLoc.PrivateRack0,
        TileLoc.PrivateRack1,
        TileLoc.PrivateRack2,
        TileLoc.PrivateRack3
    };
    public List<TileLoc> DisplayRackLocations = new() {
        TileLoc.DisplayRack0,
        TileLoc.DisplayRack1,
        TileLoc.DisplayRack2,
        TileLoc.DisplayRack3
    };

    // initialized in SetupHost
    public TileTrackerServer(ClassReferences refs)
    {
        refs.TileTracker = this;
        this.refs = refs;
        foreach (List<int> rack in privateRacks)
        {
            PrivateRacks.Add(rack.AsReadOnly());
        }
        foreach (List<int> rack in displayRacks)
        {
            DisplayRacks.Add(rack.AsReadOnly());
        }
        TileLocToListMap = new() {
            { TileLoc.Wall, wall},
            { TileLoc.Discard, discard},
            { TileLoc.PrivateRack0, privateRacks[0] },
            { TileLoc.PrivateRack1, privateRacks[1] },
            { TileLoc.PrivateRack2, privateRacks[2] },
            { TileLoc.PrivateRack3, privateRacks[3] },
            { TileLoc.DisplayRack0, displayRacks[0] },
            { TileLoc.DisplayRack1, displayRacks[1] },
            { TileLoc.DisplayRack2, displayRacks[2] },
            { TileLoc.DisplayRack3, displayRacks[3] }
        };
    }

    // Tile Locations for Client in networkable format (int arrays)
    public NetworkableTileLocations NetworkTileLocs(int playerId)
    {
        return new()
        {
            WallCount = Wall.Count,
            Discard = Discard.ToArray(),
            PrivateRack = PrivateRacks[playerId].ToArray(),
            PrivateRackCounts = PrivateRacks.Select(rack => rack.Count).ToArray(),
            DisplayRacks = DisplayRacks.Select(list => list.ToArray()).ToArray()
        };
    }

    // Simple move just removes tile from previous location and adds it onto the end of the new location.
    // Might not be suitable for all tile moves (like dropping to specific location on rack).
    public void MoveTile(int tileId, TileLoc location)
    {
        UnityEngine.Debug.Log($"TileTrackerServer.MoveTile({tileId}, {location})");

        // set last loc. If tile isn't already being tracked, set last loc to the wall
        TileLoc newLastLoc = tileLocations.ContainsKey(tileId) ? tileLocations[tileId].curLoc : TileLoc.Wall;
        TileLoc newCurLoc = location;

        tileLocations[tileId] = new LocChange() // update dictionary entry
        {
            lastLoc = newLastLoc,
            curLoc = newCurLoc
        };

        List<int> oldLocationList = TileLocToListMap[newLastLoc];
        List<int> newLocationList = TileLocToListMap[newCurLoc];

        oldLocationList.Remove(tileId);   // remove tile from the list it's currently on
        newLocationList.Add(tileId);                   // add tile to its new location

        SendGameStateToAll();
    }

    public void SendGameStateToAll()
    {
        for (int playerId = 0; playerId < 4; playerId++)
        {
            if (refs.FManager.IsPlayerAI(playerId)) continue;
            SendGameStateToPlayer(playerId);
        }
    }

    void SendGameStateToPlayer(int playerId)
    {
        List<TileLoc> ClientTileLocs = new() {
            TileLoc.Discard,
            PrivateRackLocations[playerId],
            TileLoc.DisplayRack0,
            TileLoc.DisplayRack1,
            TileLoc.DisplayRack2,
            TileLoc.DisplayRack3
        };

        Dictionary<int, LocChange> tileLocsForClient = new();
        foreach (KeyValuePair<int, LocChange> item in tileLocations)
        {
            TileLoc curLoc = item.Value.curLoc;
            if (ClientTileLocs.Contains(curLoc)) tileLocsForClient.Add(item.Key, item.Value);
            refs.Fusion.RPC_S2C_SendGameState(playerId);
        }
    }

    // TODO: unit tests for the methods below.
    public string PrivateRacksToString()
    {
        StringBuilder res = new();
        for (int rackId = 0; rackId < 4; rackId++)
        {
            res.Append($"Private Rack {rackId}: ");
            res.AppendJoin(", ", PrivateRacks[rackId].Select(tileId => Tile.ToString(tileId)));
            res.Append("\n");
        }
        return res.ToString();
    }

    string DisplayRacksToString()
    {
        StringBuilder res = new();
        for (int rackId = 0; rackId < 4; rackId++)
        {
            res.Append($"Display Rack {rackId}");
            res.AppendJoin(", ", DisplayRacks[rackId].Select(tileId => Tile.ToString(tileId)));
            res.Append("\n");
        }
        return res.ToString();
    }

    string WallToString()
    {
        StringBuilder res = new();
        res.Append("Wall: ");
        res.AppendJoin(", ", Wall.Select(tileId => Tile.ToString(tileId)));
        return res.ToString();
    }

    string DiscardToString()
    {
        StringBuilder res = new();
        res.Append("Discard: ");
        res.AppendJoin(", ", Discard.Select(tileId => Tile.ToString(tileId)));
        return res.ToString();
    }

    public override string ToString()
    {
        StringBuilder res = new();
        string[] pieces = new string[] {
            PrivateRacksToString()
            , DisplayRacksToString()
            , WallToString()
            , DiscardToString() };
        res.AppendJoin("\n\n", pieces);
        return res.ToString();
    }
}

public struct NetworkableTileLocations
{
    public int WallCount;
    public int[] Discard;
    public int[] PrivateRack;
    public int[] PrivateRackCounts;
    public int[][] DisplayRacks;
}

public enum TileLoc
{
    Wall,
    Discard,
    PrivateRack0,
    PrivateRack1,
    PrivateRack2,
    PrivateRack3,
    DisplayRack0,
    DisplayRack1,
    DisplayRack2,
    DisplayRack3,
}


public struct LocChange
{
    public TileLoc lastLoc;
    public TileLoc curLoc;
}