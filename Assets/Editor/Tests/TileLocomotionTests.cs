using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class TileLocomotionTests
{
    // Note: Many client-side actions require logic and verification from the host. I won't test any of those actions in this class, and will probably return to them in TurnManager unit tests. I'll make notes of the test cases I'm avoiding here. The only tests here will be actions that can be verified and completed on the client side.

    // OnPointerClick, Charleston
    [Test]
    public void OnPointerClick_RackTileDuringCharles_TileMovesToCharles()
    {
        var (tileTracker, charles, charlesFusion, tileLoco) = MakeVariablesForCharlestonTest();
        tileLoco.OnPointerClick(true);

        List<int> expectedRack = new(TestRack.ToList().GetRange(1, 4));
        int[] expectedPassArr = new int[] { 0, -1, -1 };
        (List<int> actualRack, int[] actualPassArr) =
            GetActualVarsForCharlestons(tileTracker, charles);

        CollectionAssert.AreEqual(expectedRack, actualRack);
        CollectionAssert.AreEqual(expectedPassArr, actualPassArr);
    }

    [Test]
    public void OnPointerClick_RackTileDuringCharlesNotEmpty_TileMovesToCharles()
    {
        var (tileTracker, charles, charlesFusion, tileLoco) = MakeVariablesForCharlestonTest();
        charles.ClientPassArr[0] = 94;

        tileLoco.OnPointerClick(true);

        List<int> expectedRack = new(TestRack.ToList().GetRange(1, 4));
        int[] expectedPassArr = new int[] { 94, 0, -1 };
        (List<int> actualRack, int[] actualPassArr) =
            GetActualVarsForCharlestons(tileTracker, charles);

        CollectionAssert.AreEqual(expectedRack, actualRack);
        CollectionAssert.AreEqual(expectedPassArr, actualPassArr);
    }

    [Test]
    public void OnPointerClick_RackTileDuringCharlesBoxIsFull_TileReplacesCharles()
    {
        var (tileTracker, charles, charlesFusion, tileLoco) = MakeVariablesForCharlestonTest();
        charles.ClientPassArr = new int[] { 11, 12, 13 };

        tileLoco.OnPointerClick(true);

        List<int> expectedRack = new(TestRack.ToList().GetRange(1, 4)) { 13 };

        int[] expectedPassArr = new int[] { 11, 12, 0 };
        (List<int> actualRack, int[] actualPassArr) =
            GetActualVarsForCharlestons(tileTracker, charles);

        CollectionAssert.AreEqual(expectedRack, actualRack);
        CollectionAssert.AreEqual(expectedPassArr, actualPassArr);
    }

    [Test]
    public void OnPointerClick_CharlesTile_TileMovesToRack()
    {
        var (tileTracker, charles, charlesFusion, tileLoco) = MakeVariablesForCharlestonTest(11);
        charles.ClientPassArr = new int[] { 11, 12, 13 };

        tileLoco.OnPointerClick(true);

        List<int> expectedRack = TestRack;
        expectedRack.Add(11);
        int[] expectedPassArr = new int[] { -1, 12, 13 };
        (List<int> actualRack, int[] actualPassArr) =
            GetActualVarsForCharlestons(tileTracker, charles);

        CollectionAssert.AreEqual(expectedRack, actualRack);
        CollectionAssert.AreEqual(expectedPassArr, actualPassArr);
    }

    [Test]
    public void OnPointerClick_JokerOnRack_DoesNotGoToCharleston()
    {
        int jokerId = 147;
        var (tileTracker, charles, charlesFusion, tileLoco) = MakeVariablesForCharlestonTest(147);
        tileTracker.LocalPrivateRack.Add(jokerId);

        tileLoco.OnPointerClick(true);

        List<int> expectedRack = TestRack;
        expectedRack.Add(jokerId);
        int[] expectedPassArr = new int[] { -1, -1, -1 };
        (List<int> actualRack, int[] actualPassArr) =
            GetActualVarsForCharlestons(tileTracker, charles);

        CollectionAssert.AreEqual(expectedRack, actualRack);
        CollectionAssert.AreEqual(expectedPassArr, actualPassArr);
    }

    // OnPointerClick, Nothing Happens - removed these because I'm going to throw exceptions at the end of all known cases for these, which will find invalid cases for me as I go through tileTrackerplay

    // OnEndDrag, Rack

    [Test]
    public void OnEndDrag_MoveTileRightOnRackRightOfAnotherTile_RackRearranges()
    {
        var (tileTracker, charles, charlesFusion, tileLoco) = MakeVariablesForCharlestonTest();
        List<MonoObject> raycastResults = new() { MonoObject.PrivateRack };
        tileTracker.LocalPrivateRack = TestRack;
        int dropIx = 3;
        bool rightOfTile = true;

        tileLoco.OnEndDrag(raycastResults, dropIx, rightOfTile);
        List<int> expectedRack = new() { 1, 2, 3, 0, 4 };
        List<int> actualRack = tileTracker.LocalPrivateRack;

        CollectionAssert.AreEqual(expectedRack, actualRack);
    }

    [Test]
    public void OnEndDrag_MoveTileRightOnRackLeftOfAnotherTile_RackRearranges()
    {
        var (tileTracker, charles, charlesFusion, tileLoco) = MakeVariablesForCharlestonTest();
        List<MonoObject> raycastResults = new() { MonoObject.PrivateRack };
        tileTracker.LocalPrivateRack = TestRack;
        int dropIx = 3;
        bool rightOfTile = false;

        tileLoco.OnEndDrag(raycastResults, dropIx, rightOfTile);
        List<int> expectedRack = new() { 1, 2, 0, 3, 4 };
        List<int> actualRack = tileTracker.LocalPrivateRack;

        CollectionAssert.AreEqual(expectedRack, actualRack);
    }

    [Test]
    public void OnEndDrag_MoveTileLeftOnRackRightOfAnotherTile_RackRearranges()
    {
        var (tileTracker, charles, charlesFusion, tileLoco) = MakeVariablesForCharlestonTest(4);
        List<MonoObject> raycastResults = new() { MonoObject.PrivateRack };
        tileTracker.LocalPrivateRack = TestRack;
        int dropIx = 2;
        bool rightOfTile = true;

        tileLoco.OnEndDrag(raycastResults, dropIx, rightOfTile);
        List<int> expectedRack = new() { 0, 1, 2, 4, 3 };
        List<int> actualRack = tileTracker.LocalPrivateRack;

        CollectionAssert.AreEqual(expectedRack, actualRack);
    }

    [Test]
    public void OnEndDrag_MoveTileLeftOnRackLeftOfAnotherTile_RackRearranges()
    {
        var (tileTracker, charles, charlesFusion, tileLoco) = MakeVariablesForCharlestonTest(4);
        List<MonoObject> raycastResults = new() { MonoObject.PrivateRack };
        tileTracker.LocalPrivateRack = TestRack;
        int dropIx = 2;
        bool rightOfTile = false;

        tileLoco.OnEndDrag(raycastResults, dropIx, rightOfTile);
        List<int> expectedRack = new() { 0, 1, 4, 2, 3 };
        List<int> actualRack = tileTracker.LocalPrivateRack;

        CollectionAssert.AreEqual(expectedRack, actualRack);
    }

    [Test]
    public void OnEndDrag_MoveTileRightOnRackRightOfAllTiles_RackRearranges()
    {
        var (tileTracker, charles, charlesFusion, tileLoco) = MakeVariablesForCharlestonTest();
        List<MonoObject> raycastResults = new() { MonoObject.PrivateRack };
        tileTracker.LocalPrivateRack = TestRack;
        int dropIx = -1;
        bool rightOfTile = false;

        tileLoco.OnEndDrag(raycastResults, dropIx, rightOfTile);
        List<int> expectedRack = new() { 1, 2, 3, 4, 0 };
        List<int> actualRack = tileTracker.LocalPrivateRack;

        CollectionAssert.AreEqual(expectedRack, actualRack);
    }

    // OnEndDrag, Charleston
    [Test]
    public void OnEndDrag_RackToEmptyCharlesSpot_TileMovesToCharleston()
    {
        var (tileTracker, charles, charlesFusion, tileLoco) = MakeVariablesForCharlestonTest();
        List<MonoObject> raycastTargets = new() { MonoObject.CharlestonSpot1 };

        tileLoco.OnEndDrag(raycastTargets);

        List<int> expectedRack = new(TestRack.ToList().GetRange(1, 4));
        int[] expectedPassArr = new int[] { -1, 0, -1 };
        (List<int> actualRack, int[] actualPassArr) =
            GetActualVarsForCharlestons(tileTracker, charles);
        CollectionAssert.AreEqual(expectedRack, actualRack);
        CollectionAssert.AreEqual(expectedPassArr, actualPassArr);
    }

    [Test]
    public void OnEndDrag_RackToOccupiedCharlesSpot_TileMovesToCharleston()
    {
        var (tileTracker, charles, charlesFusion, tileLoco) = MakeVariablesForCharlestonTest();
        List<MonoObject> raycastTargets = new() { MonoObject.CharlestonSpot1 };
        charles.ClientPassArr[1] = 84;

        tileLoco.OnEndDrag(raycastTargets);

        List<int> expectedRack = new(TestRack.ToList().GetRange(1, 4));
        expectedRack.Add(84);
        int[] expectedPassArr = new int[] { -1, 0, -1 };
        (List<int> actualRack, int[] actualPassArr) =
            GetActualVarsForCharlestons(tileTracker, charles);

        CollectionAssert.AreEqual(expectedRack, actualRack);
        CollectionAssert.AreEqual(expectedPassArr, actualPassArr);
    }

    [Test]
    public void OnEndDrag_JokerToCharleston_TileMovesBack()
    {
        int jokerId = 147;
        var (tileTracker, charles, charlesFusion, tileLoco) = MakeVariablesForCharlestonTest(jokerId);
        tileTracker.LocalPrivateRack.Add(jokerId);
        List<MonoObject> raycastTargets = new() { MonoObject.CharlestonSpot0 };

        tileLoco.OnEndDrag(raycastTargets);

        List<int> expectedRack = TestRack;
        expectedRack.Add(jokerId);
        int[] expectedPassArr = new int[] { -1, -1, -1 };
        (List<int> actualRack, int[] actualPassArr) =
            GetActualVarsForCharlestons(tileTracker, charles);

        CollectionAssert.AreEqual(expectedRack, actualRack);
        CollectionAssert.AreEqual(expectedPassArr, actualPassArr);
    }

    [Test]
    public void OnEndDrag_CharlesToEndOfRack_TileMovesToRack()
    {
        var (tileTracker, charles, charlesFusion, tileLoco) = MakeVariablesForCharlestonTest(84);
        List<MonoObject> raycastTargets = new() { MonoObject.PrivateRack };
        charles.ClientPassArr[1] = 84;

        tileLoco.OnEndDrag(raycastTargets, -1);

        List<int> expectedRack = TestRack;
        expectedRack.Add(84);
        int[] expectedPassArr = new int[] { -1, -1, -1 };
        (List<int> actualRack, int[] actualPassArr) =
            GetActualVarsForCharlestons(tileTracker, charles);

        CollectionAssert.AreEqual(expectedRack, actualRack);
        CollectionAssert.AreEqual(expectedPassArr, actualPassArr);
    }

    [Test]
    public void OnEndDrag_CharlesToMiddleOfRackRightOfTile_TileMovesToRack()
    {
        var (tileTracker, charles, charlesFusion, tileLoco) = MakeVariablesForCharlestonTest(84);
        List<MonoObject> raycastTargets = new() { MonoObject.PrivateRack };
        charles.ClientPassArr[1] = 84;

        tileLoco.OnEndDrag(raycastTargets, 2, true);

        List<int> expectedRack = TestRack;
        expectedRack.Insert(3, 84);
        int[] expectedPassArr = new int[] { -1, -1, -1 };
        (List<int> actualRack, int[] actualPassArr) =
            GetActualVarsForCharlestons(tileTracker, charles);

        CollectionAssert.AreEqual(expectedRack, actualRack);
        CollectionAssert.AreEqual(expectedPassArr, actualPassArr);
    }

    [Test]
    public void OnEndDrag_CharlesToMiddleOfRackLeftOfTile_TileMovesToRack()
    {
        var (tileTracker, charles, charlesFusion, tileLoco) = MakeVariablesForCharlestonTest(84);
        List<MonoObject> raycastTargets = new() { MonoObject.PrivateRack };
        charles.ClientPassArr[1] = 84;

        tileLoco.OnEndDrag(raycastTargets, 2, false);

        List<int> expectedRack = TestRack;
        expectedRack.Insert(2, 84);
        int[] expectedPassArr = new int[] { -1, -1, -1 };
        (List<int> actualRack, int[] actualPassArr) =
            GetActualVarsForCharlestons(tileTracker, charles);

        CollectionAssert.AreEqual(expectedRack, actualRack);
        CollectionAssert.AreEqual(expectedPassArr, actualPassArr);
    }

    [Test]
    public void OnEndDrag_CharlesToCharlesEmpty_CharlesRearranges()
    {
        var (tileTracker, charles, _, tileLoco) = MakeVariablesForCharlestonTest(84);
        List<MonoObject> raycastTargets = new() { MonoObject.CharlestonSpot2 };
        charles.ClientPassArr[1] = 84;

        tileLoco.OnEndDrag(raycastTargets);

        List<int> expectedRack = TestRack;
        int[] expectedPassArr = new int[] { -1, -1, 84 };
        (List<int> actualRack, int[] actualPassArr) =
            GetActualVarsForCharlestons(tileTracker, charles);

        CollectionAssert.AreEqual(expectedRack, actualRack);
        CollectionAssert.AreEqual(expectedPassArr, actualPassArr);
    }

    [Test]
    public void OnEndDrag_CharlesToCharlesPopulated_CharlesRearranges()
    {
        var (tileTracker, charles, charlesFusion, tileLoco) = MakeVariablesForCharlestonTest(84);
        List<MonoObject> raycastTargets = new() { MonoObject.CharlestonSpot2 };
        charles.ClientPassArr[1] = 84;
        charles.ClientPassArr[2] = 4;

        tileLoco.OnEndDrag(raycastTargets);

        List<int> expectedRack = TestRack;
        int[] expectedPassArr = new int[] { -1, 4, 84 };
        (List<int> actualRack, int[] actualPassArr) =
            GetActualVarsForCharlestons(tileTracker, charles);

        CollectionAssert.AreEqual(expectedRack, actualRack);
        CollectionAssert.AreEqual(expectedPassArr, actualPassArr);
    }

    [Test]
    public void OnEndDrag_CharlesToNowhere_TileMovesBack()
    {
        var (tileTracker, charles, charlesFusion, tileLoco) = MakeVariablesForCharlestonTest(84);
        List<MonoObject> raycastTargets = new() { };
        charles.ClientPassArr[1] = 84;
        charles.ClientPassArr[2] = 4;

        tileLoco.OnEndDrag(raycastTargets);

        List<int> expectedRack = TestRack;
        int[] expectedPassArr = new int[] { -1, 84, 4 };
        (List<int> actualRack, int[] actualPassArr) =
            GetActualVarsForCharlestons(tileTracker, charles);

        CollectionAssert.AreEqual(expectedRack, actualRack);
        CollectionAssert.AreEqual(expectedPassArr, actualPassArr);
    }

    // OnEndDrag, Gameplay

    [Test]
    public void OnEndDrag_RackToNowhere_TileMovesBack()
    {
        var (tileTracker, charles, charlesFusion, tileLoco) = MakeVariablesForCharlestonTest();
        List<MonoObject> raycastResults = new();
        tileTracker.LocalPrivateRack = TestRack;

        tileLoco.OnEndDrag(raycastResults);
        List<int> expectedRack = TestRack;
        List<int> actualRack = tileTracker.LocalPrivateRack;

        CollectionAssert.AreEqual(expectedRack, actualRack);
    }

    // Utility functions
    List<int> TestRack { get => new(Enumerable.Range(0, 5)); }
    int[] TestClientPassArr { get => new int[] { -1, -1, -1 }; }

    (TileTrackerClient, CharlestonClient, FakeCharlestonFusion, TileLocomotion)
        MakeVariablesForCharlestonTest(int tileId = 0)
    {
        ClassReferences refs = new();
        new FakeFusionManager(refs) { GamePhase = GamePhase.Charleston };
        new FakeMonoWrapper(refs);
        TileTrackerClient tileTracker = new(refs) { LocalPrivateRack = TestRack };
        CharlestonClient charleston = new(refs) { ClientPassArr = TestClientPassArr };
        FakeCharlestonFusion charlesFusion = new(refs);
        TileLocomotion tileLoco = new(refs, new FakeTileLocomotionMono(tileId));

        return (tileTracker, charleston, charlesFusion, tileLoco);
    }

    (List<int>, int[]) GetActualVarsForCharlestons(
        TileTrackerClient tileTracker, CharlestonClient charles)
    {
        List<int> actualRack = tileTracker.LocalPrivateRack;
        int[] actualPassArr = charles.ClientPassArr;

        return (actualRack, actualPassArr);
    }

    (TileTrackerClient, TileLocomotion)
        MakeVariablesForGameplayTest(int tileId = 0)
    {
        ClassReferences refs = new();
        new FakeFusionManager(refs) { GamePhase = GamePhase.Charleston };
        new FakeMonoWrapper(refs);
        TileTrackerClient tileTracker = new(refs) { LocalPrivateRack = TestRack };
        TileLocomotion tileLoco = new(refs, new FakeTileLocomotionMono(tileId));

        return (tileTracker, tileLoco);
    }
}

class FakeTileLocomotionMono : ITileLocomotionMono
{
    public FakeTileLocomotionMono(int tileId) => TileId = tileId;

    public int TileId { get; private set; }

    public List<float> TilePositions => throw new NotImplementedException();

    public void MoveBack() { }

    public void MoveTile(Transform newParent)
    {
        throw new NotImplementedException();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        throw new NotImplementedException();
    }

    public void OnDrag(PointerEventData eventData)
    {
        throw new NotImplementedException();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        throw new NotImplementedException();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        throw new NotImplementedException();
    }

    public void OnSelect(BaseEventData eventData)
    {
        throw new NotImplementedException();
    }
}