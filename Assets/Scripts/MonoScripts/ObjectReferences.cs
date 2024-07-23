using UnityEngine;
using System.Collections.Generic;

public class ObjectReferences : MonoBehaviour
{
    public ClassReferences ClassRefs;

    public Transform Discard;
    public Transform TilePool;
    public Transform LocalRack;
    public Transform PublicRack;
    public Transform PrivateRack;
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
    public Dictionary<Transform, MonoObject> ReverseObjectDict;
    public List<MonoObject> CharlestonSpots;

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
        PrivateRack = GameObject.Find("Rack - Private").transform;
        PublicRack = GameObject.Find("Rack - Display").transform;
        Dragging = GameObject.Find("Dragging").transform;

        ObjectDict = new()
        {
            { MonoObject.Discard, Discard },
            { MonoObject.CharlestonBox, CharlestonBox },
            { MonoObject.CharlestonPassButton, CharlestonPassButton },
            { MonoObject.CharlestonSpot1, CharlestonBox.GetChild(0) },
            { MonoObject.CharlestonSpot2, CharlestonBox.GetChild(1) },
            { MonoObject.CharlestonSpot3, CharlestonBox.GetChild(2) },
            { MonoObject.TilePool, TilePool },
            { MonoObject.CallWaitButtons, CallWaitButtons },
            { MonoObject.WaitButton, CallWaitButtons.GetChild(0) },
            { MonoObject.PassButton, CallWaitButtons.GetChild(1) },
            { MonoObject.NeverMind, CallWaitButtons.GetChild(3) },
            { MonoObject.PrivateRack, PrivateRack },
            { MonoObject.PublicRack, PublicRack },
            { MonoObject.StartButtons, StartButtons }
        };

        ReverseObjectDict = new();
        foreach (KeyValuePair<MonoObject, Transform> item in ObjectDict)
        {
            ReverseObjectDict[item.Value] = item.Key;
        }

        CharlestonSpots = new()
        {
            MonoObject.CharlestonSpot1,
            MonoObject.CharlestonSpot2,
            MonoObject.CharlestonSpot3
        };
    }
}

public enum MonoObject
{
    StartButtons,
    CharlestonBox,
    CharlestonPassButton,
    CharlestonSpot1,
    CharlestonSpot2,
    CharlestonSpot3,
    TilePool,
    Discard,
    CallWaitButtons,
    WaitButton,
    PassButton,
    NeverMind,
    PrivateRack,
    PublicRack,
}