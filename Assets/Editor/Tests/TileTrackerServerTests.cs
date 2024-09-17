using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;

public class TileTrackerServerTests : MonoBehaviour
{


    [Test]
    public void MoveTile_PrivateRackToCharleston_Success()
    {
        TileTrackerServerTestVars vars = CreateVariablesForTest();
    }

    [Test]
    public void MoveTile_CharlestonToPrivateRack_Success() { throw new NotImplementedException(); }

    [Test]
    public void MoveTile_WallToActivePrivateRack_Success() { throw new NotImplementedException(); }

    [Test]
    public void MoveTile_WallToNonActivePrivateRack_FailAssertion()
    {
        // haven't decided how much validation I actually want to implement in TileTrackerServer
        throw new NotImplementedException();
    }

    [Test]
    public void MoveTile_ActivePrivateRackToDiscard_Success() { throw new NotImplementedException(); }

    [Test]
    public void MoveTile_NonActivePrivateRackToDiscard_FailAssertion() { throw new NotImplementedException(); }

    [Test]
    public void MoveTile_DiscardToExposingDisplayRack_Success() { throw new NotImplementedException(); }

    [Test]
    public void MoveTile_DiscardToNonExposingDisplayRack_FailAssertion() { throw new NotImplementedException(); }

    [Test]
    public void MoveTile_PrivateRackToDisplayRackDuringExpose_Success() { throw new NotImplementedException(); }

    [Test]
    public void MoveTile_PrivateRackToDisplayRackOtherwise_FailAssertion() { throw new NotImplementedException(); }

    [Test]
    public void MoveTile_DisplayRackToDiscardDuringNeverMind_Success() { throw new NotImplementedException(); }

    [Test]
    public void MoveTile_DisplayRackToDiscardOtherwise_FailAssertion() { throw new NotImplementedException(); }

    [Test]
    public void MoveTile_DisplayRackToOtherDisplayRackDuringJokerSwap() { throw new NotImplementedException(); }

    [Test]
    public void MoveTile_DisplayRackToOtherDisplayRackNotJokerSwap_FailAssertion() { throw new NotImplementedException(); }

    TileTrackerServerTestVars CreateVariablesForTest()
    {
        TileTrackerServerTestVars vars = new();
        ClassReferences refs = new();
        vars.tileTracker = new(refs);
        vars.tileTracker.wall

    }
}

struct TileTrackerServerTestVars
{
    public TileTrackerServer tileTracker;
}
