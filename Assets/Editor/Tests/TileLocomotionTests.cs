using NUnit.Framework;

public class TileLocomotionTests
{
    // OnPointerClick, Charleston
    [Test]
    public void OnPointerClick_RackTileDuringCharles_TileMovesToCharles() { }

    [Test]
    public void OnPointerClick_RackTileDuringCharlesBoxIsFull_TileReplacesCharles() { }

    [Test]
    public void OnPointerClick_CharlesTile_TileMovesToRack() { }

    // OnPointerClick, Gameplay
    [Test]
    public void OnPointerClick_RackTileWhenDiscardIsValid_TileIsDiscarded() { }

    [Test]
    public void OnPointerClick_RackTileWhenExposeIsValid_TileIsExposed() { }

    [Test]
    public void OnPointerClick_RackTileWhenDiscardAndExposePossible_Discarded() { }

    [Test]
    public void OnPointerClick_RackTileWhenDiscardAndExposePossible_Expose() { }

    [Test]
    public void OnPointerClick_DiscardedTileDuringCalling_TileIsCalled() { }

    // OnPointerClick, Nothing Happens
    [Test]
    public void OnPointerClick_SingleClick_NothingHappens() { }

    [Test]
    public void OnPointerClick_AlreadyDiscardedTile_NothingHappens() { }

    [Test]
    public void OnPointerClick_LocalDisplayRackTile_NothingHappends() { }

    [Test]
    public void OnPointerClick_OtherDisplayRackTile_NothingHappens() { }

    [Test]
    public void OnPointerClick_RackTileDuringCalling_NothingHappens() { }

    // OnEndDrag, Rack
    [Test]
    public void OnEndDrag_RackToRack_RackRearranges() { }

    // OnEndDrag, Charleston
    [Test]
    public void OnEndDrag_RackToCharleston_TileMovesToCharleston() { }

    [Test]
    public void OnEndDrag_CharlestonToRack_TileMovesToRack() { }

    [Test]
    public void OnEndDrag_CharlesToCharles_CharlesRearranges() { }

    [Test]
    public void OnEndDrag_CharlestonToNowhere_TileMovesBack() { }

    // OnEndDrag, Gameplay
    [Test]
    public void OnEndDrag_RackToDiscardValid_Discard() { }

    [Test]
    public void OnEndDrag_RackToDiscardInvalid_TileMovesBack() { }

    [Test]
    public void OnEndDrag_DiscardToDisplayRack_Expose() { }

    [Test]
    public void OnEndDrag_RackToDisplayRackValid_Expose() { }

    [Test]
    public void OnEndDrag_RackToDisplayRackInvalid_TileMovesBack() { }

    [Test]
    public void OnEndDrag_RackToNowhere_TileMovesBack() { }
}
