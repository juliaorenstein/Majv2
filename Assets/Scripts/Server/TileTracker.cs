using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Text;
using System;

public class TileTracker
{
    readonly ClassReferences refs;

    // initialized in SetupHost
    public TileTracker(ClassReferences refs)
    {
        refs.TileTracker = this;
        this.refs = refs;
    }

    // TileLocations
    public List<int> Wall;
    public List<int> Discard = new();
    public List<List<int>> PrivateRacks = new() { new(), new(), new(), new() };
    public List<List<int>> DisplayRacks = new() { new(), new(), new(), new() };

    public Dictionary<int, List<int>> TileLocations = new();

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
    public void SimpleMoveTile(int tileId, List<int> location)
    {
        Debug.Assert(LocationIsValid(location));
        TileLocations[tileId].Remove(tileId);   // remove tile from the list it's currently on
        location.Add(tileId);                   // add tile to its new location
        TileLocations[tileId] = location;       // update dictionary entry
        SendGameStateToAll();
    }

    // just checks that the List<int> passed in is a location in the TileTracker.
    // does not verify that the move itself is valid.
    bool LocationIsValid(List<int> location)
    {
        if (location == Wall) return true;
        if (location == Discard) return true;
        if (PrivateRacks.Contains(location)) return true;
        if (DisplayRacks.Contains(location)) return true;
        return false;
    }

    public void SendGameStateToAll()
    {
        for (int playerId = 0; playerId < 4; playerId++)
        {
            if (refs.Fusion.IsPlayerAI(playerId)) continue;
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


