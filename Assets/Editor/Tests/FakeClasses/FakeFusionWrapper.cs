class FakeFusionWrapper : IFusionWrapper
{
    public FakeFusionWrapper(ClassReferences refs)
    {
        refs.Fusion = this;
    }

    public int LocalPlayerId => throw new System.NotImplementedException();

    public int TurnPlayerId { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public int CallPlayerId { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    public bool IsServer => throw new System.NotImplementedException();

    public bool IsTimerExpired { get; set; }

    bool isTimerRunning = false;
    public bool IsTimerRunning => isTimerRunning;

    public void CreateTimer()
    {
        isTimerRunning = true;
    }

    public void FixedUpdateNetwork()
    {
        throw new System.NotImplementedException();
    }

    public void ResetTimer()
    {
        isTimerRunning = false;
    }

    public void RPC_S2C_SendGameState(int playerId)
    {

    }

    public void RPC_C2A_Expose(int exposeTileId)
    {
        throw new System.NotImplementedException();
    }

    public void RPC_C2S_Discard(int discardTileId)
    {
        throw new System.NotImplementedException();
    }

    public void RPC_S2A_NeverMind()
    {
        throw new System.NotImplementedException();
    }

    public void RPC_S2A_ResetButtons()
    {
        UnityEngine.Debug.Log("RPC_S2A_ResetButtons()");
    }

    public void RPC_S2A_ShowButtons(int discardPlayerId)
    {
        UnityEngine.Debug.Log($"RPC_S2A_ShowButtons({discardPlayerId})");
    }

    public void RPC_S2A_ShowDiscard(int discardTileId)
    {
        UnityEngine.Debug.Log($"RPC_S2A_ShowDiscard({discardTileId})");
    }

    public void RPC_S2C_CallTurn(int callPlayerId, int callTileId)
    {
        UnityEngine.Debug.Log($"RPC_S2C_CallTurn({callPlayerId}, {callTileId})");
    }

    public void RPC_S2C_NextTurn(int nextPlayerId, int nextTileId)
    {
        UnityEngine.Debug.Log($"RPC_S2C_NextTurn({nextPlayerId}, {nextTileId})");
    }

    public void RPC_S2C_SendRack(int playerId, int[] tileArr) { }

    public bool WasCallPressed(int playerId)
    {
        throw new System.NotImplementedException();
    }

    public bool WasNeverMindPressed(int playerId)
    {
        throw new System.NotImplementedException();
    }

    public bool WasPassPressed(int playerId)
    {
        throw new System.NotImplementedException();
    }

    public bool WasWaitPressed(int playerId)
    {
        throw new System.NotImplementedException();
    }
}