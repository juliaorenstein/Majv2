using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;

public class CharlestonClientTests
{
    [TearDown]
    public void Cleanup()
    {
        
    }

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
        Refs.GManagerClient = gManagerClient;

        // ACT
        Refs.ReceiveGame.ReceiveRackUpdate(new int[5] { 4, 5, 6, 7, 8 });
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
        refs.TManager = new(refs);
        CharlestonClient CClient = new(refs);
        mono.SetActive(MonoObject.CharlestonBox, true);

        // ACT
        CClient.UpdateButton(counter);

        // ASSERT
        CollectionAssert.DoesNotContain(mono.ActiveList, MonoObject.CharlestonBox);
    }

    // PASSLOGIC
    /*
    List<List<int>> TestHostPassArr()
    {
        return new()
        {
            new() { 1, 2, 3 },
            new() { 4, 5, 6 },
            new() { 7, 8, 9 },
            new() { 10, 11, 12 }
        };
    }

    
    [Test]
    public void PassLogic_ToTheRightNoStealing_CheckArray()
    {
        // ARRANGE
        CharlestonClient CClient = new();
        CClient.HostPassArr = TestHostPassArr();
        CClient.Counter = 0;

        // ACT
        List<List<int>> result = CClient.PassLogic();
        List<List<int>> expected = new()
        {
            new() { 10, 11, 12 },
            new() { 1, 2, 3 },
            new() { 4, 5, 6 },
            new() { 7, 8, 9 }
        };

        // ASSERT
        CollectionAssert.AreEqual(expected, result);
    }

    [Test]
    public void PassLogic_AcrossNoStealing_CheckArray()
    {
        // ARRANGE
        CharlestonClient CClient = new();
        CClient.HostPassArr = TestHostPassArr();
        CClient.Counter = 0;

        // ACT
        List<List<int>> result = CClient.PassLogic();
        List<List<int>> expected = new()
        {
            new() { 7, 8, 9 },
            new() { 10, 11, 12 },
            new() { 1, 2, 3 },
            new() { 4, 5, 6 },
        };

        // ASSERT
        CollectionAssert.AreEqual(expected, result);
    }

    [Test]
    public void PassLogic_ToTheLeftNoStealing_CheckArray()
    {
        // ARRANGE
        CharlestonClient CClient = new();
        CClient.HostPassArr = TestHostPassArr();
        CClient.Counter = 0;

        // ACT
        List<List<int>> result = CClient.PassLogic();
        List<List<int>> expected = new()
        {
            new() { 4, 5, 6 },
            new() { 7, 8, 9 },
            new() { 10, 11, 12 },
            new() { 1, 2, 3 }
        };

        // ASSERT
        CollectionAssert.AreEqual(expected, result);
    }
    
    [Test]
    public void PassLogic_AcrossWithStealing_CheckArray()
    {

    }

    [Test]
    public void PassLogic_ToTheLeftWithStealing_CheckArray()
    {

    }

    [Test]
    public void PassLogic_OptionalAcross_CheckArray()
    {

    }
    */
}

class FakeMonoWrapper : IMonoWrapper
{
    public void ExposeOtherPlayerTile(int rackId, int tileId)
    {

    }

    public Dictionary<int, MonoObject> TileLocations = new();
    public void MoveTile(int tileId, MonoObject destination)
    {
        TileLocations[tileId] = destination;
    }

    public List<MonoObject> ActiveList = new();
    public void SetActive(MonoObject monoObject, bool value)
    {
        if (value) ActiveList.Add(monoObject);
        else ActiveList.Remove(monoObject);
    }

    public bool IsButtonInteractableValue { get; private set; }
    public bool IsButtonInteractable(MonoObject monoObject)
    {
        return IsButtonInteractableValue;
    }

    public void SetButtonInteractable(MonoObject monoObject, bool value)
    {
        IsButtonInteractableValue = value;
    }

    public string ButtonText;
    public void SetButtonText(MonoObject monoObject, string text)
    {
        ButtonText = text;
    }

    public List<MonoObject> RaycastTargetList = new();
    public void SetRaycastTarget(MonoObject monoObject, bool value)
    {
        if (value) RaycastTargetList.Add(monoObject);
        else RaycastTargetList.Remove(monoObject);
    }

    public List<int> TileRaycastTargetList = new();
    public void SetRaycastTargetOnTile(int tileId, bool value)
    {
        if (value) TileRaycastTargetList.Add(tileId);
        else TileRaycastTargetList.Remove(tileId);
    }

    public void SetTurnIndicatorText(int playerId)
    {

    }

    public void StartNewCoroutine(IEnumerator func)
    {

    }

    public void UnexposeOtherPlayerTile(int rackId, int tileId)
    {

    }

    public IEnumerator WaitForSeconds(int seconds)
    {
        yield return new();
    }
}
