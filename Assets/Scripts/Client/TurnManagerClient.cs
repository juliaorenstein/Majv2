public class TurnManagerClient
{
    readonly IMonoWrapper mono;
    readonly IFusionWrapper fusion;
    readonly IFusionManager fusionManager;

    public TurnManagerClient(ClassReferences refs)
    {
        refs.TManagerClient = this;
        mono = refs.Mono;
        fusion = refs.Fusion;
        fusionManager = refs.FManager;
    }

    // Setup first turn
    public void StartGamePlay()
    {
        UnityEngine.Debug.Assert(!fusionManager.IsServer);
        mono.SetActive(MonoObject.Discard, true);

        if (fusionManager.IsDealer)
        {
            mono.SetRaycastTarget(MonoObject.Discard, true);
        }
    }

    // Client discards a tile
    public void RequestDiscard(int discardTileId)
    {
        UnityEngine.Debug.Assert(fusionManager.IsActivePlayer);

        fusion.RPC_C2S_Discard(discardTileId);
        mono.SetRaycastTarget(MonoObject.Discard, false);
    }

    // All clients show discarded tile
    public void ShowDiscard(int discardTileId)
    {
        mono.MoveTile(discardTileId, MonoObject.Discard);
        mono.SetRaycastTargetOnTile(discardTileId, false);
    }

    // All clients except discarder see the call wait pass buttons
    public void ShowButtons()
    {
        if (!fusionManager.IsActivePlayer)
        { mono.SetActive(MonoObject.CallWaitButtons, true); }
    }

    public void ResetButtons()
    {
        mono.SetTurnIndicatorText(fusionManager.ActivePlayer);
        mono.SetActive(MonoObject.WaitButton, true);
        mono.SetActive(MonoObject.PassButton, false);
        mono.SetActive(MonoObject.CallWaitButtons, false);
    }

    // Client starts turn
    public void NextTurn(int nextTileId)
    {
        mono.SetRaycastTargetOnTile(nextTileId, true);
        mono.MoveTile(nextTileId, MonoObject.PrivateRack);
        mono.SetRaycastTarget(MonoObject.Discard, true);
    }

    public void RequestExpose(int exposeTileId)
    {
        mono.MoveTile(exposeTileId, MonoObject.PublicRack);
        fusion.RPC_C2A_Expose(exposeTileId);
        if (ReadyToContinue()) { mono.SetRaycastTarget(MonoObject.Discard, true); }
    }

    public void ExposeOtherPlayer(int exposeTileId)
    {
        int rackId = (fusionManager.ExposingPlayer - fusionManager.LocalPlayer + 4) % 4 - 1;
        mono.ExposeOtherPlayerTile(rackId, exposeTileId);

        // FIXME: not working
    }

    public void NeverMind()
    {

    }

    bool ReadyToContinue() { return true; } // TODO: later make sure it's >2 and valid group
}
