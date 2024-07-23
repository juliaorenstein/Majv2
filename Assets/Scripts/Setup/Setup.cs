using System.Collections.Generic;
using System.Linq;

public class Setup
{
    /*
    public ObjectReferences Refs;
    public SetupMono setupMono;

    public Setup()
    {
        Refs = ObjectReferences.Instance;
    }

    public void C_Setup()
    {
        Refs.GManager.LocalPlayerId = Refs.Fusion.LocalPlayerId; // set player ID
        Refs.GManager.DealerId = 3;                    // make the server the dealer
        Refs.EventSystem.gameObject.AddComponent<Navigation>();

        //Refs.Charleston.transform.SetParent(Refs.Board.transform);
        HideButtons();                      // hide start buttons
        // show the other player's racks
        setupMono.PopulateOtherRacks(Refs.GManager.DealerId == Refs.GManager.LocalPlayerId);               
    }

    public void H_Setup(int playerId)
    {

        if (playerId == Refs.Fusion.LocalPlayerId)    // one time actions when the host joins
        {
            Shuffle();
            Deal();
        }
        int[] tileArr = PrepRackForClient(playerId);
        Refs.Fusion.RPC_H2C_SendRack(playerId, tileArr);
    }

    // FISHER-YATES
    public void Shuffle()
    {
        List<int> shuffleTileList = Enumerable.Range(0, 152).ToList();
        int tmp;
        int k;

        System.Random rnd = new();

        for (int i = shuffleTileList.Count - 1; i > 0; i--)
        {
            k = rnd.Next(i);
            tmp = shuffleTileList[k];
            shuffleTileList[k] = shuffleTileList[i];
            shuffleTileList[i] = tmp;
        }

        // CREATE THE WALL
        Refs.GManager.Wall = new(shuffleTileList);
    }

    public void Deal()
    {
        List<int> rack;

        for (int i = 0; i < 4; i++)
        {   // ADD FOUR RACKS TO RACKS FIELD
            rack = new();
            Refs.GManager.Racks.Add(rack);

            for (int j = 0; j < 13; j++)
            {   // ADD THIRTEEN TILES TO EACH RACK
                rack.Add(Refs.GManager.Wall.Pop());
            }    
        }

        // DEAL ONE MORE TILE TO THE DEALER
        Refs.GManager.Racks[Refs.GManager.DealerId].Add(Refs.GManager.Wall.Pop());
    }

    // PREPARE A LIST OF TILE IDS TO SEND TO CLIENT FOR RPC_SendRackToPlayer
    int[] PrepRackForClient(int playerID)
    {
        List<int> RackTileIDs = new();
        foreach (int tile in Refs.GManager.Racks[playerID])
        {
            RackTileIDs.Add(tile);
        }
        return RackTileIDs.ToArray();
    }

    void HideButtons() => Refs.Mono.SetActive(MonoObject.StartButtons, false);
    */
}
