using System.Collections.Generic;
using System.Linq;

public class SetupHost
{
    readonly ClassReferences refs;
    bool ShuffledAndDealt;
    readonly TileTracker tileTracker;

    public SetupHost(ClassReferences refs)
    {
        this.refs = refs;
        ShuffledAndDealt = false;
        tileTracker = new(refs);
    }

    public void SetupDriver()
    {
        refs.TManager = new(refs);

        if (!ShuffledAndDealt)
        {
            Shuffle();
            Deal();
            ShuffledAndDealt = true;
        }

        refs.FManager.GamePhase = GamePhase.Charleston;
        // TODO: eventually make this conditional based on whether all players have joined.
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
        for (int i = 0; i < 4; i++)
        {
            List<int> rack = tileTracker.PrivateRacks[i];
            for (int j = 0; j < 13; j++)
            {
                tileTracker.SimpleMoveTile(tileTracker.Wall.Last(), rack);
            }
        }

        // one more tile to the dealer
        tileTracker.SimpleMoveTile(tileTracker.Wall.Last(), tileTracker.PrivateRacks[refs.GManager.DealerId]);
    }
}