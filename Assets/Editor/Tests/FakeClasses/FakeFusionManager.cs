using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using UnityEngine;

public class FakeFusionManager : IFusionManager
{
    public Dictionary<int, InputForServer> InputDict { get; set; }
    public GamePhase GamePhase { get; set; }
    public TurnPhase TurnPhase { get; set; }
    public FakeFusionManager(ClassReferences refs) => refs.FManager = this;

    // players
    public bool IsServer { get; set; }

    public List<int> AiPlayers = new();
    public bool IsPlayerAI(int playerID) => AiPlayers.Contains(playerID);
    public int LocalPlayer { get; set; }
    public int Dealer { get; set; }
    public bool IsDealer { get => LocalPlayer == Dealer; }
    public int ActivePlayer { get; set; }
    public bool IsActivePlayer { get => ActivePlayer == LocalPlayer; }
    public int ExposingPlayer { get; set; }
    public bool IsExposingPlayer { get => ExposingPlayer == LocalPlayer; }
    public int ActiveDiscardTileId { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    // input
    public bool WaitPressed(int playerId) => InputDict[playerId].input == Buttons.wait;
    public bool PassPressed(int playerId) => InputDict[playerId].input == Buttons.pass;
    public bool CallPressed(int playerId) => InputDict[playerId].input == Buttons.call;
    public bool NeverMindPressed(int playerId) => InputDict[playerId].input == Buttons.nevermind;
}
