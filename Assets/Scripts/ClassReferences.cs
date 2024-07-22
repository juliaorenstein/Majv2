using System;

// This class is intended to hold references to wrappers and managers that are
// not monobehaviours or networkbehaviours. Offshoot from ObjectReferences as I
// don't want to bother stubbing ObjectReferences. Will slowly transition
// classes 
public class ClassReferences
{
    // Initiated in ObjectReferences

    public CharlestonHost CHost;
    public CharlestonClient CClient;
    public ICharlestonFusion CFusion;
    public TurnManager TManager;
    public FusionManager FManager;
    public GameManager GManager;
    public GameManagerClient GManagerClient;
    public IMonoWrapper Mono;
    public IFusionWrapper Fusion;
    public SendGameState SendGame;
    public ReceiveGameState ReceiveGame;
}
