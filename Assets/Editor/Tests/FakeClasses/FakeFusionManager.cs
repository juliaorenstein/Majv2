using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FakeFusionManager : IFusionManager
{
    public Dictionary<int, InputCollection> InputDict { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public GamePhase GamePhase { get; set; }
    public TurnPhase TurnPhase { get; set; }

    // players
    public int LocalPlayer { get; set; }
    public int Dealer { get; set; }
    public bool IsDealer { get => LocalPlayer == Dealer; }
    public int ActivePlayer { get; set; }
    public bool IsActivePlayer { get => ActivePlayer == LocalPlayer; }
    public int ExposingPlayer { get; set; }
    public bool IsExposingPlayer { get => ExposingPlayer == LocalPlayer; }

    public FakeFusionManager(ClassReferences refs) => refs.FManager = this;
}
