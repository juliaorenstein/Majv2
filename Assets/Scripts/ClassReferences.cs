using Fusion;

// This class is intended to hold references to wrappers and managers that are
// not monobehaviours or networkbehaviours. Offshoot from ObjectReferences as I
// don't want to bother stubbing ObjectReferences. Will slowly transition
// classes 
public class ClassReferences
{
    // Initiated in ObjectReferences

    public NetworkCallbacks NetworkCallbacks;
    public Navigation Nav;
    public NetworkRunner Runner;
    public CharlestonHost CHost;
    public CharlestonClient CClient;
    public ICharlestonFusion CFusion;
    public TurnManager TManager;
    public IFusionManager FManager;
    public GameManager GManager;            // TODO: deprecate
    public GameManagerClient GManagerClient;// TODO: deprecate
    public TileTracker TileTracker;
    public TileTrackerClient TileTrackerClient;
    public IMonoWrapper Mono;
    public IFusionWrapper Fusion;
    public SendGameState SendGame;
    public ReceiveGameState ReceiveGame;
}
