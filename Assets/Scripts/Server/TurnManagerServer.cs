using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class TurnManagerServer
{
    readonly ClassReferences refs;
    readonly IMonoWrapper mono;
    readonly IFusionWrapper fusion;
    readonly GameManager gameManager;
    readonly TileTracker tileTracker;

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
        refs.TManager = this;
        mono = refs.Mono;
        fusion = refs.Fusion;
        gameManager = refs.GManager;
        tileTracker = refs.TileTracker;
        playersWaiting = new();
        playersCalling = new();
    }

    // Setup first turn
    public void C_StartGamePlay()
    {
        mono.SetActive(MonoObject.Discard, true);

        if (gameManager.DealerId == gameManager.LocalPlayerId)
        {
            mono.SetRaycastTarget(MonoObject.Discard, true);
        }
        else if (fusion.IsPlayerAI(gameManager.DealerId) && fusion.IsServer)
        {
            H_AITurn(tileTracker.PrivateRacks[gameManager.DealerId].Last());
        }
    }

    // Client discards a tile
    public void C_RequestDiscard(int discardTileId)
    {
        fusion.RPC_C2S_Discard(discardTileId);
        mono.SetRaycastTarget(MonoObject.Discard, false);
    }

    // Server does next turn logic
    public void H_Discard(int discardTileId)
    {
        // update lists
        discardPlayerId = fusion.TurnPlayerId;
        DiscardTile = discardTileId;
        tileTracker.PrivateRacks[discardPlayerId].Remove(discardTileId);
        fusion.RPC_S2A_ShowDiscard(discardTileId);

        // wait for callers
        if (!Tile.IsJoker(discardTileId))
        {
            fusion.CreateTimer();
            fusion.RPC_S2A_ShowButtons(discardPlayerId);
        }
        else { mono.StartNewCoroutine(WaitForJoker()); }
    }

    // All clients show discarded tile
    void C_ShowDiscard(int discardTileId)
    {
        mono.MoveTile(discardTileId, MonoObject.Discard);
        mono.SetRaycastTargetOnTile(discardTileId, false);
    }

    public void C_ShowButtons()
    {
        if (gameManager.LocalPlayerId != discardPlayerId)
        { mono.SetActive(MonoObject.CallWaitButtons, true); }

        // TODO: verify that this is working on clients now
    }

    public void H_TileCallingMonitor()
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
            playersCalling.Sort((x, y) => PlayersCallingSorter(x, y, fusion.TurnPlayerId));

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
        fusion.TurnPlayerId = (fusion.TurnPlayerId + 1) % 4;
        H_NextTurn();
    }

    IEnumerator WaitForJoker()
    {
        yield return mono.WaitForSeconds(2);
        fusion.TurnPlayerId = (fusion.TurnPlayerId + 1) % 4;
        H_NextTurn();
    }

    void H_NextTurn()
    {
        int nextPlayer = H_InitializeNextTurn();
        int nextTileId = tileTracker.Wall.Last();
        mono.SetRaycastTargetOnTile(nextTileId, true);

        tileTracker.PrivateRacks[fusion.TurnPlayerId].Add(nextTileId);                 // add that tile to the player's rack list
        if (fusion.IsPlayerAI(nextPlayer))                         // AI turn
        {
            H_AITurn(nextTileId);
            return;
        }
        fusion.RPC_S2C_NextTurn(nextPlayer, nextTileId);         // if it's a person, hand it over to that client
    }

    void H_CallTurn()
    {
        callTile = DiscardTile;
        exposePlayerId = H_InitializeNextTurn();

        tileTracker.PrivateRacks[fusion.TurnPlayerId].Add(callTile); // TODO: track public tiles separately
        // TODO: AI support for calling
        fusion.RPC_S2C_CallTurn(exposePlayerId, callTile);
    }

    int H_InitializeNextTurn()
    {
        fusion.ResetTimer();
        playersWaiting.Clear(); // this shouldn't be needed
        playersCalling.Clear();
        fusion.RPC_S2A_ResetButtons();
        return fusion.TurnPlayerId;   // set next player
    }

    public void C_ResetButtons()
    {
        mono.SetTurnIndicatorText(fusion.TurnPlayerId);
        mono.SetActive(MonoObject.WaitButton, true);
        mono.SetActive(MonoObject.PassButton, false);
        mono.SetActive(MonoObject.CallWaitButtons, false);
    }

    // Client starts turn
    public void C_NextTurn(int nextTileId)
    {
        exposePlayerId = -1;
        mono.MoveTile(nextTileId, MonoObject.PrivateRack);
        mono.SetRaycastTarget(MonoObject.Discard, true);
    }

    public void C_CallTurn(int CallTile)
    {
        C_Expose(CallTile);

        // FIXME: if a player puts an exposed tile back on their rack, remove it from screen
    }

    public void C_Expose(int exposeTileId)
    {
        mono.MoveTile(exposeTileId, MonoObject.PublicRack);
        fusion.RPC_C2A_Expose(exposeTileId);
        if (ReadyToContinue()) { mono.SetRaycastTarget(MonoObject.Discard, true); }
    }

    public void C_ExposeOtherPlayer(int exposeTileId)
    {
        int rackId = (exposePlayerId - gameManager.LocalPlayerId + 4) % 4 - 1;
        mono.ExposeOtherPlayerTile(rackId, exposeTileId);

        // FIXME: not working
    }

    public void C_NeverMind()
    {

    }

    bool ReadyToContinue() { return true; } // TODO: later make sure it's >2 and valid group

    void H_AITurn(int newTileId)
    {
        int discardTileId = newTileId; // for now just discard what was picked up
        H_Discard(discardTileId);
    }

    // FIXME: when client calls host doesn't see the first exposed tile
    // FIXME: if client exposes the same tile multiple times, more tiles get destroyed on the other clients
}
