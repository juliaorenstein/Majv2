using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

public class CharlestonServerTests
{
    [TestCase(0, new int[] { 3 })]
    [TestCase(0, new int[] { 2, 3 })]
    [TestCase(1, new int[] { 2, 3 })]
    public void PassDriver_AiPass_WhenCalledWithAis_PassListUpdatedForAis(int sourcePlayerId, int[] aiPlayers)
    {
        // FIXME: this is failing because TileList isn't populated for unit tests.
        // ARRANGE
        var (CHost, _, fusionManager, _, _, tileTracker) = CreateTestVariables();
        fusionManager.AiPlayers = aiPlayers.ToList();

        // ACT
        CHost.PassDriver(sourcePlayerId,
            tileTracker.PrivateRacks[sourcePlayerId].GetRange(0, 3).ToArray());

        // ASSERT
        foreach (int aiPlayerId in aiPlayers)
        {
            Assert.True(CHost.AiPassed);
            Assert.True(CHost.PassList[aiPlayerId].Count == 3);
        }
    }

    [Test]
    public void PassDriver_AiPass_WhenCalledWithoutAis_AiPassedTrueAnyway()
    {
        // ARRANGE
        var (CHost, _, _, _, _, tileTracker) = CreateTestVariables();

        // ACT
        CHost.PassDriver(0,
            tileTracker.PrivateRacks[0].GetRange(0, 3).ToArray());

        // ASSERT
        Assert.True(CHost.AiPassed);
    }

    [TestCase(new int[] { 0 }, new int[] { 3 })]
    [TestCase(new int[] { 2 }, new int[] { 3 })]
    [TestCase(new int[] { 0, 2 }, new int[] { 3 })]
    [TestCase(new int[] { 1 }, new int[] { 2, 3 })]
    public void PassDriver_NotEverybodyReady_DontPass(int[] playersPassing, int[] aiPlayers)
    {
        // ARRANGE
        var (CHost, Refs, fusionManager, fusionWrapper, CFusion, tileTracker) = CreateTestVariables();

        // ACT
        foreach (int playerId in playersPassing)
        {
            CHost.PassDriver(playerId, TestPasses()[playerId].ToArray());
        }

        // ASSERT
        Assert.True(CHost.PassList.Any(pass => pass.Count > 0));
        Assert.True(CHost.RecList.All(rec => rec.Count == 0));
        Assert.True(Refs.CFusion.Counter == 0);
    }

    [TestCase(new int[] { 0 }, new int[] { 1, 2, 3 })]
    [TestCase(new int[] { 0, 1 }, new int[] { 2, 3 })]
    [TestCase(new int[] { 3, 1, 2, 0 }, new int[] { })]
    public void PassDriver_EverybodyReady_Pass(int[] playersPassing, int[] aiPlayers)
    {
        // ARRANGE
        var (CHost, Refs, fusionManager, fusionWrapper, CFusion, tileTracker) = CreateTestVariables();
        fusionManager.AiPlayers = aiPlayers.ToList();

        // ACT
        foreach (int playerId in playersPassing)
        {
            CHost.PassDriver(playerId, TestPasses()[playerId].ToArray());
        }

        // ASSERT
        Assert.True(CHost.PassList.All(pass => pass.Count == 0));
        Assert.True(CHost.RecList.All(rec => rec.Count == 0));
        Assert.True(Refs.CFusion.Counter == 1);
    }

    [TestCase(0, 1)]
    [TestCase(1, 2)]
    [TestCase(2, 3)]
    [TestCase(3, 3)]
    [TestCase(4, 2)]
    [TestCase(5, 1)]
    public void PassDriver_SimplePasses_RacksAreUpdated(int counter, int shift)
    {
        // ARRANGE
        var (CHost, _, _, _, CFusion, tileTracker) = CreateTestVariables();
        CFusion.Counter = counter;

        // ACT
        for (int playerId = 0; playerId < 4; playerId++)
        {
            CHost.PassDriver(playerId, TestPasses()[playerId].ToArray());
        }
        // shift indicates how many places the passes should move based on the counter.
        // i.e. a right pass is a shift of 1, over is shift of 2, left is shift of 3.
        List<List<int>> expectedSubsets = new() {
            TestPasses()[shift],
            TestPasses()[(shift + 1) % 4],
            TestPasses()[(shift + 2) % 4],
            TestPasses()[(shift + 3) % 4],
        };
        List<List<int>> actualRacks = tileTracker.PrivateRacks;

        // ASSERT
        for (int playerId = 0; playerId < 4; playerId++)
        {
            CollectionAssert.IsSupersetOf(actualRacks[playerId], expectedSubsets[playerId]);
            Assert.True(actualRacks[playerId].Count() == 10);
        }
    }

    // FIXME: tests didn't indicate when steals were actually working incorrectlly
    [TestCase(
        new int[] { 0, 1, 2 }, new int[] { 20, 21, 22 }, new int[] { 40, 41, 42 }, new int[] { 60, 61 },
        new int[] { 20, 21, 22 }, new int[] { 40, 41, 42 }, new int[] { 60, 61, 2 }, new int[] { 0, 1 })]
    [TestCase(
        new int[] { 0, 1, 2 }, new int[] { 20, 21 }, new int[] { 40, 41, 42 }, new int[] { 60, 61, 62 },
        new int[] { 20, 21, 42 }, new int[] { 40, 41 }, new int[] { 60, 61, 62 }, new int[] { 0, 1, 2 })]
    [TestCase(
        new int[] { 0, 1 }, new int[] { 20, 21, 22 }, new int[] { 40, 41, 42 }, new int[] { 60, 61 },
        new int[] { 20, 21 }, new int[] { 40, 41, 42 }, new int[] { 60, 61, 22 }, new int[] { 0, 1 })]
    [TestCase(
        new int[] { 0, 1 }, new int[] { 20, 21 }, new int[] { 40, 41 }, new int[] { 60, 61 },
        new int[] { 20, 21 }, new int[] { 40, 41 }, new int[] { 60, 61 }, new int[] { 0, 1 })]
    [TestCase(
        new int[] { 0, 1 }, new int[] { 20, 21, 22 }, new int[] { 40, 41 }, new int[] { 60, 61, 62 },
        new int[] { 20, 21 }, new int[] { 40, 41, 62 }, new int[] { 60, 61 }, new int[] { 0, 1, 22 })]
    [TestCase(
        new int[] { 0 }, new int[] { 20 }, new int[] { 40 }, new int[] { 60 },
        new int[] { 20 }, new int[] { 40 }, new int[] { 60 }, new int[] { 0 })]
    [TestCase(
        new int[] { 0 }, new int[] { 20, 21, 22 }, new int[] { 40 }, new int[] { 60 },
        new int[] { 20 }, new int[] { 40, 21, 22 }, new int[] { 60 }, new int[] { 0 })]

    public void PassDriver_StealRight_RacksAreUpdated(
        int[] pass0, int[] pass1, int[] pass2, int[] pass3,
        int[] rec0, int[] rec1, int[] rec2, int[] rec3)
    {
        // ARRANGE
        var (CHost, _, _, _, _, tileTracker) = CreateTestVariables();
        List<int[]> testPasses = new() { pass0, pass1, pass2, pass3 };

        // ACT
        for (int playerId = 0; playerId < 4; playerId++)
        {
            CHost.PassDriver(playerId, testPasses[playerId].ToArray());
        }
        // shift indicates how many places the passes should move based on the counter.
        // i.e. a right pass is a shift of 1, over is shift of 2, left is shift of 3.
        List<List<int>> expectedSubsets = new()
        { rec0.ToList(), rec1.ToList(), rec2.ToList(), rec3.ToList() };
        List<List<int>> actualRacks = tileTracker.PrivateRacks;

        // ASSERT
        for (int playerId = 0; playerId < 4; playerId++)
        {
            CollectionAssert.IsSupersetOf(actualRacks[playerId], expectedSubsets[playerId]);
            Assert.True(actualRacks[playerId].Count() == 10);
        }
    }

    public void PassDriver_AfterPass_VariablesAreReset()
    {
        // ARRANGE
        var (CHost, _, _, _, _, _) = CreateTestVariables();

        // ACT
        for (int playerId = 0; playerId < 4; playerId++)
        {
            CHost.PassDriver(playerId, TestPasses()[playerId].ToArray());
        }

        // ASSERT
        Assert.False(CHost.AiPassed);
        for (int i = 0; i < 4; i++)
        {
            CollectionAssert.IsEmpty(CHost.PassList[i]);
            CollectionAssert.IsEmpty(CHost.RecList[i]);
        }
    }



    // factory method to set up all the useful variables for the unit tests
    (CharlestonServer
        , ClassReferences
        , FakeFusionManager
        , FakeFusionWrapper
        , FakeCharlestonFusion
        , TileTrackerServer)
        CreateTestVariables()
    {
        ClassReferences refs = new();
        FakeFusionManager fusionManager = new(refs);
        FakeFusionWrapper fusionWrapper = new(refs);
        FakeCharlestonFusion cFusion = new(refs);
        TileTrackerServer tileTracker = new(refs);
        for (int i = 0; i < 4; i++)
        {
            foreach (int tileId in TestRacks()[i])
            {
                tileTracker.MoveTile(tileId, tileTracker.PrivateRacks[i]);
            }
        }
        CharlestonServer cHost = new(refs);
        Tile.TileList = Tile.GenerateTiles();

        return (cHost, refs, fusionManager, fusionWrapper, cFusion, tileTracker);
    }

    // shortening to ten tiles per rack to allow output of unit tests to be more readable
    List<int> Rack0 { get => new(Enumerable.Range(0, 10).ToList()); }
    List<int> Rack1 { get => new(Enumerable.Range(20, 10).ToList()); }
    List<int> Rack2 { get => new(Enumerable.Range(40, 10).ToList()); }
    List<int> Rack3 { get => new(Enumerable.Range(60, 10).ToList()); }

    List<int> Pass0 { get => new(Enumerable.Range(0, 3).ToList()); }
    List<int> Pass1 { get => new(Enumerable.Range(20, 3).ToList()); }
    List<int> Pass2 { get => new(Enumerable.Range(40, 3).ToList()); }
    List<int> Pass3 { get => new(Enumerable.Range(60, 3).ToList()); }

    List<List<int>> TestRacks() => new() { Rack0, Rack1, Rack2, Rack3 };
    List<List<int>> TestPasses() => new() { Pass0, Pass1, Pass2, Pass3 };
}
