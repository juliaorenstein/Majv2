using Fusion;

public class CharlestonFusion : NetworkBehaviour, ICharlestonFusion
{
    CharlestonHost CHost;
    CharlestonClient CClient;
    ObjectReferences objRefs;
    [Networked] public int Counter { get; set; }

    public override void Spawned()
    {
        objRefs = ObjectReferences.Instance;
        objRefs.ClassRefs.CFusion = this;
        CHost = new(objRefs.ClassRefs);
        CClient = new(objRefs.ClassRefs);
        Counter = 0;
    }

    // send tiles from Client to Host
    public void RPC_C2H_StartPass(int[] tileIDsToPass) =>
        RPC_C2H_StartPassWithInfo(tileIDsToPass);
    [Rpc(RpcSources.All, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_C2H_StartPassWithInfo(int[] tileIDsToPass, RpcInfo info = default)
    {
        CHost.PassDriver(info.Source.PlayerId, tileIDsToPass);
    }
}