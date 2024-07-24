using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FakeFusionManager : IFusionManager
{
    public Dictionary<int, InputCollection> InputDict { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public GamePhase GamePhase { get; set; }
    public TurnPhase TurnPhase { get; set; }

    public FakeFusionManager(ClassReferences refs) => refs.FManager = this;
}
