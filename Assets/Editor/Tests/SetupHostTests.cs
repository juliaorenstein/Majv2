using NUnit.Framework;
using System.Collections.Generic;

public class SetupHostTests
{
	[Test]
	public void SetupDriver_WhenCalled_PopulatesWall()
	{
		// ARRANGE
		ClassReferences refs = new();
        TileTracker tileTracker = new(refs);
		SetupHost setupHost = new(refs);

		// ACT
		setupHost.SetupDriver();
		List<int> wall = tileTracker.Wall;

		// ASSERT
		Assert.True(wall.Count == 152 - (13 * 4 + 1));
	}

    [Test]
    public void SetupDriver_WhenCalled_PopulatesRacks()
    {
        // ARRANGE
        ClassReferences refs = new();
        TileTracker tileTracker = new(refs);
        SetupHost setupHost = new(refs);

        // ACT
        setupHost.SetupDriver();
        List<List<int>> racks = tileTracker.PrivateRacks;

        // ASSERT
        Assert.True(racks.Count == 4);
        foreach (List<int> rack in racks)
        {
            Assert.True(rack.Count >= 13);
        }
    }
}