using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class FusionManager : NetworkBehaviour
{
    public Dictionary<int, PlayerRef> PlayerDict { get; set; }
    public Dictionary<int, InputCollection> InputDict { get; set; }

    public override void Spawned()
    {
        PlayerDict = new();
        InputDict = new();
        for (int playerID = 0; playerID < 4; playerID++)
        {
            PlayerDict[playerID] = PlayerRef.None;
        }
    }

    public void H_InitializePlayer(PlayerRef player)
    {
        NetworkObject newInputObj = Runner.Spawn(Resources.Load<GameObject>("Prefabs/Input Object"));
        newInputObj.AssignInputAuthority(player);

        PlayerDict[player.PlayerId] = player;
        InputDict.Add(player.PlayerId, newInputObj.GetComponent<InputCollection>());
    }
}