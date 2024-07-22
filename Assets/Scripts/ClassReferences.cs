using System;

// This class is intended to hold references to wrappers and managers that are
// not monobehaviours or networkbehaviours. Offshoot from ObjectReferences as I
// don't want to bother stubbing ObjectReferences. Will slowly transition
// classes 
public class ClassReferences
{
    // Initiated in ObjectReferences

    // singleton pattern
    /*private static readonly Lazy<ClassReferences> lazy =
            new Lazy<ClassReferences>(() => new ClassReferences());

    private static ClassReferences instance;
    public static ClassReferences Instance
    {
        get
        {
            return lazy.Value;
        }
    }

    public static void SetTestingInstance(ClassReferences newInstance)
    {
        instance = newInstance;
    }
    */

    

    public CharlestonHost CHost;
    public CharlestonClient CClient;
    public ICharlestonFusion CFusion;
    public TurnManager TManager;
    public FusionManager FManager;
    public GameManager GManager;
    public GameManagerClient GManagerClient;
    public IMonoWrapper Mono;
    public IFusionWrapper Fusion;
}
