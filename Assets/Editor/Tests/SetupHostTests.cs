using NUnit.Framework;
using System.Collections.Generic;

public class SetupHostTests
{
	[Test]
	public void SetupDriver_WhenCalled_PopulatesWall()
	{
		// ARRANGE
		ClassReferences refs = new();
		GameManager gManager = new(refs);
		SetupHost setupHost = new(refs);

		// ACT
		setupHost.SetupDriver();
		Stack<int> wall = gManager.Wall;
		List<List<int>> racks = gManager.Racks;

		// ASSERT
		Assert.True(wall.Count == 152 - (13 * 4 + 1));
	}

    [Test]
    public void SetupDriver_WhenCalled_PopulatesRacks()
    {
        // ARRANGE
        ClassReferences refs = new();
        GameManager gManager = new(refs);
        SetupHost setupHost = new(refs);

        // ACT
        setupHost.SetupDriver();
        Stack<int> wall = gManager.Wall;
        List<List<int>> racks = gManager.Racks;

        // ASSERT
        Assert.True(racks.Count == 4);
        foreach (List<int> rack in racks)
        {
            Assert.True(rack.Count >= 13);
        }
    }
}