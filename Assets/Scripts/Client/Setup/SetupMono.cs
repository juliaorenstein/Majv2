using UnityEngine;

public class SetupMono : MonoBehaviour
{
    ObjectReferences Refs;
    GameObject TileBackPF;

    void Awake()
    {
        Refs = ObjectReferences.Instance;
        TileBackPF = Resources.Load<GameObject>("Prefabs/Tile Back");
    }

    public void PopulateOtherRacks(bool isDealer)
    {
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 13; j++)
            { Instantiate(TileBackPF, Refs.OtherRacks.GetChild(i).GetChild(1)); }
        }

        // one more tile for the dealer if this isn't the server/dealer
        if (!isDealer)
        { Instantiate(TileBackPF, Refs.OtherRacks.GetChild(0).GetChild(1)); }
    }
}
