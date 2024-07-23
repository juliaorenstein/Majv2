using Fusion;

public class CharlestonFusion : NetworkBehaviour, ICharlestonFusion
{
    CharlestonHost CHost;
    CharlestonClient CClient;
    ObjectReferences Refs;
    [Networked] public int Counter { get; set; }

    public override void Spawned()
    {
        Refs = ObjectReferences.Instance;
        CHost = new(Refs.ClassRefs);
        CClient = new(Refs.ClassRefs);
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