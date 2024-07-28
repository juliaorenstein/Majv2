using System.Collections.Generic;
using Fusion;

public interface IFusionManager
{
    Dictionary<int, InputCollection> InputDict { get; set; }
    GamePhase GamePhase { get; set; }
    TurnPhase TurnPhase { get; set; }
}