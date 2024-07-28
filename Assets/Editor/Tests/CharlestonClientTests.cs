using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;

public class CharlestonClientTests
{
    // CheckReadyToPass
    [Test]
    public void CheckReadyToPass_ThreeValidTiles_ReturnsTrue()
    {
        // ARRANGE
        ClassReferences refs = new()
        {
            Mono = new FakeMonoWrapper(),
            CFusion = new FakeCharlestonFusion()
        };

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
        ClassReferences refs = new()
        {
            Mono = new FakeMonoWrapper(),
            CFusion = new FakeCharlestonFusion()
        };

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
        ClassReferences refs = new()
        {
            Mono = new FakeMonoWrapper(),
            CFusion = new FakeCharlestonFusion() { Counter = 2 },
        };

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
        FakeMonoWrapper mono = new();
        ClassReferences refs = new()
        {
            Mono = mono,
            CFusion = new FakeCharlestonFusion()
        };

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
        FakeMonoWrapper mono = new();
        ClassReferences refs = new()
        {
            Mono = mono,
            CFusion = new FakeCharlestonFusion()
        };

        CharlestonClient CClient = new(refs)
        {
            ClientPassArr = new int[3] {1, 2, 3}
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
        ClassReferences Refs = new()
        {
            Mono = new FakeMonoWrapper(),
            CFusion = new FakeCharlestonFusion()
        };
        GameManagerClient gManagerClient = new(Refs)
        {
            PrivateRack = new() { 1, 2, 3, 4, 5 }
        };
        ReceiveGameState ReceiveGame = new(Refs);

        // ACT
        ReceiveGame.ReceiveRackUpdate(new int[5] { 4, 5, 6, 7, 8 });
        List<int> expected = new() { 4, 5, 6, 7, 8 };
        List<int> actual = gManagerClient.PrivateRack;

        // ASSERT
        CollectionAssert.AreEqual(expected, actual);
    }

    [TestCase(0)]
    [TestCase(5)]
    public void UpdateButton_RightPass_ButtonTextUpdated(int counter)
    {
        // ARRANGE
        FakeMonoWrapper mono = new();
        ClassReferences refs = new()
        {
            Mono = mono,
            CFusion = new FakeCharlestonFusion()
        };
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
        FakeMonoWrapper mono = new();
        ClassReferences refs = new()
        {
            Mono = mono,
            CFusion = new FakeCharlestonFusion()
        };
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
        FakeMonoWrapper mono = new();
        ClassReferences refs = new()
        {
            Mono = mono,
            CFusion = new FakeCharlestonFusion()
        };
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
        FakeMonoWrapper mono = new();
        ClassReferences refs = new()
        {
            Mono = mono,
            CFusion = new FakeCharlestonFusion()
        };
        refs.GManager = new(refs);
        refs.TManager = new(refs);
        CharlestonClient CClient = new(refs);
        mono.SetActive(MonoObject.CharlestonBox, true);

        // ACT
        CClient.UpdateButton(counter);

        // ASSERT
        CollectionAssert.DoesNotContain(mono.ActiveList, MonoObject.CharlestonBox);
    }
}
