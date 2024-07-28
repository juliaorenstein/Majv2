using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;

public class TileLocomotionTests
{
    // Note: Many client-side actions require logic and verification from the host. I won't test any of those actions in this class, and will probably return to them in TurnManager unit tests. I'll make notes of the test cases I'm avoiding here. The only tests here will be actions that can be verified and completed on the client side.

    // OnPointerClick, Charleston
    [Test]
    public void OnPointerClick_RackTileDuringCharles_TileMovesToCharles()
    {
        (GameManagerClient game
            , CharlestonClient charles
            , TileLocomotion tileLoco) = MakeVariablesForCharlestonTest();
        
        tileLoco.OnPointerClick(true);

        List<int> expectedRack = TestRack.GetRange(1, 4).ToList();
        int[] expectedPassArr = new int[] { 0, -1, -1 };
        (List<int> actualRack, int[] actualPassArr) =
            GetActualVarsForCharlestons(game, charles);

        CollectionAssert.AreEqual(expectedRack, actualRack);
        CollectionAssert.AreEqual(expectedPassArr, actualPassArr);
    }

    [Test]
    public void OnPointerClick_RackTileDuringCharlesNotEmpty_TileMovesToCharles()
    {
        (GameManagerClient game
            , CharlestonClient charles
            , TileLocomotion tileLoco) = MakeVariablesForCharlestonTest();
        charles.ClientPassArr[0] = 94;

        tileLoco.OnPointerClick(true);

        List<int> expectedRack = TestRack.GetRange(1, 4).ToList();
        int[] expectedPassArr = new int[] { 94, 0, -1 };
        (List<int> actualRack, int[] actualPassArr) =
            GetActualVarsForCharlestons(game, charles);

        CollectionAssert.AreEqual(expectedRack, actualRack);
        CollectionAssert.AreEqual(expectedPassArr, actualPassArr);
    }

    [Test]
    public void OnPointerClick_RackTileDuringCharlesBoxIsFull_TileReplacesCharles()
    {
        (GameManagerClient game
            , CharlestonClient charles
            , TileLocomotion tileLoco) = MakeVariablesForCharlestonTest();
        charles.ClientPassArr = new int[] { 11, 12, 13 };

        tileLoco.OnPointerClick(true);

        List<int> expectedRack = TestRack.GetRange(1, 4).ToList();
        expectedRack.Add(13);
        int[] expectedPassArr = new int[] { 11, 12, 0 };
        (List<int> actualRack, int[] actualPassArr) =
            GetActualVarsForCharlestons(game, charles);

        CollectionAssert.AreEqual(expectedRack, actualRack);
        CollectionAssert.AreEqual(expectedPassArr, actualPassArr);
    }

    [Test]
    public void OnPointerClick_CharlesTile_TileMovesToRack()
    {
        (GameManagerClient game
            , CharlestonClient charles
            , TileLocomotion tileLoco) = MakeVariablesForCharlestonTest(11);
        charles.ClientPassArr = new int[] { 11, 12, 13 };

        tileLoco.OnPointerClick(true);

        List<int> expectedRack = TestRack;
        expectedRack.Add(11);
        int[] expectedPassArr = new int[] { -1, 12, 13 };
        (List<int> actualRack, int[] actualPassArr) =
            GetActualVarsForCharlestons(game, charles);

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

    // OnPointerClick, Nothing Happens - removed these because I'm going to throw exceptions at the end of all known cases for these, which will find invalid cases for me as I go through gameplay

    // OnEndDrag, Rack

    [Test]
    public void OnEndDrag_MoveTileRightOnRackRightOfAnotherTile_RackRearranges()
    {
        (GameManagerClient game
            , TileLocomotion tileLoco) = MakeVariablesForGameplayTest();
        List<MonoObject> raycastResults = new() { MonoObject.PrivateRack };
        game.PrivateRack = TestRack;
        int dropIx = 3;
        bool rightOfTile = true;

        tileLoco.OnEndDrag(raycastResults, dropIx, rightOfTile);
        List<int> expectedRack = new() { 1, 2, 3, 0, 4 };
        List<int> actualRack = game.PrivateRack;

        CollectionAssert.AreEqual(expectedRack, actualRack);
    }

    [Test]
    public void OnEndDrag_MoveTileRightOnRackLeftOfAnotherTile_RackRearranges()
    {
        (GameManagerClient game
            , TileLocomotion tileLoco) = MakeVariablesForGameplayTest();
        List<MonoObject> raycastResults = new() { MonoObject.PrivateRack };
        game.PrivateRack = TestRack;
        int dropIx = 3;
        bool rightOfTile = false;

        tileLoco.OnEndDrag(raycastResults, dropIx, rightOfTile);
        List<int> expectedRack = new() { 1, 2, 0, 3, 4 };
        List<int> actualRack = game.PrivateRack;

        CollectionAssert.AreEqual(expectedRack, actualRack);
    }

    [Test]
    public void OnEndDrag_MoveTileLeftOnRackRightOfAnotherTile_RackRearranges()
    {
        (GameManagerClient game
            , TileLocomotion tileLoco) = MakeVariablesForGameplayTest(4);
        List<MonoObject> raycastResults = new() { MonoObject.PrivateRack };
        game.PrivateRack = TestRack;
        int dropIx = 2;
        bool rightOfTile = true;

        tileLoco.OnEndDrag(raycastResults, dropIx, rightOfTile);
        List<int> expectedRack = new() { 0, 1, 2, 4, 3 };
        List<int> actualRack = game.PrivateRack;

        CollectionAssert.AreEqual(expectedRack, actualRack);
    }

    [Test]
    public void OnEndDrag_MoveTileLeftOnRackLeftOfAnotherTile_RackRearranges()
    {
        (GameManagerClient game
            , TileLocomotion tileLoco) = MakeVariablesForGameplayTest(4);
        List<MonoObject> raycastResults = new() { MonoObject.PrivateRack };
        game.PrivateRack = TestRack;
        int dropIx = 2;
        bool rightOfTile = false;

        tileLoco.OnEndDrag(raycastResults, dropIx, rightOfTile);
        List<int> expectedRack = new() { 0, 1, 4, 2, 3 };
        List<int> actualRack = game.PrivateRack;

        CollectionAssert.AreEqual(expectedRack, actualRack);
    }

    [Test]
    public void OnEndDrag_MoveTileRightOnRackRightOfAllTiles_RackRearranges()
    {
        (GameManagerClient game
    , TileLocomotion tileLoco) = MakeVariablesForGameplayTest();
        List<MonoObject> raycastResults = new() { MonoObject.PrivateRack };
        game.PrivateRack = TestRack;
        int dropIx = -1;
        bool rightOfTile = false;

        tileLoco.OnEndDrag(raycastResults, dropIx, rightOfTile);
        List<int> expectedRack = new() { 1, 2, 3, 4, 0 };
        List<int> actualRack = game.PrivateRack;

        CollectionAssert.AreEqual(expectedRack, actualRack);
    }

    // OnEndDrag, Charleston
    [Test]
    public void OnEndDrag_RackToEmptyCharlesSpot_TileMovesToCharleston()
    {
        (GameManagerClient game
            , CharlestonClient charles
            , TileLocomotion tileLoco) = MakeVariablesForCharlestonTest();
        List<MonoObject> raycastTargets = new() { MonoObject.CharlestonSpot1 };

        tileLoco.OnEndDrag(raycastTargets);

        List<int> expectedRack = TestRack.GetRange(1, 4).ToList();
        int[] expectedPassArr = new int[] { -1, 0, -1 };
        (List<int> actualRack, int[] actualPassArr) =
            GetActualVarsForCharlestons(game, charles);
        CollectionAssert.AreEqual(expectedRack, actualRack);
        CollectionAssert.AreEqual(expectedPassArr, actualPassArr);
    }

    [Test]
    public void OnEndDrag_RackToOccupiedCharlesSpot_TileMovesToCharleston()
    {
        (GameManagerClient game
            , CharlestonClient charles
            , TileLocomotion tileLoco) = MakeVariablesForCharlestonTest();
        List<MonoObject> raycastTargets = new() { MonoObject.CharlestonSpot1 };
        charles.ClientPassArr[1] = 84;

        tileLoco.OnEndDrag(raycastTargets);

        List<int> expectedRack = TestRack.GetRange(1, 4).ToList();
        expectedRack.Add(84);
        int[] expectedPassArr = new int[] { -1, 0, -1 };
        (List<int> actualRack, int[] actualPassArr) =
            GetActualVarsForCharlestons(game, charles);

        CollectionAssert.AreEqual(expectedRack, actualRack);
        CollectionAssert.AreEqual(expectedPassArr, actualPassArr);
    }

    [Test]
    public void OnEndDrag_JokerToCharleston_TileMovesBack()
    {
        int jokerId = 147;
        (GameManagerClient game
            , CharlestonClient charles
            , TileLocomotion tileLoco) = MakeVariablesForCharlestonTest(jokerId);
        game.PrivateRack.Add(jokerId);
        List<MonoObject> raycastTargets = new() { MonoObject.CharlestonSpot0 };

        tileLoco.OnEndDrag(raycastTargets);

        List<int> expectedRack = TestRack;
        expectedRack.Add(jokerId);
        int[] expectedPassArr = new int[] { -1, -1, -1 };
        (List<int> actualRack, int[] actualPassArr) =
            GetActualVarsForCharlestons(game, charles);

        CollectionAssert.AreEqual(expectedRack, actualRack);
        CollectionAssert.AreEqual(expectedPassArr, actualPassArr);
    }

    [Test]
    public void OnEndDrag_CharlestonToRack_TileMovesToRack()
    {
        (GameManagerClient game
            , CharlestonClient charles
            , TileLocomotion tileLoco) = MakeVariablesForCharlestonTest(84);
        List<MonoObject> raycastTargets = new() { MonoObject.PrivateRack };
        charles.ClientPassArr[1] = 84;

        tileLoco.OnEndDrag(raycastTargets);

        List<int> expectedRack = TestRack;
        expectedRack.Add(84);
        int[] expectedPassArr = new int[] { -1, -1, -1 };
        (List<int> actualRack, int[] actualPassArr) =
            GetActualVarsForCharlestons(game, charles);

        CollectionAssert.AreEqual(expectedRack, actualRack);
        CollectionAssert.AreEqual(expectedPassArr, actualPassArr);
    }

    [Test]
    public void OnEndDrag_CharlesToCharlesEmpty_CharlesRearranges()
    {
        (GameManagerClient game
           , CharlestonClient charles
           , TileLocomotion tileLoco) = MakeVariablesForCharlestonTest(84);
        List<MonoObject> raycastTargets = new() { MonoObject.CharlestonSpot2 };
        charles.ClientPassArr[1] = 84;

        tileLoco.OnEndDrag(raycastTargets);

        List<int> expectedRack = TestRack;
        int[] expectedPassArr = new int[] { -1, -1, 84 };
        (List<int> actualRack, int[] actualPassArr) =
            GetActualVarsForCharlestons(game, charles);

        CollectionAssert.AreEqual(expectedRack, actualRack);
        CollectionAssert.AreEqual(expectedPassArr, actualPassArr);
    }

    [Test]
    public void OnEndDrag_CharlesToCharlesPopulated_CharlesRearranges()
    {
        (GameManagerClient game
           , CharlestonClient charles
           , TileLocomotion tileLoco) = MakeVariablesForCharlestonTest(84);
        List<MonoObject> raycastTargets = new() { MonoObject.CharlestonSpot2 };
        charles.ClientPassArr[1] = 84;
        charles.ClientPassArr[2] = 4;

        tileLoco.OnEndDrag(raycastTargets);

        List<int> expectedRack = TestRack;
        int[] expectedPassArr = new int[] { -1, 4, 84 };
        (List<int> actualRack, int[] actualPassArr) =
            GetActualVarsForCharlestons(game, charles);

        CollectionAssert.AreEqual(expectedRack, actualRack);
        CollectionAssert.AreEqual(expectedPassArr, actualPassArr);
    }

    [Test]
    public void OnEndDrag_CharlestonToNowhere_TileMovesBack()
    {
        (GameManagerClient game
           , CharlestonClient charles
           , TileLocomotion tileLoco) = MakeVariablesForCharlestonTest(84);
        List<MonoObject> raycastTargets = new() { };
        charles.ClientPassArr[1] = 84;
        charles.ClientPassArr[2] = 4;

        tileLoco.OnEndDrag(raycastTargets);

        List<int> expectedRack = TestRack;
        int[] expectedPassArr = new int[] { -1, 84, 4 };
        (List<int> actualRack, int[] actualPassArr) =
            GetActualVarsForCharlestons(game, charles);

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
    public void OnEndDrag_RackToNowhere_TileMovesBack() { throw new NotImplementedException(); }

    // Utility functions
    List<int> TestRack { get => new(Enumerable.Range(0, 5).ToList()); }
    int[] TestClientPassArr { get => new int[] { -1, -1, -1 }; }

    (GameManagerClient, CharlestonClient, TileLocomotion)
        MakeVariablesForCharlestonTest(int tileId = 0)
    {
        ClassReferences refs = new();
        new FakeFusionManager(refs) { GamePhase = GamePhase.Charleston };
        new FakeMonoWrapper(refs);
        GameManagerClient gameManagerClient = new(refs) { PrivateRack = TestRack };
        CharlestonClient charleston = new(refs) { ClientPassArr = TestClientPassArr };
        TileLocomotion tileLoco = new(refs, new FakeTileLocomotionMono(tileId));

        return (gameManagerClient, charleston, tileLoco);
    }

    (List<int>, int[]) GetActualVarsForCharlestons(
        GameManagerClient game, CharlestonClient charles)
    {
        List<int> actualRack = game.PrivateRack;
        int[] actualPassArr = charles.ClientPassArr;

        return (actualRack, actualPassArr);
    }

    (GameManagerClient, TileLocomotion)
        MakeVariablesForGameplayTest(int tileId = 0)
    {
        ClassReferences refs = new();
        new FakeFusionManager(refs) { GamePhase = GamePhase.Charleston };
        new FakeMonoWrapper(refs);
        GameManagerClient gameManagerClient = new(refs) { PrivateRack = TestRack };
        TileLocomotion tileLoco = new(refs, new FakeTileLocomotionMono(tileId));

        return (gameManagerClient, tileLoco);
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