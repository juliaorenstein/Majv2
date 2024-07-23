using System.Collections.Generic;
using System.Linq;

public class SetupHost
{
    ClassReferences Refs;
    bool ShuffledAndDealt;

    public SetupHost(ClassReferences refs)
    {
        Refs = refs;
        ShuffledAndDealt = false;
    }

    public void SetupDriver()
    {
        Refs.TManager = new(Refs); // TODO: rename to TManagerHost?

        if (!ShuffledAndDealt)
        {
            Shuffle();
            Deal();
            ShuffledAndDealt = true;
        }
    }

    void Shuffle()
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

    void Deal()
    {
        List<int> rack;

        for (int i = 0; i < 4; i++)
        {   
            rack = new();
            Refs.GManager.Racks.Add(rack);

            for (int j = 0; j < 13; j++)
            {   
                rack.Add(Refs.GManager.Wall.Pop());
            }
        }

        // one more tile to the dealer
        Refs.GManager.Racks[Refs.GManager.DealerId].Add(Refs.GManager.Wall.Pop());
    }

    public void SendRack(int playerId)
    {
        int[] rack = Refs.GManager.Racks[playerId].ToArray();
        Refs.Fusion.RPC_H2C_SendRack(playerId, rack);
    }
}
