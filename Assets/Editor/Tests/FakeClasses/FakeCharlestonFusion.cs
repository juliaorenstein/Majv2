class FakeCharlestonFusion : ICharlestonFusion
{
    public int Counter { get; set; }
    public void RPC_C2H_StartPass(int[] tileIDsToPass)
    { }
    public void RPC_H2C_UpdateRack(int playerId, int[] newRack)
    { }

    public FakeCharlestonFusion(ClassReferences refs)
    {
        refs.CFusion = this;
        Counter = 0;
    }
}
