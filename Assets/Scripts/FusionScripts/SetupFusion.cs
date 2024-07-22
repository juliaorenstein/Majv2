using UnityEngine;
using Fusion;
/*
public class SetupFusion : NetworkBehaviour, ISetupFusion
{
    IFusionManager FManager;
    Setup setup;

    public void SetupNetworkObject(PlayerRef player)
    {
        NetworkObject newInputObj = Runner.Spawn(Resources.Load<GameObject>("Prefabs/Input Object"));

        FManager.PlayerDict.Add(player.PlayerId, player);
        FManager.InputDict.Add(player.PlayerId, newInputObj.GetComponent<InputCollection>());

        newInputObj.AssignInputAuthority(player);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, TickAligned = false)]
    public void RPC_SendRackToPlayer(int playerId, int[] tileArr) => setup.PopulateLocalRack(tileArr);
}
*/
