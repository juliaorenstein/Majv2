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
    public CharlestonServer CHost;
    public CharlestonClient CClient;
    public ICharlestonFusion CFusion;
    public TurnManagerServer TManager;
    public TurnManagerClient TManagerClient;
    public IFusionManager FManager;
    public GameManager GManager;            // TODO: deprecate
    public TileTrackerServer TileTracker;
    public TileTrackerClient TileTrackerClient;
    public IMonoWrapper Mono;
    public IFusionWrapper Fusion;
    public SendGameState SendGame;
}
