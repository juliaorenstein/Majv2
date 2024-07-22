using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;

public class SendGameState : NetworkBehaviour
{
    ObjectReferences Refs = ObjectReferences.Instance;

    private void Awake()
    {
        Refs.SendGame = this;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, TickAligned = false)]
    public void RPC_SendRackToPlayer(int playerId, int[] tileArr) => PopulateLocalRack(tileArr);

    public void PopulateLocalRack(int[] tileArr)
    {
        Refs.ClassRefs.GManagerClient.DisplayRack = tileArr.ToList();
        foreach (int tileId in tileArr)
        {
            Refs.Mono.MoveTile(tileId, MonoObject.PrivateRack);
        }
    }
}
