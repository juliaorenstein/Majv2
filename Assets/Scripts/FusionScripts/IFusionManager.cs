using System.Collections.Generic;
using Fusion;

public interface IFusionManager
{
    GamePhase GamePhase { get; set; }
    TurnPhase TurnPhase { get; set; }

    // players
    public bool IsServer { get; }
    public bool IsPlayerAI(int playerID);
    public int LocalPlayer { get; set; }
    public int Dealer { get; set; }
    public bool IsDealer { get; }
    public int ActivePlayer { get; set; }
    public bool IsActivePlayer { get; }
    public int ExposingPlayer { get; set; }
    public bool IsExposingPlayer { get; }

    bool CallPressed(int playerId);
    bool NeverMindPressed(int playerId);
    bool PassPressed(int playerId);
    bool WaitPressed(int playerId);
}