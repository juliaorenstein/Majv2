using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

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
    public List<List<int>> PrivateRacks = new();
    public List<List<int>> DisplayRacks = new();

    public Dictionary<int, List<int>> TileLocations = new();

    // Tile Locations for Client in networkable format (int arrays)
    NetworkableTileLocations NetworkTileLocs
    {
        get => new()
        {
            WallCount = Wall.Count,
            Discard = Discard.ToArray(),
            PrivateRackCounts = PrivateRacks.Select(rack => rack.Count).ToArray(),
            DisplayRacks = DisplayRacks.Select(list => list.ToArray()).ToArray()
        };
    }

    // Simple move just removes tile from previous location and adds it onto the end of the new location.
    // Might not be suitable for all tile moves (like dropping to specific location on rack.
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
        // TODO: figure out why a switch statement won't work here (lists aren't constant values?)
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
            refs.Fusion.RPC_S2C_SendGameState(playerId, NetworkTileLocs);
        }
    } 
}

public struct NetworkableTileLocations // TODO: figure out if you can rpc structs of primitives
{
    public int WallCount;
    public int[] Discard;
    public int[] PrivateRack;
    public int[] PrivateRackCounts;
    public int[][] DisplayRacks;
}
