using System.Collections.Generic;
using Fusion;

public interface IFusionManager
{
    Dictionary<int, InputCollection> InputDict { get; set; }
    GamePhase GamePhase { get; set; }
    TurnPhase TurnPhase { get; set; }

    public int LocalPlayer { get; set; }
    public int Dealer { get; set; }
    public bool IsDealer { get; }
    public int ActivePlayer { get; set; }
    public bool IsActivePlayer { get; }
    public int ExposingPlayer { get; set; }
    public bool IsExposingPlayer { get; }


}