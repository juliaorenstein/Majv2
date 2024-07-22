using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class TurnManager
{
    public ClassReferences Refs;
    public IMonoWrapper mono;
    public IFusionWrapper fusion;

    public FusionManager FManager;
    private readonly GameManager GManager;
    public string ExposeTileName;
    private int DiscardTile;
    private int CallTile;
    private int DiscardPlayerId;
    private int ExposePlayerId;

    private List<int> PlayersWaiting;
    private bool AnyPlayerWaiting
    { get { return PlayersWaiting.Count > 0; } }

    private List<int> PlayersCalling;
    private bool AnyPlayerCalling
    { get { return PlayersCalling.Count > 0; } }

    // instantiated in SetupHost
    public TurnManager(ClassReferences refs)
    {
        Refs = refs;
        refs.TManager = this;
        mono = refs.Mono;
        fusion = refs.Fusion;
        PlayersWaiting = new();
        PlayersCalling = new();
    }

    // Setup first turn
    public void C_StartGamePlay()
    {
        mono.SetActive(MonoObject.Discard, true);
        
        if (GManager.DealerId == GManager.LocalPlayerId)
        {
            mono.SetRaycastTarget(MonoObject.Discard, true);
        }
        else if (fusion.IsPlayerAI(GManager.DealerId) && fusion.IsServer)
        {
            H_AITurn(GManager.Racks[GManager.DealerId].Last());
        }
    }

    // Client discards a tile
    public void C_Discard(int discardTileId)
    {
        ExposeTileName = null;
        fusion.RPC_C2H_Discard(discardTileId);
        mono.SetRaycastTarget(MonoObject.Discard, false);
    }

    // Server does next turn logic
    public void H_Discard(int discardTileId)
    {
        // update lists
        DiscardPlayerId = fusion.TurnPlayerId;
        DiscardTile = discardTileId;
        GManager.Racks[DiscardPlayerId].Remove(discardTileId);
        fusion.RPC_H2A_ShowDiscard(discardTileId);

        // wait for callers
        if (!Tile.IsJoker(discardTileId))
        {
            fusion.CreateTimer();
            fusion.RPC_H2A_ShowButtons(DiscardPlayerId);
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
        if (GManager.LocalPlayerId != DiscardPlayerId)
        { mono.SetActive(MonoObject.CallWaitButtons, true); }

        // TODO: verify that this is working on clients now
    }

    public void H_TileCallingMonitor()
    {
        // this check rules out times when players aren't calling
        // but also clients because they never have a timer set
        if (!fusion.IsTimerRunning) { return; }

        foreach ((int playerId, InputCollection playerInput) in FManager.InputDict)
        {
            if (playerInput.wait) { PlayersWaiting.Add(playerId); }
            if (playerInput.pass) { PlayersWaiting.Remove(playerId); }
            if (playerInput.call)
            {
                PlayersWaiting.Remove(playerId);
                PlayersCalling.Add(playerId);
            }
        }
        
        if (AnyPlayerWaiting) { return; }                           // if any player says wait, don't do anything
        else if (AnyPlayerCalling && fusion.IsTimerExpired)         // if any players call and timer is done/not running, do logic
        {
            // sort PlayersCalling by going around from current player
            PlayersCalling.Sort((x, y) => PlayersCallingSorter(x, y, fusion.TurnPlayerId));

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
        int nextTileId = GManager.Wall.Pop();
        mono.SetRaycastTargetOnTile(nextTileId, true);
        
        GManager.Racks[fusion.TurnPlayerId].Add(nextTileId);                 // add that tile to the player's rack list
        if (fusion.IsPlayerAI(nextPlayer))                         // AI turn
        {
            H_AITurn(nextTileId);
            return;
        }
        fusion.RPC_H2C_NextTurn(nextPlayer, nextTileId);         // if it's a person, hand it over to that client
    }

    void H_CallTurn()
    {
        CallTile = DiscardTile;
        ExposePlayerId = H_InitializeNextTurn();

        GManager.Racks[fusion.TurnPlayerId].Add(CallTile); // TODO: track public tiles separately
        // TODO: AI support for calling
        fusion.RPC_H2C_CallTurn(ExposePlayerId, CallTile);
    }

    int H_InitializeNextTurn()
    {
        fusion.ResetTimer();
        PlayersWaiting.Clear(); // this shouldn't be needed
        PlayersCalling.Clear();
        fusion.RPC_H2A_ResetButtons();
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
        ExposePlayerId = -1;
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
        int rackId = (ExposePlayerId - GManager.LocalPlayerId + 4) % 4 - 1;
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
