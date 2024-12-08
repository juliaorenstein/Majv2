using NUnit.Framework;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEditor;
using System;

public class TileTrackerClientTests
{
    [Test]
    public void ReceiveGameState_BeginningOfGame_WallAndRacksPopulate()
    {
        // the following step is setup but also calls ReceiveGameState
        // for inital population of TileTrackerClient, so this is 
        // arrange and act in one
        Vars vars = MakeVariablesForTest();
        AssertAssembly(vars);
    }

    // all subsequent tests assume first test passes
    [TestCase(0)]
    [TestCase(6)]
    [TestCase(12)]
    public void ReceiveGameState_LocalDiscard_TileMovesPrivateRackToDiscard(int ix)
    {
        Vars vars = MakeVariablesForTest();

        int tileId = vars.testPrivateRack[ix];
        vars.testPrivateRack.RemoveAt(ix);
        vars.testDiscard.Add(tileId);

        ReceiveGameState(vars);
        AssertAssembly(vars);
    }

    [Test]
    public void ReceiveGameState_ExternalDiscard_TileMovesTilePoolToDiscard()
    {
        Vars vars = MakeVariablesForTest();

        vars.testPrivateRackCounts[3] -= 1;
        vars.testDiscard.Add(94);

        ReceiveGameState(vars);
        AssertAssembly(vars);
    }

    public void ReceiveGameState_ExternalDiscard_ExternalPrivateRackCountUpdates()
    {
        // actually covered by test above
    }

    [Test]
    public void ReceiveGameState_LocalNextTurn_TileMovesTilePoolToPrivateRack()
    {
        Vars vars = MakeVariablesForTest();

        vars.testWallCount--;
        vars.testPrivateRack.Add(35);

        ReceiveGameState(vars);
        AssertAssembly(vars);
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    public void ReceiveGameState_ExternalNextTurn_ExternalPrivateRackCountUpdates(int playerId)
    {
        Vars vars = MakeVariablesForTest();

        vars.testWallCount--;
        vars.testPrivateRackCounts[playerId]++;

        ReceiveGameState(vars);
        AssertAssembly(vars);

        // FIXME: if this passes for local player, maybe it shouldn't and there should be some 
        //      sort of logic to ensure that local player private rack count actually matches the real count
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    public void ReceiveGameState_Call_TileMovesDiscardToDisplayRack(int playerId)
    {
        Vars vars = MakeVariablesForTest();

        int tileId = vars.testDiscard.Last();
        vars.testDiscard.Remove(tileId);
        vars.testDisplayRacks[playerId].Add(tileId);

        ReceiveGameState(vars);
        AssertAssembly(vars);
    }

    [Test]
    public void ReceiveGameState_LocalExpose_TileMovesPrivateRackToLocalDisplayRack()
    {
        Vars vars = MakeVariablesForTest();

        int tileId = vars.testPrivateRack.Last();
        vars.testPrivateRack.Remove(tileId);
        vars.testDisplayRacks[0].Add(tileId);

        ReceiveGameState(vars);
        AssertAssembly(vars);
    }

    [Test]
    public void ReceiveGameState_ExternalExpose_TileMovesTilePoolToExternalDisplayRack()
    {
        Vars vars = MakeVariablesForTest();

        int tileId = 95;
        vars.testPrivateRackCounts[1]--;
        vars.testDisplayRacks[1].Add(tileId);

        ReceiveGameState(vars);
        AssertAssembly(vars);
    }

    public void ReceiveGameState_ExternalExpose_ExternalPrivateRackCountUpdates()
    {
        // covered above
    }

    [Test]
    public void ReceiveGameState_LocalNeverMind_TileMovesDisplayRackToDiscard()
    {
        throw new NotImplementedException();
    }

    [Test]
    public void ReceiveGameState_ExternalNeverMind_TileMovesDisplayRackToDiscard()
    {
        throw new NotImplementedException();
    }


    Vars MakeVariablesForTest()
    {
        Vars vars = new();

        List<Tile> tiles = Tile.GenerateTiles();
        ClassReferences refs = new();
        new FakeMonoWrapper(refs);
        new CharlestonClient(refs);
        new FakeFusionManager(refs);
        vars.tileTracker = new(refs);
        PopulateTileTracker(vars.tileTracker);

        vars.testWallCount = GetWallCount;
        vars.testDiscard = GetDiscard;
        vars.testPrivateRack = GetPrivateRack;
        vars.testPrivateRackCounts = GetPrivateRackCounts;
        vars.testDisplayRacks = GetDisplayRacks;

        return vars;
    }


    struct Vars
    {
        public TileTrackerClient tileTracker;
        public int testWallCount;
        public List<int> testDiscard;
        public List<int> testPrivateRack;
        public int[] testPrivateRackCounts;
        public List<List<int>> testDisplayRacks;
    }

    TileTrackerClient PopulateTileTracker(TileTrackerClient tileTracker)
    {
        tileTracker.ReceiveGameState(new()
        {
            TileDict = GetTileLocsFromServer(),
            WallCount = GetWallCount,
            PrivateRackCounts = GetPrivateRackCounts
        });

        return tileTracker;
    }

    int GetWallCount => 89;
    List<int> GetDiscard => Enumerable.Range(120, 10).ToList();
    List<int> GetPrivateRack => Enumerable.Range(0, 13).ToList();
    int[] GetPrivateRackCounts => new int[4] { 13, 13, 13, 14 };
    List<List<int>> GetDisplayRacks => new() {
        new List<int> { },
        new List<int> { 100, 101, 102 },
        new List<int> { 110, 111, 112, 113 },
        new List<int> { },
    };

    Dictionary<int, LocChange> GetTileLocsFromServer()
    {
        Dictionary<int, LocChange> outDict = new();

        LocChange discard = new() { lastLoc = TileLoc.Wall, curLoc = TileLoc.Discard };
        foreach (int tileId in GetDiscard) { outDict[tileId] = discard; }

        LocChange privateRack = new() { lastLoc = TileLoc.Wall, curLoc = TileLoc.PrivateRack0 };
        foreach (int tileId in GetPrivateRack) { outDict[tileId] = privateRack; }

        LocChange displayRack0 = new() { lastLoc = TileLoc.Wall, curLoc = TileLoc.DisplayRack0 };
        foreach (int tileId in GetDisplayRacks[0]) { outDict[tileId] = displayRack0; }

        LocChange displayRack1 = new() { lastLoc = TileLoc.Wall, curLoc = TileLoc.DisplayRack1 };
        foreach (int tileId in GetDisplayRacks[1]) { outDict[tileId] = displayRack1; }

        LocChange displayRack2 = new() { lastLoc = TileLoc.Wall, curLoc = TileLoc.DisplayRack2 };
        foreach (int tileId in GetDisplayRacks[2]) { outDict[tileId] = displayRack2; }

        LocChange displayRack3 = new() { lastLoc = TileLoc.Wall, curLoc = TileLoc.DisplayRack3 };
        foreach (int tileId in GetDisplayRacks[3]) { outDict[tileId] = displayRack3; }

        return outDict;
    }

    void ReceiveGameState(Vars vars)
    {
        vars.tileTracker.ReceiveGameState(
            vars.testWallCount
            , vars.testDiscard.ToArray()
            , vars.testPrivateRack.ToArray()
            , vars.testPrivateRackCounts
            , vars.testDisplayRacks[0].ToArray()
            , vars.testDisplayRacks[1].ToArray()
            , vars.testDisplayRacks[2].ToArray()
            , vars.testDisplayRacks[3].ToArray());
    }

    void AssertAssembly(Vars vars)
    {
        Assert.AreEqual(vars.testWallCount, vars.tileTracker.WallCount);
        CollectionAssert.AreEqual(vars.testDiscard, vars.tileTracker.Discard);
        CollectionAssert.AreEqual(vars.testPrivateRack, vars.tileTracker.LocalPrivateRack);
        CollectionAssert.AreEqual(vars.testPrivateRackCounts, vars.tileTracker.PrivateRackCounts);
        for (int i = 0; i < 4; i++)
        {
            CollectionAssert.AreEqual(vars.testDisplayRacks[i], vars.tileTracker.DisplayRacks[i]);
        }
    }
}
