using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;

public class TileLocomotionTests
{
    // OnPointerClick, Charleston
    [Test]
    public void OnPointerClick_RackTileDuringCharles_TileMovesToCharles()
    {
        (ClassReferences refs
            , GameManagerClient game
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
        (ClassReferences refs
            , GameManagerClient game
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
        (ClassReferences refs
            , GameManagerClient game
            , CharlestonClient charles
            , TileLocomotion tileLoco) = MakeVariablesForCharlestonTest();
        charles.ClientPassArr = new int[] { 11, 12, 13 };

        tileLoco.OnPointerClick(true);

        List<int> expectedRack = TestRack.GetRange(1, 4).ToList();
        int[] expectedPassArr = new int[] { 11, 12, 0 };
        (List<int> actualRack, int[] actualPassArr) =
            GetActualVarsForCharlestons(game, charles);

        CollectionAssert.AreEqual(expectedRack, actualRack);
        CollectionAssert.AreEqual(expectedPassArr, actualPassArr);
    }

    [Test]
    public void OnPointerClick_CharlesTile_TileMovesToRack()
    {
        (ClassReferences refs
            , GameManagerClient game
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
    [Test]
    public void OnPointerClick_RackTileWhenDiscardIsValid_TileIsDiscarded()
    {
        (ClassReferences refs
            , GameManagerClient game
            , TileLocomotion tileLoco) = MakeVariablesForGameplayTest();

        tileLoco.OnPointerClick(true);

        List<int> expectedRack = TestRack;
        


    }

    [Test]
    public void OnPointerClick_RackTileWhenExposeIsValid_TileIsExposed() { throw new NotImplementedException(); }

    [Test]
    public void OnPointerClick_RackTileWhenDiscardAndExposePossible_Discarded() { throw new NotImplementedException(); }

    [Test]
    public void OnPointerClick_RackTileWhenDiscardAndExposePossible_Expose() { throw new NotImplementedException(); }

    [Test]
    public void OnPointerClick_DiscardedTileDuringCalling_TileIsCalled() { throw new NotImplementedException(); }

    // OnPointerClick, Nothing Happens
    [Test]
    public void OnPointerClick_SingleClick_NothingHappens() { throw new NotImplementedException(); }

    [Test]
    public void OnPointerClick_AlreadyDiscardedTile_NothingHappens() { throw new NotImplementedException(); }

    [Test]
    public void OnPointerClick_LocalDisplayRackTile_NothingHappends() { throw new NotImplementedException(); }

    [Test]
    public void OnPointerClick_OtherDisplayRackTile_NothingHappens() { throw new NotImplementedException(); }

    [Test]
    public void OnPointerClick_RackTileDuringCalling_NothingHappens() { throw new NotImplementedException(); }

    // OnEndDrag, Rack
    [Test]
    public void OnEndDrag_RackToRack_RackRearranges() { throw new NotImplementedException(); }

    // OnEndDrag, Charleston
    [Test]
    public void OnEndDrag_RackToCharleston_TileMovesToCharleston() { throw new NotImplementedException(); }

    [Test]
    public void OnEndDrag_JokerToCharleston_TileMovesBack() { throw new NotImplementedException(); }

    [Test]
    public void OnEndDrag_CharlestonToRack_TileMovesToRack() { throw new NotImplementedException(); }

    [Test]
    public void OnEndDrag_CharlesToCharles_CharlesRearranges() { throw new NotImplementedException(); }

    [Test]
    public void OnEndDrag_CharlestonToNowhere_TileMovesBack() { throw new NotImplementedException(); }

    // OnEndDrag, Gameplay
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

    [Test]
    public void OnEndDrag_RackToNowhere_TileMovesBack() { throw new NotImplementedException(); }

    // Utility functions
    List<int> TestRack { get => new(Enumerable.Range(0, 5).ToList()); }
    int[] TestClientPassArr { get => new int[] { -1, -1, -1 }; }

    (ClassReferences, GameManagerClient, CharlestonClient, TileLocomotion)
        MakeVariablesForCharlestonTest(int tileId = 0)
    {
        ClassReferences refs = new();
        new FakeFusionManager(refs) { GamePhase = GamePhase.Charleston };
        GameManagerClient gameManagerClient = new(refs) { PrivateRack = TestRack };
        CharlestonClient charleston = new(refs) { ClientPassArr = TestClientPassArr };
        TileLocomotion tileLoco = new(refs, new FakeTileLocomotionMono(tileId));

        return (refs, gameManagerClient, charleston, tileLoco);
    }

    (List<int>, int[]) GetActualVarsForCharlestons(
        GameManagerClient game, CharlestonClient charles)
    {
        List<int> actualRack = game.PrivateRack;
        int[] actualPassArr = charles.ClientPassArr;

        return (actualRack, actualPassArr);
    }

    (ClassReferences, GameManagerClient, TileLocomotion)
        MakeVariablesForGameplayTest(int tileId = 0)
    {
        ClassReferences refs = new();
        new FakeFusionManager(refs) { GamePhase = GamePhase.Charleston };
        GameManagerClient gameManagerClient = new(refs) { PrivateRack = TestRack };
        TileLocomotion tileLoco = new(refs, new FakeTileLocomotionMono(tileId));

        return (refs, gameManagerClient, tileLoco);
    }
}

class FakeTileLocomotionMono : ITileLocomotionMono
{
    public FakeTileLocomotionMono(int tileId) => TileId = tileId;

    public int TileId { get; private set; }

    public List<float> TilePositions => throw new NotImplementedException();

    public void MoveBack()
    {
        throw new NotImplementedException();
    }

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