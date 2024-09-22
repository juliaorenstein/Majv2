using NUnit.Framework;

public class TileTrackerClientTests
{
    [Test]
    public void ReceiveGameState_BeginningOfGame_WallAndRacksPopulate() { }

    [Test]
    public void ReceiveGameState_CharlestonPass_PrivateRackUpdates() { }

    [Test]
    public void ReceiveGameState_LocalDiscard_TileMovesPrivateRackToDiscard() { }

    [Test]
    public void ReceiveGameState_ExternalDiscard_TileMovesTilePoolToDiscard() { }

    [Test]
    public void ReceiveGameState_ExternalDiscard_ExternalPrivateRackCountUpdates() { }

    [Test]
    public void ReceiveGameState_LocalNextTurn_TileMovesTilePoolToPrivateRack() { }

    [Test]
    public void ReceiveGameState_ExternalNextTurn_ExternalPrivateRackCountUpdates() { }

    [Test]
    public void ReceiveGameState_LocalCall_TileMovesDiscardToLocalDisplayRack() { }

    [Test]
    public void ReceiveGameState_ExternalCall_TileMovesDiscardToExternalDisplayRack() { }

    [Test]
    public void ReceiveGameState_LocalExpose_TileMovesPrivateRackToLocalDisplayRack() { }

    [Test]
    public void ReceiveGameState_ExternalExpose_TileMovesTilePoolToExternalDisplayRack() { }

    [Test]
    public void ReceiveGameState_ExternalExpose_ExternalPrivateRackCountUpdates() { }

    [Test]
    public void ReceiveGameState_LocalNeverMind_TileMovesDisplayRackToDiscard() { }

    [Test]
    public void ReceiveGameState_ExternalNeverMind_TileMovesDisplayRackToDiscard() { }




}
