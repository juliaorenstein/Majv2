using System.Collections.Generic;

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

    public int ActiveDiscardTileId { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    public bool IsTimerExpired => throw new System.NotImplementedException();

    public bool IsTimerRunning => throw new System.NotImplementedException();

    public void CreateTimer()
    {
        throw new System.NotImplementedException();
    }

    public void FixedUpdateNetwork()
    {
        throw new System.NotImplementedException();
    }

    public List<int> AiPlayers = new();
    public bool IsPlayerAI(int playerID) => AiPlayers.Contains(playerID);

    public void ResetTimer()
    {
        throw new System.NotImplementedException();
    }

    public void RPC_S2C_SendGameState(int playerId, NetworkableTileLocations tileLocs)
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
        throw new System.NotImplementedException();
    }

    public void RPC_S2A_ShowButtons(int discardPlayerId)
    {
        throw new System.NotImplementedException();
    }

    public void RPC_S2A_ShowDiscard(int discardTileId)
    {
        throw new System.NotImplementedException();
    }

    public void RPC_S2C_CallTurn(int callPlayerId, int callTileId)
    {
        throw new System.NotImplementedException();
    }

    public void RPC_S2C_NextTurn(int nextPlayerId, int nextTileId)
    {
        throw new System.NotImplementedException();
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