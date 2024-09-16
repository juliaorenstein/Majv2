using Fusion;

public sealed class FusionWrapper : NetworkBehaviour, IFusionWrapper
{
    ClassReferences refs;
    FusionManager FManager { get => (FusionManager)refs.FManager; }
    TurnManagerServer TurnManagerServer { get => refs.TManager; }
    TurnManagerClient TurnManagerClient { get => refs.TManagerClient; }
    TileTrackerServer TileTracker { get => refs.TileTracker; }

    // Info about players
    public int LocalPlayerId { get => Runner.LocalPlayer.PlayerId; }

    public bool IsServer { get => Runner.IsServer; }
    public bool IsPlayerAI(int playerID)
    { return FManager.PlayerDict[playerID] == PlayerRef.None; }


    // Tiles
    [Networked] public int ActiveDiscardTileId { get; set; }

    // Timer stuff
    private TickTimer timer;
    public bool IsTimerExpired { get => timer.Expired(Runner); }
    public bool IsTimerRunning { get => timer.IsRunning; }
    public void CreateTimer() => timer = TickTimer.CreateFromSeconds(Runner, 2f);
    public void ResetTimer() => timer = TickTimer.None;

    public override void Spawned()
    {
        refs = ObjectReferences.Instance.ClassRefs;
        refs.Fusion = this;
    }

    // Frame based update stuff
    public override void FixedUpdateNetwork()
    {
        TurnManagerServer.TileCallingMonitor();
    }

    // RPCs
    public void RPC_S2C_SendGameState(int playerId)
    {
        NetworkableTileLocations tileLocs = TileTracker.NetworkTileLocs(playerId);
        RPC_S2C_SendGameState(FManager.PlayerDict[playerId], tileLocs.WallCount
            , tileLocs.Discard, tileLocs.PrivateRack, tileLocs.PrivateRackCounts
            , tileLocs.DisplayRacks[0], tileLocs.DisplayRacks[1]
            , tileLocs.DisplayRacks[2], tileLocs.DisplayRacks[3]);
    }
    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    void RPC_S2C_SendGameState(PlayerRef player, int wallCount, int[] discard
        , int[] privateRack, int[] privateRackCounts, int[] displayRack0
        , int[] displayRack1, int[] displayRack2, int[] displayRack3)
    {
        UnityEngine.Debug.Log($"RPC receivied for player {player}");
        refs.TileTrackerClient.ReceiveGameState(wallCount, discard, privateRack
            , privateRackCounts, displayRack0, displayRack1, displayRack2
            , displayRack3);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_C2S_Discard(int discardTileId) => TurnManagerServer.Discard(discardTileId);

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_S2A_ShowDiscard(int discardTileId) => TurnManagerClient.ShowDiscard(discardTileId);

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_S2A_ShowButtons(int discardPlayerId) => TurnManagerClient.ShowButtons();

    public void RPC_S2C_NextTurn(int nextPlayerId, int nextTileId) => RPC_S2C_NextTurn(FManager.PlayerDict[nextPlayerId], nextPlayerId);
    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_S2C_NextTurn(PlayerRef _, int nextTileId) => TurnManagerClient.NextTurn(nextTileId);

    public void RPC_S2C_CallTurn(int callPlayerId, int callTileId) => RPC_S2C_CallTurn(FManager.PlayerDict[callPlayerId], callTileId);
    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_S2C_CallTurn(PlayerRef _, int callTileId) => TurnManagerClient.CallTurn(callTileId);

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_S2A_ResetButtons() => TurnManagerClient.ResetButtons();

    [Rpc(RpcSources.All, RpcTargets.All, HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_C2A_Expose(int exposeTileId) => TurnManagerClient.ExposeOtherPlayer(exposeTileId);

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_S2A_NeverMind() => TurnManagerClient.NeverMind();

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, TickAligned = false)]
    public void RPC_S2C_SendRack(int playerId, int[] tileArr) => refs.TileTrackerClient.ReceiveRackUpdate(tileArr); // TODO: game state updates

    // Player Input

    public bool WasWaitPressed(int playerId) => FManager.InputDict[playerId].wait;
    public bool WasPassPressed(int playerId) => FManager.InputDict[playerId].pass;
    public bool WasCallPressed(int playerId) => FManager.InputDict[playerId].call;
    public bool WasNeverMindPressed(int playerId) => FManager.InputDict[playerId].nevermind;
}

