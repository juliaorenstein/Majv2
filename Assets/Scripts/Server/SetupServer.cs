using System.Collections.Generic;
using System.Linq;
using Codice.Client.BaseCommands.Merge;

public class SetupServer
{
    readonly ClassReferences refs;
    bool ShuffledAndDealt;
    readonly TileTrackerServer tileTracker;

    public SetupServer(ClassReferences refs)
    {
        this.refs = refs;
        ShuffledAndDealt = false;
        tileTracker = new(refs);
    }

    public void SetupDriver()
    {
        refs.TManagerServer = new(refs);

        if (!ShuffledAndDealt)
        {
            Shuffle();
            Deal();
            ShuffledAndDealt = true;
        }

        // TODO: eventually make this conditional based on whether all players have joined.
        refs.FManager.GamePhase = GamePhase.Charleston;
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
        foreach (int tileId in shuffleTileList)
        {
            tileTracker.MoveTile(tileId, TileLoc.Wall);
        }
    }

    void Deal()
    {
        for (int i = 0; i < 4; i++)
        {
            TileLoc rack = tileTracker.PrivateRackLocations[i];
            for (int j = 0; j < 13; j++)
            {
                tileTracker.MoveTile(tileTracker.Wall.Last(), rack);
            }
        }

        // one more tile to the dealer
        tileTracker.MoveTile(tileTracker.Wall.Last(), tileTracker.PrivateRackLocations[refs.FManager.Dealer]);
    }
}