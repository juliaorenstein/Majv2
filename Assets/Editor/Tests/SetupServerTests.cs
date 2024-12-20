﻿using NUnit.Framework;
using System.Collections.Generic;

public class SetupServerTests
{
    [Test]
    public void SetupDriver_WhenCalled_PopulatesWall()
    {
        // ARRANGE
        ClassReferences refs = new();
        SetupServer setupHost = new(refs);
        new FakeFusionWrapper(refs);
        new FakeFusionManager(refs);

        // ACT
        setupHost.SetupDriver();
        IReadOnlyList<int> wall = refs.TileTracker.Wall;

        // ASSERT
        Assert.True(wall.Count == 152 - (13 * 4 + 1));
    }

    [Test]
    public void SetupDriver_WhenCalled_PopulatesRacks()
    {
        // ARRANGE
        ClassReferences refs = new();
        SetupServer setupHost = new(refs);
        new FakeFusionWrapper(refs);
        new FakeFusionManager(refs);

        // ACT
        setupHost.SetupDriver();
        List<IReadOnlyList<int>> racks = refs.TileTracker.PrivateRacks;

        // ASSERT
        Assert.True(racks.Count == 4);
        foreach (IReadOnlyList<int> rack in racks)
        {
            Assert.True(rack.Count >= 13);
        }
    }
}