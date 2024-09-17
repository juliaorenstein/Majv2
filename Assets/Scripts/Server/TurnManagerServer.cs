using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Mono.Cecil.Cil;

public class TurnManagerServer
{
    // TODO: should I make client and server into separate assemblies?
    readonly ClassReferences refs;
    readonly IFusionWrapper fusion;
    readonly IFusionManager fusionManager;
    readonly TileTrackerServer tileTracker;
    public int DiscardTile;
    int callTile;
    readonly List<int> playersWaiting;
    bool AnyPlayerWaiting
    { get { return playersWaiting.Count > 0; } }

    readonly List<int> playersCalling;
    bool AnyPlayerCalling
    { get { return playersCalling.Count > 0; } }

    // instantiated in SetupHost
    public TurnManagerServer(ClassReferences refs)
    {
        this.refs = refs;
        refs.TManagerServer = this;
        fusion = refs.Fusion;
        fusionManager = refs.FManager;
        tileTracker = refs.TileTracker;
        playersWaiting = new();
        playersCalling = new();
    }

    public void StartGamePlay()
    {
        UnityEngine.Debug.Log("TurnManagerServer.StartGamePlay()");
        UnityEngine.Debug.Assert(fusionManager.IsServer, "Reached TurnManagerServer on client.");

        fusionManager.ActivePlayer = fusionManager.Dealer;
        if (fusionManager.IsPlayerAI(fusionManager.Dealer))
        {
            AITurn();
        }
    }

    public void Discard(int discardTileId)
    {
        UnityEngine.Debug.Log($"TurnManagerServer.Discard({discardTileId})");
        UnityEngine.Debug.Assert(
            tileTracker.ActivePrivateRack.Contains(discardTileId)
            , "Discard tile not on active player's rack.");

        // housekeeping
        fusionManager.TurnPhase = TurnPhase.Discarding;
        DiscardTile = discardTileId;

        // update lists
        tileTracker.PrivateRacks[fusionManager.ActivePlayer].Remove(discardTileId);
        tileTracker.Discard.Add(discardTileId);
        fusion.RPC_S2A_ShowDiscard(discardTileId);

        fusion.CreateTimer();
        // wait for callers
        if (Tile.IsJoker(discardTileId))
        {
            UnityEngine.Debug.Log("Discarding joker - no buttons.");
            return;
        }
        fusion.RPC_S2A_ShowButtons(fusionManager.ActivePlayer);
    }

    public void TileCallingMonitor()
    {
        // this check rules out times when players aren't calling
        // but also clients because they never have a timer set
        if (!fusion.IsTimerRunning) { return; }

        foreach ((int playerId, InputCollection playerInput) in refs.FManager.InputDict)
        {
            if (playerInput.wait) { playersWaiting.Add(playerId); }
            if (playerInput.pass) { playersWaiting.Remove(playerId); }
            if (playerInput.call)
            {
                playersWaiting.Remove(playerId);
                playersCalling.Add(playerId);
            }
        }

        if (AnyPlayerWaiting) return;                           // if any player says wait, don't do anything
        if (fusion.IsTimerExpired)
        {
            if (AnyPlayerCalling)
            {   // if any players call and timer is done/not running, do logic
                // sort PlayersCalling by going around from current player
                playersCalling.Sort((x, y) => PlayersCallingSorter(x, y, fusionManager.ActivePlayer));

                Call();
                return;
            }

            // otherwise go to next turn
            NextTurn();
        }
    }

    // helper function for sorting PlayersCalling
    public static int PlayersCallingSorter(int x, int y, int startPoint)
    {
        // Calculate adjusted values based on the starting point
        int adjustedX = (x - startPoint + 4) % 4;
        int adjustedY = (y - startPoint + 4) % 4;

        // Compare the adjusted values
        return adjustedX.CompareTo(adjustedY);
    }

    // TODO: if called, other clients should see the tile go to otherracks

    // TODO: if a joker is discarded it can't be called

    // FIXME: wait call buttons don't go away on clients for next turn

    void Call()
    {
        callTile = DiscardTile;
        H_InitializeNextTurn();

        tileTracker.PrivateRacks[fusionManager.ActivePlayer].Add(callTile); // TODO: track public tiles separately
        // TODO: AI support for calling
        fusion.RPC_S2C_CallTurn(fusionManager.ExposingPlayer, callTile);
    } // silly

    void IncrementActivePlayer()
    {
        fusionManager.ActivePlayer = fusionManager.ActivePlayer = (fusionManager.ActivePlayer + 1) % 4;
    }
    void NextTurn()
    {
        UnityEngine.Debug.Log("TurnManagerServer.NextTurn()");

        IncrementActivePlayer();
        H_InitializeNextTurn();
        int nextTileId = tileTracker.Wall.Last();
        fusionManager.ExposingPlayer = -1;

        tileTracker.PrivateRacks[fusionManager.ActivePlayer].Add(nextTileId);                 // add that tile to the player's rack list
        if (fusionManager.IsPlayerAI(fusionManager.ActivePlayer))                         // AI turn
        {
            AITurn();
            return;
        }
        fusion.RPC_S2C_NextTurn(fusionManager.ActivePlayer, nextTileId);         // if it's a person, hand it over to that client
    }

    void H_InitializeNextTurn()
    {
        fusion.ResetTimer();
        playersWaiting.Clear(); // this shouldn't be needed
        playersCalling.Clear();
        fusion.RPC_S2A_ResetButtons();
    }



    void AITurn()
    {
        // TODO: add more sophisticated ai turn functionality
        int discardTileId = tileTracker.ActivePrivateRack.Last();
        Discard(discardTileId);
    }

    // FIXME: when client calls host doesn't see the first exposed tile
    // FIXME: if client exposes the same tile multiple times, more tiles get destroyed on the other clients
}
