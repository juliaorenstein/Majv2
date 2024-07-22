using NUnit.Framework;
using System.Collections.Generic;

public class SetupHostTests
{
	[Test]
	public void SetupDriver_WhenCalled_PopulatesWallAndRacks()
	{
		// ARRANGE
		ClassReferences refs = new() { GManager = new() };
		SetupHost setupHost = new(refs);

		// ACT
		setupHost.SetupDriver();
		Stack<int> wall = refs.GManager.Wall;
		List<List<int>> racks = refs.GManager.Racks;

		// ASSERT
		Assert.NotNull(wall);
		foreach (List<int> rack in racks)
		{
			Assert.True(rack.Count >= 13);
		}
	}
}