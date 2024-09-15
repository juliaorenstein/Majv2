using NUnit.Framework;
using System.Collections.Generic;

public class SetupHostTests
{
    [Test]
    public void SetupDriver_WhenCalled_PopulatesWall()
    {
        // ARRANGE
        ClassReferences refs = new();
        SetupHost setupHost = new(refs);
        new FakeFusionWrapper(refs);
        new FakeFusionManager(refs);
        new GameManager(refs);

        // ACT
        setupHost.SetupDriver();
        List<int> wall = refs.TileTracker.Wall;

        // ASSERT
        Assert.True(wall.Count == 152 - (13 * 4 + 1));
    }

    [Test]
    public void SetupDriver_WhenCalled_PopulatesRacks()
    {
        // ARRANGE
        ClassReferences refs = new();
        SetupHost setupHost = new(refs);
        new FakeFusionWrapper(refs);
        new FakeFusionManager(refs);
        new GameManager(refs);

        // ACT
        setupHost.SetupDriver();
        List<List<int>> racks = refs.TileTracker.PrivateRacks;

        // ASSERT
        Assert.True(racks.Count == 4);
        foreach (List<int> rack in racks)
        {
            Assert.True(rack.Count >= 13);
        }
    }
}