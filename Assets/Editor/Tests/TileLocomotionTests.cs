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
        (TileTrackerClient tileTracker
            , CharlestonClient charles
            , TileLocomotion tileLoco) = MakeVariablesForCharlestonTest();

        tileLoco.OnPointerClick(true);

        ObservableCollection<int> expectedRack = new(TestRack.ToList().GetRange(1, 4));
        int[] expectedPassArr = new int[] { 0, -1, -1 };
        (ObservableCollection<int> actualRack, int[] actualPassArr) =
            GetActualVarsForCharlestons(tileTracker, charles);

        CollectionAssert.AreEqual(expectedRack, actualRack);
        CollectionAssert.AreEqual(expectedPassArr, actualPassArr);
    }

    [Test]
    public void OnPointerClick_RackTileDuringCharlesNotEmpty_TileMovesToCharles()
    {
        (TileTrackerClient tileTracker
            , CharlestonClient charles
            , TileLocomotion tileLoco) = MakeVariablesForCharlestonTest();
        charles.ClientPassArr[0] = 94;

        tileLoco.OnPointerClick(true);

        ObservableCollection<int> expectedRack = new(TestRack.ToList().GetRange(1, 4));
        int[] expectedPassArr = new int[] { 94, 0, -1 };
        (ObservableCollection<int> actualRack, int[] actualPassArr) =
            GetActualVarsForCharlestons(tileTracker, charles);

        CollectionAssert.AreEqual(expectedRack, actualRack);
        CollectionAssert.AreEqual(expectedPassArr, actualPassArr);
    }

    [Test]
    public void OnPointerClick_RackTileDuringCharlesBoxIsFull_TileReplacesCharles()
    {
        (TileTrackerClient tileTracker
            , CharlestonClient charles
            , TileLocomotion tileLoco) = MakeVariablesForCharlestonTest();
        charles.ClientPassArr = new int[] { 11, 12, 13 };

        tileLoco.OnPointerClick(true);

        ObservableCollection<int> expectedRack = new(TestRack.ToList().GetRange(1, 4));
        expectedRack.Add(13);
        int[] expectedPassArr = new int[] { 11, 12, 0 };
        (ObservableCollection<int> actualRack, int[] actualPassArr) =
            GetActualVarsForCharlestons(tileTracker, charles);

        CollectionAssert.AreEqual(expectedRack, actualRack);
        CollectionAssert.AreEqual(expectedPassArr, actualPassArr);
    }

    [Test]
    public void OnPointerClick_CharlesTile_TileMovesToRack()
    {
        (TileTrackerClient tileTracker
            , CharlestonClient charles
            , TileLocomotion tileLoco) = MakeVariablesForCharlestonTest(11);
        charles.ClientPassArr = new int[] { 11, 12, 13 };

        tileLoco.OnPointerClick(true);

        ObservableCollection<int> expectedRack = TestRack;
        expectedRack.Add(11);
        int[] expectedPassArr = new int[] { -1, 12, 13 };
        (ObservableCollection<int> actualRack, int[] actualPassArr) =
            GetActualVarsForCharlestons(tileTracker, charles);

        CollectionAssert.AreEqual(expectedRack, actualRack);
        CollectionAssert.AreEqual(expectedPassArr, actualPassArr);
    }

    // OnPointerClick, Gameplay

    // The following five tests are out of scope for TileLocomotion, but when TurnManager is tested, those tests should start from this class.
    // public void OnPointerClick_RackTileWhenDiscardIsValid_TileIsDiscarded()

    // public void OnPointerClick_RackTileWhenExposeIsValid_TileIsExposed()

    // public void OnPointerClick_RackTileWhenDiscardAndExposePossible_Discarded()

    // public void OnPointerClick_RackTileWhenDiscardAndExposePossible_Expose()

    // public void OnPointerClick_DiscardedTileDuringCalling_TileIsCalled()

    // OnPointerClick, Nothing Happens - removed these because I'm going to throw exceptions at the end of all known cases for these, which will find invalid cases for me as I go through tileTrackerplay

    // OnEndDrag, Rack

    [Test]
    public void OnEndDrag_MoveTileRightOnRackRightOfAnotherTile_RackRearranges()
    {
        (TileTrackerClient tileTracker
            , CharlestonClient _
            , TileLocomotion tileLoco) = MakeVariablesForCharlestonTest();
        List<MonoObject> raycastResults = new() { MonoObject.PrivateRack };
        tileTracker.PrivateRack = TestRack;
        int dropIx = 3;
        bool rightOfTile = true;

        tileLoco.OnEndDrag(raycastResults, dropIx, rightOfTile);
        ObservableCollection<int> expectedRack = new() { 1, 2, 3, 0, 4 };
        ObservableCollection<int> actualRack = tileTracker.PrivateRack;

        CollectionAssert.AreEqual(expectedRack, actualRack);
    }

    [Test]
    public void OnEndDrag_MoveTileRightOnRackLeftOfAnotherTile_RackRearranges()
    {
        (TileTrackerClient tileTracker
            , CharlestonClient _
            , TileLocomotion tileLoco) = MakeVariablesForCharlestonTest();
        List<MonoObject> raycastResults = new() { MonoObject.PrivateRack };
        tileTracker.PrivateRack = TestRack;
        int dropIx = 3;
        bool rightOfTile = false;

        tileLoco.OnEndDrag(raycastResults, dropIx, rightOfTile);
        ObservableCollection<int> expectedRack = new() { 1, 2, 0, 3, 4 };
        ObservableCollection<int> actualRack = tileTracker.PrivateRack;

        CollectionAssert.AreEqual(expectedRack, actualRack);
    }

    [Test]
    public void OnEndDrag_MoveTileLeftOnRackRightOfAnotherTile_RackRearranges()
    {
        (TileTrackerClient tileTracker
            , CharlestonClient _
            , TileLocomotion tileLoco) = MakeVariablesForCharlestonTest(4);
        List<MonoObject> raycastResults = new() { MonoObject.PrivateRack };
        tileTracker.PrivateRack = TestRack;
        int dropIx = 2;
        bool rightOfTile = true;

        tileLoco.OnEndDrag(raycastResults, dropIx, rightOfTile);
        ObservableCollection<int> expectedRack = new() { 0, 1, 2, 4, 3 };
        ObservableCollection<int> actualRack = tileTracker.PrivateRack;

        CollectionAssert.AreEqual(expectedRack, actualRack);
    }

    [Test]
    public void OnEndDrag_MoveTileLeftOnRackLeftOfAnotherTile_RackRearranges()
    {
        (TileTrackerClient tileTracker
            , CharlestonClient _
            , TileLocomotion tileLoco) = MakeVariablesForCharlestonTest(4);
        List<MonoObject> raycastResults = new() { MonoObject.PrivateRack };
        tileTracker.PrivateRack = TestRack;
        int dropIx = 2;
        bool rightOfTile = false;

        tileLoco.OnEndDrag(raycastResults, dropIx, rightOfTile);
        ObservableCollection<int> expectedRack = new() { 0, 1, 4, 2, 3 };
        ObservableCollection<int> actualRack = tileTracker.PrivateRack;

        CollectionAssert.AreEqual(expectedRack, actualRack);
    }

    [Test]
    public void OnEndDrag_MoveTileRightOnRackRightOfAllTiles_RackRearranges()
    {
        (TileTrackerClient tileTracker
            , CharlestonClient _
            , TileLocomotion tileLoco) = MakeVariablesForCharlestonTest();
        List<MonoObject> raycastResults = new() { MonoObject.PrivateRack };
        tileTracker.PrivateRack = TestRack;
        int dropIx = -1;
        bool rightOfTile = false;

        tileLoco.OnEndDrag(raycastResults, dropIx, rightOfTile);
        ObservableCollection<int> expectedRack = new() { 1, 2, 3, 4, 0 };
        ObservableCollection<int> actualRack = tileTracker.PrivateRack;

        CollectionAssert.AreEqual(expectedRack, actualRack);
    }

    // OnEndDrag, Charleston
    [Test]
    public void OnEndDrag_RackToEmptyCharlesSpot_TileMovesToCharleston()
    {
        (TileTrackerClient tileTracker
            , CharlestonClient charles
            , TileLocomotion tileLoco) = MakeVariablesForCharlestonTest();
        List<MonoObject> raycastTargets = new() { MonoObject.CharlestonSpot1 };

        tileLoco.OnEndDrag(raycastTargets);

        ObservableCollection<int> expectedRack = new(TestRack.ToList().GetRange(1, 4));
        int[] expectedPassArr = new int[] { -1, 0, -1 };
        (ObservableCollection<int> actualRack, int[] actualPassArr) =
            GetActualVarsForCharlestons(tileTracker, charles);
        CollectionAssert.AreEqual(expectedRack, actualRack);
        CollectionAssert.AreEqual(expectedPassArr, actualPassArr);
    }

    [Test]
    public void OnEndDrag_RackToOccupiedCharlesSpot_TileMovesToCharleston()
    {
        (TileTrackerClient tileTracker
            , CharlestonClient charles
            , TileLocomotion tileLoco) = MakeVariablesForCharlestonTest();
        List<MonoObject> raycastTargets = new() { MonoObject.CharlestonSpot1 };
        charles.ClientPassArr[1] = 84;

        tileLoco.OnEndDrag(raycastTargets);

        ObservableCollection<int> expectedRack = new(TestRack.ToList().GetRange(1, 4));
        expectedRack.Add(84);
        int[] expectedPassArr = new int[] { -1, 0, -1 };
        (ObservableCollection<int> actualRack, int[] actualPassArr) =
            GetActualVarsForCharlestons(tileTracker, charles);

        CollectionAssert.AreEqual(expectedRack, actualRack);
        CollectionAssert.AreEqual(expectedPassArr, actualPassArr);
    }

    [Test]
    public void OnEndDrag_JokerToCharleston_TileMovesBack()
    {
        int jokerId = 147;
        (TileTrackerClient tileTracker
            , CharlestonClient charles
            , TileLocomotion tileLoco) = MakeVariablesForCharlestonTest(jokerId);
        tileTracker.PrivateRack.Add(jokerId);
        List<MonoObject> raycastTargets = new() { MonoObject.CharlestonSpot0 };

        tileLoco.OnEndDrag(raycastTargets);

        ObservableCollection<int> expectedRack = TestRack;
        expectedRack.Add(jokerId);
        int[] expectedPassArr = new int[] { -1, -1, -1 };
        (ObservableCollection<int> actualRack, int[] actualPassArr) =
            GetActualVarsForCharlestons(tileTracker, charles);

        CollectionAssert.AreEqual(expectedRack, actualRack);
        CollectionAssert.AreEqual(expectedPassArr, actualPassArr);
    }

    [Test]
    public void OnEndDrag_CharlesToEndOfRack_TileMovesToRack()
    {
        (TileTrackerClient tileTracker
            , CharlestonClient charles
            , TileLocomotion tileLoco) = MakeVariablesForCharlestonTest(84);
        List<MonoObject> raycastTargets = new() { MonoObject.PrivateRack };
        charles.ClientPassArr[1] = 84;

        tileLoco.OnEndDrag(raycastTargets, -1);

        ObservableCollection<int> expectedRack = TestRack;
        expectedRack.Add(84);
        int[] expectedPassArr = new int[] { -1, -1, -1 };
        (ObservableCollection<int> actualRack, int[] actualPassArr) =
            GetActualVarsForCharlestons(tileTracker, charles);

        CollectionAssert.AreEqual(expectedRack, actualRack);
        CollectionAssert.AreEqual(expectedPassArr, actualPassArr);
    }

    [Test]
    public void OnEndDrag_CharlesToMiddleOfRackRightOfTile_TileMovesToRack()
    {
        (TileTrackerClient tileTracker
            , CharlestonClient charles
            , TileLocomotion tileLoco) = MakeVariablesForCharlestonTest(84);
        List<MonoObject> raycastTargets = new() { MonoObject.PrivateRack };
        charles.ClientPassArr[1] = 84;

        tileLoco.OnEndDrag(raycastTargets, 2, true);

        ObservableCollection<int> expectedRack = TestRack;
        expectedRack.Insert(3, 84);
        int[] expectedPassArr = new int[] { -1, -1, -1 };
        (ObservableCollection<int> actualRack, int[] actualPassArr) =
            GetActualVarsForCharlestons(tileTracker, charles);

        CollectionAssert.AreEqual(expectedRack, actualRack);
        CollectionAssert.AreEqual(expectedPassArr, actualPassArr);
    }

    [Test]
    public void OnEndDrag_CharlesToMiddleOfRackLeftOfTile_TileMovesToRack()
    {
        (TileTrackerClient tileTracker
            , CharlestonClient charles
            , TileLocomotion tileLoco) = MakeVariablesForCharlestonTest(84);
        List<MonoObject> raycastTargets = new() { MonoObject.PrivateRack };
        charles.ClientPassArr[1] = 84;

        tileLoco.OnEndDrag(raycastTargets, 2, false);

        ObservableCollection<int> expectedRack = TestRack;
        expectedRack.Insert(2, 84);
        int[] expectedPassArr = new int[] { -1, -1, -1 };
        (ObservableCollection<int> actualRack, int[] actualPassArr) =
            GetActualVarsForCharlestons(tileTracker, charles);

        CollectionAssert.AreEqual(expectedRack, actualRack);
        CollectionAssert.AreEqual(expectedPassArr, actualPassArr);
    }

    [Test]
    public void OnEndDrag_CharlesToCharlesEmpty_CharlesRearranges()
    {
        (TileTrackerClient tileTracker
           , CharlestonClient charles
           , TileLocomotion tileLoco) = MakeVariablesForCharlestonTest(84);
        List<MonoObject> raycastTargets = new() { MonoObject.CharlestonSpot2 };
        charles.ClientPassArr[1] = 84;

        tileLoco.OnEndDrag(raycastTargets);

        ObservableCollection<int> expectedRack = TestRack;
        int[] expectedPassArr = new int[] { -1, -1, 84 };
        (ObservableCollection<int> actualRack, int[] actualPassArr) =
            GetActualVarsForCharlestons(tileTracker, charles);

        CollectionAssert.AreEqual(expectedRack, actualRack);
        CollectionAssert.AreEqual(expectedPassArr, actualPassArr);
    }

    [Test]
    public void OnEndDrag_CharlesToCharlesPopulated_CharlesRearranges()
    {
        (TileTrackerClient tileTracker
           , CharlestonClient charles
           , TileLocomotion tileLoco) = MakeVariablesForCharlestonTest(84);
        List<MonoObject> raycastTargets = new() { MonoObject.CharlestonSpot2 };
        charles.ClientPassArr[1] = 84;
        charles.ClientPassArr[2] = 4;

        tileLoco.OnEndDrag(raycastTargets);

        ObservableCollection<int> expectedRack = TestRack;
        int[] expectedPassArr = new int[] { -1, 4, 84 };
        (ObservableCollection<int> actualRack, int[] actualPassArr) =
            GetActualVarsForCharlestons(tileTracker, charles);

        CollectionAssert.AreEqual(expectedRack, actualRack);
        CollectionAssert.AreEqual(expectedPassArr, actualPassArr);
    }

    [Test]
    public void OnEndDrag_CharlesToNowhere_TileMovesBack()
    {
        (TileTrackerClient tileTracker
           , CharlestonClient charles
           , TileLocomotion tileLoco) = MakeVariablesForCharlestonTest(84);
        List<MonoObject> raycastTargets = new() { };
        charles.ClientPassArr[1] = 84;
        charles.ClientPassArr[2] = 4;

        tileLoco.OnEndDrag(raycastTargets);

        ObservableCollection<int> expectedRack = TestRack;
        int[] expectedPassArr = new int[] { -1, 84, 4 };
        (ObservableCollection<int> actualRack, int[] actualPassArr) =
            GetActualVarsForCharlestons(tileTracker, charles);

        CollectionAssert.AreEqual(expectedRack, actualRack);
        CollectionAssert.AreEqual(expectedPassArr, actualPassArr);
    }

    // OnEndDrag, Gameplay
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

    [Test]
    public void OnEndDrag_RackToNowhere_TileMovesBack()
    {
        (TileTrackerClient tileTracker
           , CharlestonClient _
           , TileLocomotion tileLoco) = MakeVariablesForCharlestonTest();
        List<MonoObject> raycastResults = new();
        tileTracker.PrivateRack = TestRack;

        tileLoco.OnEndDrag(raycastResults);
        ObservableCollection<int> expectedRack = TestRack;
        ObservableCollection<int> actualRack = tileTracker.PrivateRack;

        CollectionAssert.AreEqual(expectedRack, actualRack);
    }

    // Utility functions
    ObservableCollection<int> TestRack { get => new(Enumerable.Range(0, 5)); }
    int[] TestClientPassArr { get => new int[] { -1, -1, -1 }; }

    (TileTrackerClient, CharlestonClient, TileLocomotion)
        MakeVariablesForCharlestonTest(int tileId = 0)
    {
        ClassReferences refs = new();
        new GameManagerClient(refs);
        new FakeFusionManager(refs) { GamePhase = GamePhase.Charleston };
        new FakeMonoWrapper(refs);
        TileTrackerClient tileTracker = new(refs) { PrivateRack = TestRack };
        CharlestonClient charleston = new(refs) { ClientPassArr = TestClientPassArr };
        TileLocomotion tileLoco = new(refs, new FakeTileLocomotionMono(tileId));

        return (tileTracker, charleston, tileLoco);
    }

    (ObservableCollection<int>, int[]) GetActualVarsForCharlestons(
        TileTrackerClient tileTracker, CharlestonClient charles)
    {
        ObservableCollection<int> actualRack = tileTracker.PrivateRack;
        int[] actualPassArr = charles.ClientPassArr;

        return (actualRack, actualPassArr);
    }

    (TileTrackerClient, TileLocomotion)
        MakeVariablesForGameplayTest(int tileId = 0)
    {
        ClassReferences refs = new();
        new FakeFusionManager(refs) { GamePhase = GamePhase.Charleston };
        new FakeMonoWrapper(refs);
        TileTrackerClient tileTracker = new(refs) { PrivateRack = TestRack };
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