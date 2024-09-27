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

    [TestCase(0, 6, 13)]
    public void ReceiveGameState_ExternalDiscard_TileMovesTilePoolToDiscard(int ix)
    {
        Vars vars = MakeVariablesForTest();

        vars.testPrivateRackCounts[3] -= 1;
        vars.testDiscard.Add(94);

        ReceiveGameState(vars);
        AssertAssembly(vars);
    }

    [Test]
    public void ReceiveGameState_ExternalDiscard_ExternalPrivateRackCountUpdates() { throw new NotImplementedException(); }

    [Test]
    public void ReceiveGameState_LocalNextTurn_TileMovesTilePoolToPrivateRack() { throw new NotImplementedException(); }

    [Test]
    public void ReceiveGameState_ExternalNextTurn_ExternalPrivateRackCountUpdates() { throw new NotImplementedException(); }

    [Test]
    public void ReceiveGameState_LocalCall_TileMovesDiscardToLocalDisplayRack() { throw new NotImplementedException(); }

    [Test]
    public void ReceiveGameState_ExternalCall_TileMovesDiscardToExternalDisplayRack() { throw new NotImplementedException(); }

    [Test]
    public void ReceiveGameState_LocalExpose_TileMovesPrivateRackToLocalDisplayRack() { throw new NotImplementedException(); }

    [Test]
    public void ReceiveGameState_ExternalExpose_TileMovesTilePoolToExternalDisplayRack() { throw new NotImplementedException(); }

    [Test]
    public void ReceiveGameState_ExternalExpose_ExternalPrivateRackCountUpdates() { throw new NotImplementedException(); }

    [Test]
    public void ReceiveGameState_LocalNeverMind_TileMovesDisplayRackToDiscard() { throw new NotImplementedException(); }

    [Test]
    public void ReceiveGameState_ExternalNeverMind_TileMovesDisplayRackToDiscard() { throw new NotImplementedException(); }

    Vars MakeVariablesForTest()
    {
        Vars vars = new();

        List<Tile> tiles = Tile.GenerateTiles();
        ClassReferences refs = new();
        new FakeMonoWrapper(refs);
        new CharlestonClient(refs);
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
        int wallCount = GetWallCount;
        int[] discard = GetDiscard.ToArray();
        int[] privateRack = GetPrivateRack.ToArray();
        int[] privateRackCounts = GetPrivateRackCounts;
        List<int[]> displayRacks = GetDisplayRacks.Select(item => item.ToArray()).ToList();

        tileTracker.ReceiveGameState(wallCount, discard
        , privateRack, privateRackCounts, displayRacks[0]
        , displayRacks[1], displayRacks[2], displayRacks[3]);

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
