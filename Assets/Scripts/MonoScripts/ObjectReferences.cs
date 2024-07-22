using UnityEngine;
using Fusion;
using System.Collections.Generic;

public class ObjectReferences : MonoBehaviour
{
    public ClassReferences ClassRefs;

    public SetupMono setupMono;
    public MonoWrapper Mono;
    public FusionWrapper Fusion;
    public GameManager GManager = GameManager.Instance;
    public CharlestonManager CManager;
    public FusionManager FManager;
    public SendGameState SendGame;
    public NetworkCallbacks NetworkCallbacks;
    public NetworkRunner Runner;

    public Transform Discard;
    public Transform TilePool;
    public Transform LocalRack;
    public Transform OtherRacks;
    public Transform EventSystem;
    public Transform StartButtons;
    public Transform CharlestonBox;
    public Transform CallWaitButtons;
    public Transform CharlestonPassButton;
    public Transform TurnIndicator;
    public Transform Dragging;

    // Unity Singleton
    private static ObjectReferences _instance;
    public static ObjectReferences Instance { get { return _instance; } }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    public Dictionary<MonoObject, Transform> ObjectDict;

    void Start()
    {
        ClassRefs = new();

        Discard = GameObject.Find("Board").transform.GetChild(0);
        TilePool = GameObject.Find("Tile Pool").transform;
        LocalRack = GameObject.Find("Local Rack").transform;
        OtherRacks = GameObject.Find("Other Racks").transform;
        EventSystem = GameObject.Find("EventSystem").transform;
        StartButtons = GameObject.Find("Start Buttons").transform;
        CharlestonBox = GameObject.Find("Charleston Box").transform;
        CallWaitButtons = GameObject.Find("Board").transform.GetChild(7);
        CharlestonPassButton = GameObject.Find("Charleston Pass Button").transform;
        TurnIndicator = GameObject.Find("Turn Indicator").transform;
        Dragging = GameObject.Find("Dragging").transform;

        ObjectDict = new()
        {
            { MonoObject.Discard, Discard },
            { MonoObject.CharlestonBox, CharlestonBox },
            { MonoObject.CharlestonPassButton, CharlestonPassButton },
            { MonoObject.TilePool, TilePool },
            { MonoObject.CallWaitButtons, CallWaitButtons },
            { MonoObject.WaitButton, CallWaitButtons.GetChild(0) },
            { MonoObject.PassButton, CallWaitButtons.GetChild(1) },
            { MonoObject.NeverMind, CallWaitButtons.GetChild(3) },
            { MonoObject.PrivateRack, LocalRack.GetChild(1) },
            { MonoObject.PublicRack, LocalRack.GetChild(0) },
            { MonoObject.StartButtons, StartButtons }
        };
    }
}

public enum MonoObject
{
    StartButtons,
    CharlestonBox,
    CharlestonPassButton,
    TilePool,
    Discard,
    CallWaitButtons,
    WaitButton,
    PassButton,
    NeverMind,
    PrivateRack,
    PublicRack,
}