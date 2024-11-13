using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

public class TileTrackerServerTests
{
    [TestCase(0, TileLoc.PrivateRack0, TileLoc.PrivateRack1)]     // private rack to private rack (charleston)
    [TestCase(149, TileLoc.Wall, TileLoc.PrivateRack1)]           // wall to private rack (next turn)
    [TestCase(4, TileLoc.PrivateRack0, TileLoc.Discard)]          // private rack to discard (discard)
    [TestCase(89, TileLoc.Discard, TileLoc.DisplayRack2)]         // discard to display rack (expose)
    [TestCase(23, TileLoc.PrivateRack1, TileLoc.DisplayRack1)]    // private rack to display rack (expose)

    // display rack to discard (never mind)
    // private rack to other display (joker swap)

    public void MoveTile_WhenCalled_MovesTile(int tileId, TileLoc oldLoc, TileLoc newLoc)
    {
        TileTrackerServerTestVars vars = CreateVariablesForTest();
        Dictionary<TileLoc, IReadOnlyList<int>> map = TileLocToListMap(vars.tileTracker);


        CollectionAssert.Contains(map[oldLoc], tileId); // this is more a check on the test than the code

        vars.tileTracker.MoveTile(tileId, newLoc);

        CollectionAssert.DoesNotContain(map[oldLoc], tileId);
        CollectionAssert.Contains(map[newLoc], tileId);
        Assert.AreEqual(newLoc, vars.tileTracker.TileLocations[tileId].curLoc);
    }

    public void MoveTile_InputRandomList_FailAssertion()
    {
        TileTrackerServerTestVars vars = CreateVariablesForTest();

        UnityEngine.TestTools.LogAssert
            .Expect(UnityEngine.LogType.Assert, "Invalid input for location.");

        vars.tileTracker.MoveTile(32, new());
    }

    // helper functions
    TileTrackerServerTestVars CreateVariablesForTest()
    {
        TileTrackerServerTestVars vars = new();

        ClassReferences refs = new();
        vars.fakeFusionManager = new(refs);
        new FakeFusionWrapper(refs);
        vars.tileTracker = new(refs);
        PopulateTileTracker(vars.tileTracker);

        return vars;
    }

    void PopulateTileTracker(TileTrackerServer tileTracker)
    {
        Tile.TileList = Tile.GenerateTiles();
        for (int i = 0; i < 4; i++)
        {
            foreach (int tileId in GetTestRacks[i])
            {
                tileTracker.MoveTile(tileId, tileTracker.PrivateRackLocations[i]);
            }
        }

        foreach (int tileId in GetTestWall)
        {
            tileTracker.MoveTile(tileId, TileLoc.Wall);
        }

        foreach (int tileId in GetTestDiscard)
        {
            tileTracker.MoveTile(tileId, TileLoc.Discard);
        }
    }

    List<List<int>> GetTestRacks
    {
        get => new() {
            Enumerable.Range(0,13).ToList(),
            Enumerable.Range(20,13).ToList(),
            Enumerable.Range(40,13).ToList(),
            Enumerable.Range(60,14).ToList()
        };
    }

    List<int> GetTestWall { get => Enumerable.Range(100, 50).ToList(); }

    List<int> GetTestDiscard { get => Enumerable.Range(80, 10).ToList(); }

    Dictionary<TileLoc, IReadOnlyList<int>> TileLocToListMap(TileTrackerServer tileTracker)
    {
        return new Dictionary<TileLoc, IReadOnlyList<int>>() {
            {TileLoc.Wall, tileTracker.Wall},
            {TileLoc.Discard, tileTracker.Discard},
            {TileLoc.PrivateRack0, tileTracker.PrivateRacks[0]},
            {TileLoc.PrivateRack1, tileTracker.PrivateRacks[1]},
            {TileLoc.PrivateRack2, tileTracker.PrivateRacks[2]},
            {TileLoc.PrivateRack3, tileTracker.PrivateRacks[3]},
            {TileLoc.DisplayRack0, tileTracker.DisplayRacks[0]},
            {TileLoc.DisplayRack1, tileTracker.DisplayRacks[1]},
            {TileLoc.DisplayRack2, tileTracker.DisplayRacks[2]},
            {TileLoc.DisplayRack3, tileTracker.DisplayRacks[3]},
        };
    }

    struct TileTrackerServerTestVars
    {
        public TileTrackerServer tileTracker;
        public FakeFusionManager fakeFusionManager;
    }
}



