using System.Collections.Generic;
using NUnit.Framework;
using System.Linq;

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
        IReadOnlyList<int> newAiRack = vars.tileTracker.PrivateRacks[3];
        IReadOnlyList<int> newDiscard = vars.tileTracker.Discard;
        List<int> aiRackDiff = oldAiRack.Except(newAiRack).ToList();

        Assert.AreEqual(13, newAiRack.Count);
        CollectionAssert.IsSubsetOf(newAiRack, oldAiRack);
        Assert.AreEqual(11, newDiscard.Count);
        CollectionAssert.IsSupersetOf(newDiscard, aiRackDiff);
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
            IReadOnlyList<int> newRack = vars.tileTracker.PrivateRacks[player];

            CollectionAssert.AreEqual(oldRack, newRack);
        }
    }

    // Discard
    [TestCase(3)] // another player's rack
    [TestCase(105)] // on the wall
    [TestCase(200)] // on display rack
    public void Discard_TileNotOnActivePlayersPrivateRack_FailAssertion(int discardTile)
    {
        Vars vars = MakeVariablesForTest(activePlayer: 2);
        if (discardTile == 200)
        {
            vars.tileTracker.MoveTile(200, vars.tileTracker.DisplayRackLocations[2]);
        }

        UnityEngine.TestTools.LogAssert
            .Expect(UnityEngine.LogType.Assert, "discardTile is not on active player's rack.");

        vars.turnManager.Discard(discardTile);
    }

    [Test]
    public void Discard_TileNotOnExposePlayersPrivateRack_FailAssertion()
    {
        Vars vars = MakeVariablesForTest(activePlayer: 2);
        vars.fakeFusionManager.ExposingPlayer = 1;
        int discardTile = 130;

        UnityEngine.TestTools.LogAssert
            .Expect(UnityEngine.LogType.Assert, "discardTile is not on exposing player's rack during expose.");

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
        IReadOnlyList<int> actualDiscard = vars.tileTracker.Discard;
        List<int> expectedRack = GetTestRacks[2];
        expectedRack.Remove(41);
        IReadOnlyList<int> actualRack = vars.tileTracker.ActivePrivateRack;

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
        vars.tileTracker.MoveTile(147, vars.tileTracker.ActivePrivateRackLoc);

        UnityEngine.TestTools.LogAssert.Expect(UnityEngine.LogType.Log, "Discarding joker - no buttons.");

        vars.turnManager.Discard(147);
    }

    // TileCallingMonitor, before timer expires
    [Test]
    public void TileCallingMonitor_BeforeTimerExpires_NothingHappens()
    {
        Vars vars = MakeVariablesForTest();
        vars.fakeFusion.CreateTimer();

        vars.turnManager.TileCallingMonitor();

        Assert.AreEqual(TileLoc.Discard, vars.tileTracker.TileLocations[89]);
    }

    // TileCallingMonitor, time expired and no callers or waiters
    [Test]
    public void TileCallingMonitor_NoCallersOrWaiters_NextTurn()
    {
        Vars vars = MakeVariablesForTest(activePlayer: 0);
        vars.fakeFusion.CreateTimer();
        vars.fakeFusion.IsTimerExpired = true;
        int discardTile = vars.tileTracker.Discard.Last();
        int nextTile = vars.tileTracker.Wall.Last();

        vars.turnManager.TileCallingMonitor();

        Assert.AreEqual(TileLoc.Discard, vars.tileTracker.TileLocations[discardTile]);
        Assert.AreEqual(TileLoc.PrivateRack1, vars.tileTracker.TileLocations[nextTile]);
    }

    // TileCallingMonitor, callers before time expired
    [Test]
    public void TileCallingMonitor_CallersBeforeTimeExpired_NothingHappens()
    {
        Vars vars = MakeVariablesForTest();
        vars.fakeFusion.CreateTimer();
        SubmitInputAndCallTileCallingMonitor(vars, 0, Buttons.call);

        List<int> expectedDiscard = GetTestDiscard; // fail if the most recent tile has been removed from discard
        List<int> actualDiscard = vars.tileTracker.Discard.ToList();

        CollectionAssert.AreEqual(expectedDiscard, actualDiscard);
    }

    // TileCallingMonitor, time expired and there are callers
    // assumes test above passes
    [TestCase(0)] // host
    [TestCase(1)] // not host
    public void TileCallingMonitor_OneCaller_TileGoesToCaller(int player)
    {
        Vars vars = MakeVariablesForTest();
        vars.fakeFusion.CreateTimer();

        CallingPeriodWorkflowShortcut(vars, new() { player }, new() { Buttons.call });

        List<int> expectedDiscard = GetTestDiscard.GetRange(0, 9);
        List<int> actualDiscard = vars.tileTracker.Discard.ToList();
        List<int> expectedDisplayRack = new() { GetTestDiscard.Last() };
        List<int> actualDisplayRack = vars.tileTracker.DisplayRacks[player].ToList();

        CollectionAssert.AreEqual(expectedDiscard, actualDiscard);
        CollectionAssert.AreEqual(expectedDisplayRack, actualDisplayRack);
    }

    [TestCase(0, 1, 2, 1)]
    [TestCase(0, 3, 2, 2)]
    [TestCase(2, 1, 3, 3)]
    [TestCase(1, 2, 0, 2)]
    [TestCase(3, 0, 2, 0)]
    [TestCase(3, 2, 1, 1)]
    public void TileCallingMonitor_TwoCallers_TileGoesToCorrectCaller(int discardPlayer, int firstCaller, int secondCaller, int correctCaller)
    {
        Vars vars = MakeVariablesForTest();
        vars.fakeFusionManager.ActivePlayer = discardPlayer;

        CallingPeriodWorkflowShortcut(vars, new() { firstCaller, secondCaller }, new() { Buttons.call, Buttons.call });

        TileLoc expectedLocation = vars.tileTracker.DisplayRackLocations[correctCaller];
        TileLoc actualLocation = vars.tileTracker.TileLocations[89].curLoc;

        Assert.AreEqual(expectedLocation, actualLocation);
    }

    //[Test]
    // TODO: Implement AI calling
    public void TileCallingMonitor_AICaller_AIExposes()
    {
        Vars vars = MakeVariablesForTest();
        vars.fakeFusionManager.ActivePlayer = 2;

        CallingPeriodWorkflowShortcut(vars, new() { 3 }, new() { Buttons.call });

        List<int> aiPrivateRack = vars.tileTracker.PrivateRacks[3].ToList();
        List<int> discard = vars.tileTracker.Discard.ToList();

        Assert.Less(aiPrivateRack.Count, 13);
        Assert.AreEqual(discard.Count, 11);
    }

    // TileCallingMonitor, timer expired and there are waiters
    [Test]
    public void TileCallingMonitor_OneWaiter_ServerWaits()
    {
        Vars vars = MakeVariablesForTest();

        CallingPeriodWorkflowShortcut(vars, new() { 2 }, new() { Buttons.wait });

        Assert.Contains(89, vars.tileTracker.Discard.ToList());
    }

    [Test]
    public void TileCallingMonitor_TwoWaitersOneRescinds_Waits()
    {
        Vars vars = MakeVariablesForTest();
        List<int> players = new() { 2, 3, 2 };
        List<Buttons> actions = new() { Buttons.wait, Buttons.wait, Buttons.nevermind };

        CallingPeriodWorkflowShortcut(vars, players, actions);

        Assert.Contains(89, vars.tileTracker.Discard.ToList());
    }

    [Test]
    public void TileCallingMonitor_WaitersAndCallers_Waits()
    {
        Vars vars = MakeVariablesForTest();
        List<int> players = new() { 2, 3 };
        List<Buttons> actions = new() { Buttons.call, Buttons.wait };

        CallingPeriodWorkflowShortcut(vars, players, actions);

        Assert.Contains(89, vars.tileTracker.Discard.ToList());
    }

    // TileCallingMonitor, back to callers after caller rescinds
    [Test]
    public void TileCallingMonitor_CancelExposeAndNoMoreCallers_NextTurn()
    {
        Vars vars = MakeVariablesForTest(activePlayer: 1);
        List<int> players = new() { 3 };
        List<Buttons> actions = new() { Buttons.call };


        CallingPeriodWorkflowShortcut(vars, players, actions);
        SubmitInputAndCallTileCallingMonitor(vars, 3, Buttons.nevermind);

        UnityEngine.Debug.Log($"Racks: {vars.tileTracker.PrivateRacksToString()}");
        UnityEngine.Debug.Log($"Active Player: {vars.fakeFusionManager.ActivePlayer}");
        Assert.AreEqual(TileLoc.Discard, vars.tileTracker.TileLocations[89]);   // the tile ends up in Discard
        Assert.AreEqual(14, vars.tileTracker.PrivateRacks[2].Count);            // play continues for the next player
        // FIXME: not making it to any debug statements in NeverMind or NextTurn
    }

    [Test]
    public void TileCallingMonitor_CallersAfterCanceledExpose_TileGoesToCaller()
    {
        Vars vars = MakeVariablesForTest(activePlayer: 1);
        List<int> players = new() { 3, 0 };
        List<Buttons> actions = new() { Buttons.call, Buttons.call };

        CallingPeriodWorkflowShortcut(vars, players, actions);
        SubmitInputAndCallTileCallingMonitor(vars, 3, Buttons.nevermind);

        Assert.AreEqual(TileLoc.DisplayRack0, vars.tileTracker.TileLocations[89]);  // the tile ends up on player 0's rack
    }

    [Test]
    public void TileCallingMonitor_NoCallersOrWaitersDuringSinglePlayer_TurnsGoAroundToFirstPlayer()
    {
        Vars vars = MakeVariablesForTest(activePlayer: 3, aiPlayers: 3);

        vars.turnManager.Discard(vars.tileTracker.PrivateRacks[3].Last());
        vars.fakeFusion.IsTimerExpired = true;
        vars.turnManager.TileCallingMonitor(); // should go to player 1's turn
        vars.fakeFusion.IsTimerExpired = true;
        vars.turnManager.TileCallingMonitor(); // should go to player 2's turn
        vars.fakeFusion.IsTimerExpired = true;
        vars.turnManager.TileCallingMonitor(); // should go to player 3's turn
        vars.fakeFusion.IsTimerExpired = true;
        vars.turnManager.TileCallingMonitor(); // should go back around to player 0\

        for (int i = 1; i < 4; i++) // assert ais have 13 tiles each
        {
            Assert.AreEqual(13, vars.tileTracker.PrivateRacks[i].Count);
        }
        Assert.AreEqual(14, vars.tileTracker.ActivePrivateRack.Count);
    }

    [Test]
    public void Discard_ExposeTurn_NextTurnStartsAfterCallingPeriod()
    {
        Vars vars = MakeVariablesForTest();
        vars.fakeFusionManager.ActivePlayer = 3;
        int exposePlayer = 1;                           // next player after this should be 2, not 0

        vars.turnManager.Discard(vars.tileTracker.PrivateRacks[3].Last());  // discard
        vars.fakeFusionManager.InputDict[exposePlayer].input = Buttons.call;
        vars.turnManager.TileCallingMonitor();
        vars.fakeFusionManager.InputDict[exposePlayer].input = Buttons.none;
        vars.fakeFusion.IsTimerExpired = true;
        vars.turnManager.TileCallingMonitor();          // player three exposes
        // skipping actual exposing steps for now - this might cause errors later
        vars.turnManager.Discard(vars.tileTracker.PrivateRacks[exposePlayer].Last());
        vars.turnManager.TileCallingMonitor();
        vars.fakeFusion.IsTimerExpired = true;
        vars.turnManager.TileCallingMonitor();          // should go to next player now

        Assert.AreEqual(14, vars.tileTracker.PrivateRacks[2].Count);  // next player will have 14 tiles, should be player 2
    }

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
            IsServer = true,
            InputDict = new() {
                {0, new()},
                {1, new()},
                {2, new()},
                {3, new()}
            }
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

    void CallingPeriodWorkflowShortcut(Vars vars, List<int> players, List<Buttons> buttons)
    {
        vars.fakeFusion.CreateTimer();
        for (int i = 0; i < players.Count; i++)
        {
            SubmitInputAndCallTileCallingMonitor(vars, players[i], buttons[i]);
        }
        vars.fakeFusion.IsTimerExpired = true;
        vars.turnManager.TileCallingMonitor();
    }

    void SubmitInputAndCallTileCallingMonitor(Vars vars, int playerId, Buttons button)
    {
        vars.fakeFusionManager.InputDict[playerId].input = button;
        vars.turnManager.TileCallingMonitor();
        vars.fakeFusionManager.InputDict[playerId].input = Buttons.none;
    }

    struct Vars
    {
        public TurnManagerServer turnManager;
        public FakeFusionManager fakeFusionManager;
        public FakeFusionWrapper fakeFusion;
        public TileTrackerServer tileTracker;
    }
}


