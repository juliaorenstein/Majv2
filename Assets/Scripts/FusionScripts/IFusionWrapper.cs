using Fusion;

public interface IFusionWrapper
{
    int LocalPlayerId { get; }
    int TurnPlayerId { get; set; }
    int CallPlayerId { get; set; }
    bool IsServer { get; }
    int ActiveDiscardTileId { get; set; }
    bool IsTimerExpired { get; }
    bool IsTimerRunning { get; }

    void CreateTimer();
    void FixedUpdateNetwork();
    bool IsPlayerAI(int playerID);
    void ResetTimer();
    void RPC_C2A_Expose(int exposeTileId);
    void RPC_C2H_Discard(int discardTileId);
    void RPC_H2A_NeverMind();
    void RPC_H2A_ResetButtons();
    void RPC_H2A_ShowButtons(int discardPlayerId);
    void RPC_H2A_ShowDiscard(int discardTileId);
    void RPC_H2C_CallTurn(int callPlayerId, int callTileId);
    void RPC_H2C_NextTurn(int nextPlayerId, int nextTileId);
    void RPC_H2C_SendRack(int playerId, int[] tileArr);
    bool WasCallPressed(int playerId);
    bool WasNeverMindPressed(int playerId);
    bool WasPassPressed(int playerId);
    bool WasWaitPressed(int playerId);
}