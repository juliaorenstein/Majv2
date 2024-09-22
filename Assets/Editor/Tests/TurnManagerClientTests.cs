using NUnit.Framework;

public class TurnManagerClientTests
{
    [Test]
    public void OnPointerClick_StartGamePlay_ActivePlayer_RequestDiscard() { }

    [Test]
    public void OnEndDrag_StartGamePlay_ActivePlayer_RequestDiscard() { }

    [Test]
    public void SpaceBar_StartGamePlay_ActivePlayer_RequestDisard() { }

    [Test]
    public void OnPointerClick_StartGamePlay_NonActivePlayer_TilesStaysOnRack() { }

    [Test]
    public void OnEndDrag_StartGamePlay_NonActivePlayer_TileStaysOnRack() { }

    [Test]
    public void SpaceBar_StartGamePlay_NonActivePlayer_TileStaysOnRack() { }

    [Test]
    public void OnPointerClick_RequestDiscard_ActivePlayer_RequestDiscard() { }

    [Test]
    public void OnEndDrag_RequestDiscard_ActivePlayer_RequestDiscard() { }

    [Test]
    public void SpaceBar_RequestDiscard_ActivePlayer_RequestDisard() { }

    [Test]
    public void OnPointerclick_RequestDiscard_AlreadyDiscardedThisTurn_TileStaysOnRack() { }

    [Test]
    public void OnEndDrag_RequestDiscard_AlreadyDiscardedThisTurn_TileStaysOnRack() { }

    [Test]
    public void SpaceBar_RequestDiscard_AlreadyDiscardedThisTurn_TileStaysOnRack() { }

    [Test]
    public void OnPointerClick_NonActivePlayer_TilesStaysOnRack() { }

    [Test]
    public void OnEndDrag_NonActivePlayer_TileStaysOnRack() { }

    [Test]
    public void SpaceBar_NonActivePlayer_TileStaysOnRack() { }

    [Test]
    public void OnPointerClick_Expose_ExposePlayer_RequestExpose() { }

    [Test]
    public void OnEndDrag_Expose_ExposePlayer_RequestExpose() { }

    [Test]
    public void SpaceBar_Expose_ExposePlayer_RequestExpose() { }

    [Test]
    public void OnPointerClick_Expose_NonExposePlayer_TileStaysOnPrivateRack() { }

    [Test]
    public void OnEndDrag_Expose_NonExposePlayer_TileStaysOnPrivateRack() { }

    [Test]
    public void SpaceBar_Expose_NonExposePlayer_TileStaysOnPrivateRack() { }

    [Test]
    public void OnPointerClick_DiscardDuringExpose_RequestDiscard() { }

    [Test]
    public void OnEndDrag_DiscardDuringExpose_RequestDiscard() { }

    [Test]
    public void SpaceBar_DiscardDuringExpose_RequestDiscard() { }

    // TODO: implement dead hand option for people who can't validly expose
    [Test]
    public void OnPointerClick_InvalidDiscardDuringExpose_TileStaysOnRack() { }

    [Test]
    public void OnEndDrag_InvalidDiscardDuringExpose_TileStaysOnRack() { }

    [Test]
    public void SpaceBar_InvalidDiscardDuringExpose_TileStaysOnRack() { }

    [Test]
    public void NeverMind_WhenCalled_RequestNeverMind() { }

    // TODO: joker swap functionality



    // The following five tests are out of scope for TileLocomotion, but when TurnManager is tested, those tests should start from this class.
    // 

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
}
