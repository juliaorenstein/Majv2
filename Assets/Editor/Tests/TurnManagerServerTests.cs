using System.Collections.Generic;
using NUnit.Framework;
using System.Linq;
using System;

public class TurnManagerTests
{
    // StartGamePlay

    public void StartGamePlay_NotServer_FailAssertion()
    {
        Vars vars = MakeVariablesForTest();
        vars.fakeFusionManager.IsServer = false;

        UnityEngine.TestTools.LogAssert
            .Expect(UnityEngine.LogType.Assert, "Reached TurnManagerServer on client.");

        vars.turnManager.StartGamePlay();
    }

    [TestCase(0)] // local player, server
    [TestCase(1)] // non local player
    [TestCase(2)] // non local player
    [TestCase(3)] // ai
    public void StartGamePlay_WhenCalled_ActivePlayerSetToDealer(int dealer)
    {
        Vars vars = MakeVariablesForTest(dealer: dealer);

        vars.turnManager.StartGamePlay();

        Assert.AreEqual(vars.fakeFusionManager.ActivePlayer, vars.fakeFusionManager.Dealer);
    }

    [Test]
    public void StartGamePlay_WhenDealerIsAI_AIDiscards()
    {
        Vars vars = MakeVariablesForTest();

        vars.turnManager.StartGamePlay();

        List<int> oldAiRack = GetTestRacks[3];
        List<int> newAiRack = vars.tileTracker.PrivateRacks[3];
        List<int> newDiscard = vars.tileTracker.Discard;
        List<int> aiRackDiff = oldAiRack.Except(newAiRack).ToList();

        Assert.True(newAiRack.Count == 13);
        CollectionAssert.IsSubsetOf(newAiRack, oldAiRack);
        Assert.True(newDiscard.Count == 1);
        CollectionAssert.AreEqual(newDiscard, aiRackDiff);
    }

    [TestCase(0)] // host
    [TestCase(1)] // not host
    public void StartGamePlay_WhenDealerIsClient_RackDoesntChange(int dealer)
    {
        Vars vars = MakeVariablesForTest(dealer: dealer);

        vars.turnManager.StartGamePlay();

        for (int player = 0; player < 4; player++)
        {
            List<int> oldRack = GetTestRacks[player];
            List<int> newRack = vars.tileTracker.PrivateRacks[player];

            CollectionAssert.AreEqual(oldRack, newRack);
        }
    }

    // Discard
    [TestCase(3)] // another player's rack
    [TestCase(105)] // on the wall
    [TestCase(349)] // invalid tile
    [TestCase(200)] // on display rack
    public void Discard_TileNotOnActivePlayersPrivateRack_FailAssertion(int discardTile)
    {
        Vars vars = MakeVariablesForTest(activePlayer: 2);
        if (discardTile == 200)
        {
            vars.tileTracker.DisplayRacks[2].Add(200);
        }

        UnityEngine.TestTools.LogAssert
            .Expect(UnityEngine.LogType.Assert, "Discard tile not on active player's rack.");

        vars.turnManager.Discard(discardTile);
    }

    [Test]
    public void Discard_WhenCalled_TileGoesToDiscard()
    {
        Vars vars = MakeVariablesForTest(activePlayer: 2);
        int discardTile = 41;

        vars.turnManager.Discard(discardTile);

        List<int> expectedDiscard = GetTestDiscard;
        expectedDiscard.Add(41);
        List<int> actualDiscard = vars.tileTracker.Discard;
        List<int> expectedRack = GetTestRacks[2];
        expectedRack.Remove(41);
        List<int> actualRack = vars.tileTracker.ActivePrivateRack;

        CollectionAssert.AreEqual(expectedDiscard, actualDiscard);
        CollectionAssert.AreEqual(expectedRack, actualRack);
    }

    [Test]
    public void Discard_WhenCalled_StartTimer()
    {
        Vars vars = MakeVariablesForTest(activePlayer: 0);

        vars.turnManager.Discard(0);

        Assert.True(vars.fakeFusion.IsTimerRunning);
    }

    [Test]
    public void Discard_WhenCalledNotJoker_ShowButtons()
    {
        Vars vars = MakeVariablesForTest(activePlayer: 0);

        UnityEngine.TestTools.LogAssert.Expect(UnityEngine.LogType.Log, $"RPC_S2A_ShowButtons(0)");

        vars.turnManager.Discard(0);
    }

    [Test]
    public void Discard_WhenCalledWithJoker_DontShowButtons()
    {
        Vars vars = MakeVariablesForTest(activePlayer: 0);
        vars.tileTracker.ActivePrivateRack.Add(147);

        UnityEngine.TestTools.LogAssert.Expect(UnityEngine.LogType.Log, "Discarding joker - no buttons.");

        vars.turnManager.Discard(147);
    }

    // TileCallingMonitor, before timer expires
    [Test]
    public void TileCallingMonitor_BeforeTimerExpires_NothingHappens()
    {
        // not yet sure the best way to implement this
        throw new NotImplementedException();
    }

    // TileCallingMonitor, time expired and no callers or waiters
    [Test]
    public void TileCallingMonitor_NoCallersOrWaiters_TileGoesToNextPlayer()
    {
        Vars vars = MakeVariablesForTest(activePlayer: 0);
        vars.fakeFusion.IsTimerExpired = true;

        vars.turnManager.TileCallingMonitor();

        List<int> expectedWall = Enumerable.Range(100, 49).ToList();
        List<int> actualWall = vars.tileTracker.Wall;
        List<List<int>> expectedRacks = GetTestRacks;
        expectedRacks[1].Add(149);
        List<List<int>> actualRacks = vars.tileTracker.PrivateRacks;

        CollectionAssert.AreEqual(expectedWall, actualWall);
        for (int rackId = 0; rackId < 4; rackId++)
        {
            CollectionAssert.AreEqual(expectedRacks[rackId], actualRacks[rackId]);
        }
    }

    // TileCallingMonitor, time expired and there are callers
    [Test]
    public void TileCallingMonitor_OneCaller_TileGoesToCaller() { throw new NotImplementedException(); }

    [Test]
    public void TileCallingMonitor_TwoCallers_TileGoesToCorrectCaller() { throw new NotImplementedException(); }

    [Test]
    public void TileCallingMonitor_AICaller_AIExposes() { throw new NotImplementedException(); }

    // TileCallingMonitor, timer expired and there are waiters
    [Test]
    public void TileCallingMonitor_OneWaiter_Waits() { throw new NotImplementedException(); }

    [Test]
    public void TileCallingMonitor_TwoWaitersOneRescinds_Waits() { throw new NotImplementedException(); }

    [Test]
    public void TileCallingMonitor_WaitersAndCallers_Waits() { throw new NotImplementedException(); }

    // TileCallingMonitor, back to callers after caller rescinds
    [Test]
    public void TileCallingMonitor_NoCallersAfterCanceledExpose_NextTurn() { throw new NotImplementedException(); }

    [Test]
    public void TileCallingMonitor_CallersAfterCanceledExpose_TileGoesToCaller() { throw new NotImplementedException(); }

    [Test]
    public void TK_ExposeTurn_NextTurnStarts() { throw new NotImplementedException(); }


    // The following five tests are out of scope for TileLocomotion, but when TurnManager is tested, those tests should start from this class.
    // public void OnPointerClick_RackTileWhenDiscardIsValid_TileIsDiscarded()

    // public void OnPointerClick_RackTileWhenExposeIsValid_TileIsExposed()

    // public void OnPointerClick_RackTileWhenDiscardAndExposePossible_Discarded()

    // public void OnPointerClick_RackTileWhenDiscardAndExposePossible_Expose()

    // public void OnPointerClick_DiscardedTileDuringCalling_TileIsCalled()

    // will test some of these in TurnManager
    /*
    [Test]
    public void OnEndDrag_RackToDiscardValid_Discard() { throw new NotImplementedException(); }

    [Test]
    public void OnEndDrag_RackToDiscardInvalid_TileMovesBack() { throw new NotImplementedException(); }

    [Test]
    public void OnEndDrag_DiscardToDisplayRack_Expose() { throw new NotImplementedException(); }

    [Test]
    public void OnEndDrag_RackToDisplayRackValid_Expose() { throw new NotImplementedException(); }

    [Test]
    public void OnEndDrag_RackToDisplayRackInvalid_TileMovesBack() { throw new NotImplementedException(); }
    */

    // automatically sets up a test where:
    // - dealer is player 3
    // - player 3 is ai
    // - local player is 0
    // - racks are populated with tiles from 0, 20, 40, and 60 with 13 each
    // - - except dealer, which has 14
    // - wall is populated with tiles 100 - 149
    // you can pass in those parameters to edit
    // AIs will be populated from 3 downward
    Vars MakeVariablesForTest(int aiPlayers = 1, int dealer = 3, int localPlayer = 0, int activePlayer = -1)
    {
        Vars vars = new();

        List<int> aiPlayerList = new();
        for (int i = 3; i > 3 - aiPlayers; i--)
        {
            aiPlayerList.Insert(0, i);
        };
        ClassReferences refs = new();
        vars.fakeFusionManager = new(refs)
        {
            AiPlayers = aiPlayerList,
            Dealer = dealer,
            LocalPlayer = localPlayer,
            ActivePlayer = activePlayer,
            IsServer = true
        };
        vars.fakeFusion = new(refs);
        vars.tileTracker = new(refs);
        PopulateTileTracker(vars.tileTracker);
        vars.turnManager = new(refs);
        List<List<int>> testRacks = GetTestRacks;
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
}

struct Vars
{
    public TurnManagerServer turnManager;
    public FakeFusionManager fakeFusionManager;
    public FakeFusionWrapper fakeFusion;
    public TileTrackerServer tileTracker;
}
