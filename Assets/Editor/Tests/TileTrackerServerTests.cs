using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

public class TileTrackerServerTests
{
    [TestCase(0, Loc.PrivateRack0, Loc.PrivateRack1)]     // private rack to private rack (charleston)
    [TestCase(149, Loc.Wall, Loc.PrivateRack1)]           // wall to private rack (next turn)
    [TestCase(4, Loc.PrivateRack0, Loc.Discard)]          // private rack to discard (discard)
    [TestCase(89, Loc.Discard, Loc.DisplayRack2)]         // discard to display rack (expose)
    [TestCase(23, Loc.PrivateRack1, Loc.DisplayRack1)]    // private rack to display rack (expose)

    // display rack to discard (never mind)
    // private rack to other display (joker swap)

    public void MoveTile_WhenCalled_MovesTile(int tileId, Loc oldLoc, Loc newLoc)
    {
        TileTrackerServerTestVars vars = CreateVariablesForTest();
        List<int> oldLocation = vars.validLocs[oldLoc];
        List<int> newLocation = vars.validLocs[newLoc];

        CollectionAssert.Contains(oldLocation, tileId); // this is more a check on the test than the code

        vars.tileTracker.MoveTile(tileId, newLocation);

        CollectionAssert.DoesNotContain(oldLocation, tileId);
        CollectionAssert.Contains(newLocation, tileId);
        Assert.AreEqual(newLocation, vars.tileTracker.TileLocations[tileId]);
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
        vars.validLocs = ValidLocations(vars.tileTracker);

        return vars;
    }

    void PopulateTileTracker(TileTrackerServer tileTracker)
    {
        Tile.TileList = Tile.GenerateTiles();
        for (int i = 0; i < 4; i++)
        {
            foreach (int tileId in GetTestRacks[i])
            {
                tileTracker.MoveTile(tileId, tileTracker.PrivateRacks[i]);
            }
        }

        foreach (int tileId in GetTestWall)
        {
            tileTracker.MoveTile(tileId, tileTracker.Wall);
        }

        foreach (int tileId in GetTestDiscard)
        {
            tileTracker.MoveTile(tileId, tileTracker.Discard);
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

    Dictionary<Loc, List<int>> ValidLocations(TileTrackerServer tileTracker)
    {
        return new Dictionary<Loc, List<int>>() {
            {Loc.Wall, tileTracker.Wall},
            {Loc.Discard, tileTracker.Discard},
            {Loc.PrivateRack0, tileTracker.PrivateRacks[0]},
            {Loc.PrivateRack1, tileTracker.PrivateRacks[1]},
            {Loc.PrivateRack2, tileTracker.PrivateRacks[2]},
            {Loc.PrivateRack3, tileTracker.PrivateRacks[3]},
            {Loc.DisplayRack0, tileTracker.DisplayRacks[0]},
            {Loc.DisplayRack1, tileTracker.DisplayRacks[1]},
            {Loc.DisplayRack2, tileTracker.DisplayRacks[2]},
            {Loc.DisplayRack3, tileTracker.DisplayRacks[3]},
        };
    }

    public enum Loc
    {
        Wall,
        Discard,
        PrivateRack0,
        PrivateRack1,
        PrivateRack2,
        PrivateRack3,
        DisplayRack0,
        DisplayRack1,
        DisplayRack2,
        DisplayRack3,
    }

    struct TileTrackerServerTestVars
    {
        public TileTrackerServer tileTracker;
        public FakeFusionManager fakeFusionManager;
        public Dictionary<Loc, List<int>> validLocs;
    }
}



