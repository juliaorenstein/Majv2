using UnityEngine;

public class SetupMono : MonoBehaviour
{
    ObjectReferences objRefs;
    GameObject TileBackPF;

    void Awake()
    {
        objRefs = ObjectReferences.Instance;
        TileBackPF = Resources.Load<GameObject>("Prefabs/Tile Back");
        PopulateTileList();
    }

    void PopulateTileList()
    {
        foreach (Transform tileTF in objRefs.TilePool)
        {
            Tile tile = tileTF.GetComponent<TileMono>().tile;
            Tile.TileList.Add(tile);
        }
    }
    public void PopulateOtherRacks(bool isDealer)
    {
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 13; j++)
            { Instantiate(TileBackPF, objRefs.OtherRacks.GetChild(i).GetChild(1)); }
        }

        // one more tile for the dealer if this isn't the server/dealer
        if (!isDealer)
        { Instantiate(TileBackPF, objRefs.OtherRacks.GetChild(0).GetChild(1)); }
    }
}
