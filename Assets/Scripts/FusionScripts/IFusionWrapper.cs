using Fusion;

public interface IFusionWrapper
{
    bool IsTimerExpired { get; }
    bool IsTimerRunning { get; }

    void CreateTimer();
    void FixedUpdateNetwork();
    void ResetTimer();
    void RPC_S2C_SendGameState(int playerId);
    void RPC_C2A_Expose(int exposeTileId);
    void RPC_C2S_Discard(int discardTileId);
    void RPC_S2A_NeverMind();
    void RPC_S2A_ResetButtons();
    void RPC_S2A_ShowButtons(int discardPlayerId);
    void RPC_S2A_ShowDiscard(int discardTileId);
    void RPC_S2C_CallTurn(int callPlayerId, int callTileId);
    void RPC_S2C_NextTurn(int nextPlayerId, int nextTileId);
}