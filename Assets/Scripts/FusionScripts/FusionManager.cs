using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class FusionManager : NetworkBehaviour, IFusionManager
{
    public Dictionary<int, PlayerRef> PlayerDict { get; set; }
    public Dictionary<int, InputCollection> InputDict { get; set; }

    // Shared between server and client
    [Networked] public GamePhase GamePhase { get; set; }
    [Networked] public TurnPhase TurnPhase { get; set; }

    // players
    [Networked] public int LocalPlayer { get; set; }
    [Networked] public int Dealer { get; set; }
    public bool IsDealer { get => LocalPlayer == Dealer; }
    [Networked] public int ActivePlayer { get; set; } = -1;
    public bool IsActivePlayer { get => ActivePlayer == LocalPlayer; }
    [Networked] public int ExposingPlayer { get; set; } = -1;
    public bool IsExposingPlayer { get => ExposingPlayer == LocalPlayer; }

    public override void Spawned()
    {
        ObjectReferences.Instance.ClassRefs.FManager = this;
        Dealer = 3; // TODO: make this rotate
        PlayerDict = new();
        InputDict = new();
        for (int playerID = 0; playerID < 4; playerID++)
        {
            PlayerDict[playerID] = PlayerRef.None;
        }
    }

    public void InitializePlayer(PlayerRef player)
    {
        NetworkObject newInputObj = Runner.Spawn(Resources.Load<GameObject>("Prefabs/Input Object"));
        newInputObj.AssignInputAuthority(player);

        PlayerDict[player.PlayerId] = player;
        InputDict.Add(player.PlayerId, newInputObj.GetComponent<InputCollection>());
    }
}

public enum GamePhase
{ // FIXME: set these in the appropriate places so they work in TileLocomotion
    Pregame,
    Charleston,
    Gameplay,
    Endgame
}

public enum TurnPhase
{
    Discarding,
    LoggingCallers,
    Exposing,
}