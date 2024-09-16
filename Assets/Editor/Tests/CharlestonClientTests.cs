using System.Collections.ObjectModel;
using NUnit.Framework;

public class CharlestonClientTests
{
    // CheckReadyToPass
    [Test]
    public void CheckReadyToPass_ThreeValidTiles_ReturnsTrue()
    {
        // ARRANGE
        var (refs, _, _) = CreateVariables();

        CharlestonClient CClient = new(refs)
        {
            ClientPassArr = new int[] { 142, 2, 35 }
        };

        // ACT
        bool result = CClient.CheckReadyToPass();

        // ASSERT
        Assert.True(result);
    }

    [TestCase(94, 38, -1)]
    [TestCase(-1, 38, -1)]
    [TestCase(-1, -1, -1)]
    public void CheckReadyToPass_FewerThanThreeTiles_ReturnsFalse(int tile1, int tile2, int tile3)
    {
        // ARRANGE
        var (refs, _, _) = CreateVariables();

        CharlestonClient CClient = new(refs)
        {
            ClientPassArr = new int[] { tile1, tile2, tile3 }
        };

        // ACT
        bool result = CClient.CheckReadyToPass();

        // ASSERT
        Assert.False(result);
    }

    [TestCase(94, 38, -1)]
    [TestCase(-1, 38, -1)]
    [TestCase(-1, -1, -1)]
    public void CheckReadyToPass_FewerThanThreeTilesOnSteal_ReturnsTrue(int tile1, int tile2, int tile3)
    {
        // ARRANGE
        var (refs, _, _) = CreateVariables(2);

        CharlestonClient CClient = new(refs)
        {
            ClientPassArr = new int[] { tile1, tile2, tile3 },
        };

        // ACT
        bool result = CClient.CheckReadyToPass();

        // ASSERT
        Assert.True(result);
    }


    // INITIATE PASS
    [Test]
    public void InitiatePass_ButtonIsInteractable_ButtonDeactivatedAndTilesMoved()
    {
        // ARRANGE
        var (refs, mono, _) = CreateVariables();

        CharlestonClient CClient = new(refs)
        {
            ClientPassArr = new int[3] { 1, 2, 3 }
        };

        mono.SetButtonInteractable(MonoObject.CharlestonPassButton, true);

        // ACT
        CClient.InitiatePass();

        // ASSERT
        Assert.False(mono.IsButtonInteractable(MonoObject.CharlestonPassButton));
        CollectionAssert.IsNotEmpty(mono.TileLocations);
    }

    [Test]
    public void InitiatePass_ButtonNotInteractable_NothingHappens()
    {
        // ARRANGE
        var (refs, mono, _) = CreateVariables();

        CharlestonClient CClient = new(refs)
        {
            ClientPassArr = new int[3] { 1, 2, 3 }
        };
        mono.SetButtonInteractable(MonoObject.CharlestonPassButton, false);

        // ACT
        CClient.InitiatePass();

        // ASSERT
        Assert.False(mono.IsButtonInteractable(MonoObject.CharlestonPassButton));
        CollectionAssert.IsEmpty(mono.TileLocations);
    }

    // RECEIVE RACK UPDATE
    [Test]
    public void ReceiveRackUpdate_ReceiveNewRack_RackIsUpdated()
    {
        // ARRANGE
        var (refs, mono, _) = CreateVariables();
        TileTrackerClient tileTracker = new(refs)
        {
            LocalPrivateRack = new() { 1, 2, 3, 4, 5 }
        };

        // ACT
        tileTracker.ReceiveRackUpdate(new int[5] { 4, 5, 6, 7, 8 });
        ObservableCollection<int> expected = new() { 4, 5, 6, 7, 8 };
        ObservableCollection<int> actual = tileTracker.LocalPrivateRack;

        // ASSERT
        CollectionAssert.AreEqual(expected, actual);
    }

    [TestCase(0)]
    [TestCase(5)]
    public void UpdateButton_RightPass_ButtonTextUpdated(int counter)
    {
        // ARRANGE
        var (refs, mono, _) = CreateVariables();
        CharlestonClient CClient = new(refs);

        // ACT
        CClient.UpdateButton(counter);

        // ASSERT
        Assert.True(mono.ButtonText == "Pass Right");
    }

    [TestCase(1)]
    [TestCase(4)]
    public void UpdateButton_AcrossPass_ButtonTextUpdated(int counter)
    {
        // ARRANGE
        var (refs, mono, _) = CreateVariables();
        CharlestonClient CClient = new(refs);

        // ACT
        CClient.UpdateButton(counter);

        // ASSERT
        Assert.True(mono.ButtonText == "Pass Over");
    }

    [TestCase(2)]
    [TestCase(3)]
    public void UpdateButton_LeftPass_ButtonTextUpdated(int counter)
    {
        // ARRANGE
        var (refs, mono, _) = CreateVariables();
        CharlestonClient CClient = new(refs);

        // ACT
        CClient.UpdateButton(counter);

        // ASSERT
        Assert.True(mono.ButtonText == "Pass Left");
    }

    public void UpdateButton_Optional_ButtonTextUpdated()
    {
        Assert.Fail();
    }

    [TestCase(-1)]
    [TestCase(7)]
    public void UpdateButton_WhenDone_DeactivateButton(int counter)
    {
        // ARRANGE
        var (refs, mono, _) = CreateVariables();
        FakeFusionManager fusionManager = new(refs);
        TurnManagerClient turnManagerClient = new(refs);
        CharlestonClient CClient = new(refs);
        mono.SetActive(MonoObject.CharlestonBox, true);

        // ACT
        CClient.UpdateButton(counter);

        // ASSERT
        CollectionAssert.DoesNotContain(mono.ActiveList, MonoObject.CharlestonBox);
    }

    (ClassReferences, FakeMonoWrapper, FakeCharlestonFusion) CreateVariables(int counter = 0)
    {
        ClassReferences refs = new();
        return (refs, new(refs), new(refs) { Counter = counter });
    }
}
