using System.Collections.Generic;
using System.Linq;

public class SetupHost
{
    ClassReferences Refs;
    bool ShuffledAndDealt;
    TileTracker tileTracker;

    public SetupHost(ClassReferences refs)
    {
        Refs = refs;
        ShuffledAndDealt = false;
        tileTracker = new(refs);
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
        tileTracker.Wall = new(shuffleTileList);
        foreach (int tileId in tileTracker.Wall)
        {
            tileTracker.TileLocations[tileId] = tileTracker.Wall;
        }
    }

    void Deal()
    {
        List<int> rack;

        for (int i = 0; i < 4; i++)
        {   
            rack = new();
            tileTracker.PrivateRacks[i] = rack;

            for (int j = 0; j < 13; j++)
            {
                tileTracker.SimpleMoveTile(tileTracker.Wall.Last(), rack);
            }
        }

        // one more tile to the dealer
        tileTracker.SimpleMoveTile(tileTracker.Wall.Last(), tileTracker.PrivateRacks[Refs.GManager.DealerId]);
    }
}