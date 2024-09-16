using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class TurnManagerServer
{
    // TODO: should I make client and server into separate assemblies?
    readonly ClassReferences refs;
    readonly IFusionWrapper fusion;
    readonly IFusionManager fusionManager;
    readonly TileTrackerServer tileTracker;
    public int DiscardTile;
    int callTile;
    int discardPlayerId;
    int exposePlayerId;

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
        UnityEngine.Debug.Assert(fusionManager.IsServer);

        fusionManager.ActivePlayer = fusionManager.Dealer;
        if (!fusionManager.IsPlayerAI(fusionManager.Dealer)) return;
        AITurn(tileTracker.PrivateRacks[fusionManager.Dealer].Last());
    }

    public void Discard(int discardTileId)
    {
        // housekeeping
        fusionManager.TurnPhase = TurnPhase.Discarding;
        DiscardTile = discardTileId;

        // update lists
        tileTracker.PrivateRacks[discardPlayerId].Remove(discardTileId);
        tileTracker.Discard.Add(discardTileId);
        fusion.RPC_S2A_ShowDiscard(discardTileId);

        // wait for callers
        if (!Tile.IsJoker(discardTileId))
        {
            fusion.CreateTimer();
            fusion.RPC_S2A_ShowButtons(discardPlayerId);
        }
        // FIXME: host can't use mono's startnewcoroutine
        //else { mono.StartNewCoroutine(WaitForJoker()); }
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

        if (AnyPlayerWaiting) { return; }                           // if any player says wait, don't do anything
        else if (AnyPlayerCalling && fusion.IsTimerExpired)         // if any players call and timer is done/not running, do logic
        {
            // sort PlayersCalling by going around from current player
            playersCalling.Sort((x, y) => PlayersCallingSorter(x, y, discardPlayerId));

            //int closestPlayerDelta = PlayersCalling.Select(playerId => playerId - fusion.TurnPlayerId).Min();
            //fusion.TurnPlayerId = (fusion.TurnPlayerId + closestPlayerDelta + 4) % 4;

            Call();
        }

        else if (fusion.IsTimerExpired) { Pass(); }                 // if nobody waited/called after 2s, pass
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

    void Call() { H_CallTurn(); } // silly

    void Pass()
    {
        fusionManager.ActivePlayer = (fusionManager.ActivePlayer + 1) % 4;
        H_NextTurn();
    }

    /*
        IEnumerator WaitForJoker()
        {
            yield return mono.WaitForSeconds(2);
            fusionManager.ActivePlayer = (fusionManager.ActivePlayer + 1) % 4;
            H_NextTurn();
        }
        */

    void H_NextTurn()
    {
        int nextPlayer = H_InitializeNextTurn();
        int nextTileId = tileTracker.Wall.Last();
        fusionManager.ExposingPlayer = -1;

        tileTracker.PrivateRacks[fusionManager.ActivePlayer].Add(nextTileId);                 // add that tile to the player's rack list
        if (fusionManager.IsPlayerAI(nextPlayer))                         // AI turn
        {
            AITurn(nextTileId);
            return;
        }
        fusion.RPC_S2C_NextTurn(nextPlayer, nextTileId);         // if it's a person, hand it over to that client
    }

    void H_CallTurn()
    {
        callTile = DiscardTile;
        exposePlayerId = H_InitializeNextTurn();

        tileTracker.PrivateRacks[fusionManager.ActivePlayer].Add(callTile); // TODO: track public tiles separately
        // TODO: AI support for calling
        fusion.RPC_S2C_CallTurn(exposePlayerId, callTile);
    }

    int H_InitializeNextTurn()
    {
        fusion.ResetTimer();
        playersWaiting.Clear(); // this shouldn't be needed
        playersCalling.Clear();
        fusion.RPC_S2A_ResetButtons();
        return fusionManager.ActivePlayer;   // set next player
    }



    void AITurn(int newTileId)
    {
        int discardTileId = newTileId; // for now just discard what was picked up
        Discard(discardTileId);
    }

    // FIXME: when client calls host doesn't see the first exposed tile
    // FIXME: if client exposes the same tile multiple times, more tiles get destroyed on the other clients
}
