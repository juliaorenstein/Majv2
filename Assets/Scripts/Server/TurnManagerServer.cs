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
    public int LastDiscarded => tileTracker.Discard.Last();
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

    public void Discard(int discardTile)
    {
        UnityEngine.Debug.Log($"TurnManagerServer.Discard({discardTile})");
        if (fusionManager.ExposingPlayer != -1) DiscardFromExpose(discardTile);

        else
        {
            UnityEngine.Debug.Assert(
                tileTracker.TileLocations[discardTile]
                == tileTracker.ActivePrivateRackLoc
                , "discardTile is not on active player's rack.");
        }

        // housekeeping
        fusionManager.TurnPhase = TurnPhase.Discarding;

        // update lists
        tileTracker.MoveTile(discardTile, TileLoc.Discard);
        fusion.RPC_S2A_ShowDiscard(discardTile);

        fusion.CreateTimer();
        // wait for callers
        if (Tile.IsJoker(discardTile))
        {
            UnityEngine.Debug.Log("Discarding joker - no buttons.");
            return;
        }
        fusion.RPC_S2A_ShowButtons(fusionManager.ActivePlayer);
    }

    void DiscardFromExpose(int discardTile)
    {

        UnityEngine.Debug.Assert(
            tileTracker.TileLocations[discardTile]
            == tileTracker.PrivateRackLocations[fusionManager.ExposingPlayer]
            , "discardTile is not on exposing player's rack during expose.");

        // exposing player's turn is done, set ActivePlayer to this ExposingPlayer
        // so that the next player is chosen correctly
        fusionManager.ActivePlayer = fusionManager.ExposingPlayer;
        InitializeNextTurn();
    }

    public void TileCallingMonitor()
    {
        // this check rules out times when players aren't calling
        // but also clients because they never have a timer set
        if (!fusion.IsTimerRunning) { return; }
        UpdateCallingAndWaitingLists();
        ExposingPlayerCheck();
        // if any player says wait, don't do anything
        if (AnyPlayerWaiting) return;

        TimerCheck();


        void UpdateCallingAndWaitingLists()
        {
            for (int playerId = 0; playerId < 4; playerId++)
            {
                if (fusionManager.WaitPressed(playerId)) { playersWaiting.Add(playerId); }
                if (fusionManager.PassPressed(playerId)) { playersWaiting.Remove(playerId); }
                if (fusionManager.CallPressed(playerId))
                {
                    playersWaiting.Remove(playerId);
                    playersCalling.Add(playerId);
                }
            }
        }

        void ExposingPlayerCheck()
        {
            // if there's an exposing player, check for never mind
            if (fusionManager.ExposingPlayer != -1)
            {
                if (fusionManager.NeverMindPressed(fusionManager.ExposingPlayer))
                {
                    NeverMind(); // don't quit out of function - go to next caller
                }
                else return;
            }
        }

        void TimerCheck()
        {
            if (fusion.IsTimerExpired)
            {
                if (AnyPlayerCalling)
                {   // if any players call and timer is done/not running, do logic
                    // sort PlayersCalling by going around from current player
                    playersCalling.Sort((x, y) => PlayersCallingSorter(x, y, fusionManager.ActivePlayer));
                    fusionManager.ExposingPlayer = playersCalling[0];

                    Call();
                    return;
                }

                // otherwise go to next turn
                NextTurn();
            }
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
        callTile = LastDiscarded;
        UnityEngine.Debug.Assert(!Tile.IsJoker(callTile));

        TileLoc rack = tileTracker.DisplayRackLocations[fusionManager.ExposingPlayer];
        tileTracker.MoveTile(callTile, rack);
        // TODO: AI support for calling
        fusion.RPC_S2C_CallTurn(fusionManager.ExposingPlayer, callTile);
    }

    void NeverMind()
    {
        UnityEngine.Debug.Log($"TurnManagerServer.NeverMind()\n   - exposingPlayer: {fusionManager.ExposingPlayer}");
        playersCalling.Remove(fusionManager.ExposingPlayer);
        fusionManager.ExposingPlayer = -1;
        tileTracker.MoveTile(callTile, TileLoc.Discard);
    }

    void IncrementActivePlayer()
    {
        fusionManager.ActivePlayer = (fusionManager.ActivePlayer + 1) % 4;
        UnityEngine.Debug.Log($"TurnManagerServer.IncrementActivePlayer()\n    - new ActivePlayer: {fusionManager.ActivePlayer}");
    }
    void NextTurn()
    {
        UnityEngine.Debug.Log("TurnManagerServer.NextTurn()");

        IncrementActivePlayer();
        InitializeNextTurn();

        int nextTileId = tileTracker.Wall.Last();
        tileTracker.MoveTile(nextTileId, tileTracker.ActivePrivateRackLoc);                // add that tile to the player's rack list
        if (fusionManager.IsPlayerAI(fusionManager.ActivePlayer))                         // AI turn
        {
            AITurn();
            return;
        }
        fusion.RPC_S2C_NextTurn(fusionManager.ActivePlayer, nextTileId);         // if it's a person, hand it over to that client
    }

    void InitializeNextTurn()
    {
        fusion.ResetTimer();
        playersWaiting.Clear(); // this shouldn't be needed
        playersCalling.Clear();
        fusion.RPC_S2A_ResetButtons();
        callTile = -1;
        fusionManager.ExposingPlayer = -1;
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
