using Fusion;
using System.Collections.Generic;

public sealed class FusionWrapper : NetworkBehaviour, IFusionWrapper
{
    ClassReferences refs;
    FusionManager FManager;
    TurnManager TManager;

    // Info about players
    public int LocalPlayerId { get => Runner.LocalPlayer.PlayerId; }
    [Networked] public int TurnPlayerId { get; set; }
    [Networked] public int CallPlayerId { get; set; }

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
        FManager = (FusionManager)refs.FManager;
        TManager = refs.TManager;
        // TODO: make sure TurnPlayerId is dealt with
        CallPlayerId = -1;
    }

    // Frame based update stuff
    public override void FixedUpdateNetwork()
    {
        TManager.H_TileCallingMonitor();
    }

    // RPCs
    public void RPC_S2C_SendGameState(int playerId, NetworkableTileLocations tileLocs)
    {
        RPC_S2C_SendGameState(FManager.PlayerDict[playerId], tileLocs.WallCount
            , tileLocs.Discard, tileLocs.PrivateRack, tileLocs.PrivateRackCounts
            , tileLocs.DisplayRacks);
    }
    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    void RPC_S2C_SendGameState(PlayerRef player, int wallCount, int[] discard
        , int[] privateRack, int[] privateRackCounts, int[][] displayRacks)
    {
        refs.TileTrackerClient.ReceiveGameState(wallCount, discard, privateRack
            , privateRackCounts, displayRacks);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_C2S_Discard(int discardTileId) => TManager.H_Discard(discardTileId);

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_S2A_ShowDiscard(int discardTileId) => TManager.C_RequestDiscard(discardTileId);

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_S2A_ShowButtons(int discardPlayerId) => TManager.C_ShowButtons();

    public void RPC_S2C_NextTurn(int nextPlayerId, int nextTileId) => RPC_S2C_NextTurn(FManager.PlayerDict[nextPlayerId], nextPlayerId);
    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_S2C_NextTurn(PlayerRef _, int nextTileId) => TManager.C_NextTurn(nextTileId);

    public void RPC_S2C_CallTurn(int callPlayerId, int callTileId) => RPC_S2C_CallTurn(FManager.PlayerDict[callPlayerId], callTileId);
    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_S2C_CallTurn(PlayerRef _, int callTileId) => TManager.C_CallTurn(callTileId);

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_S2A_ResetButtons() => TManager.C_ResetButtons();

    [Rpc(RpcSources.All, RpcTargets.All, HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_C2A_Expose(int exposeTileId) => TManager.C_ExposeOtherPlayer(exposeTileId);

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_S2A_NeverMind() => TManager.C_NeverMind();

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, TickAligned = false)]
    public void RPC_S2C_SendRack(int playerId, int[] tileArr) => refs.ReceiveGame.ReceiveRackUpdate(tileArr); // TODO: game state updates

    // Player Input

    public bool WasWaitPressed(int playerId) => FManager.InputDict[playerId].wait;
    public bool WasPassPressed(int playerId) => FManager.InputDict[playerId].pass;
    public bool WasCallPressed(int playerId) => FManager.InputDict[playerId].call;
    public bool WasNeverMindPressed(int playerId) => FManager.InputDict[playerId].nevermind;
}

