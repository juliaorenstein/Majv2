using System.Collections.Generic;
using System;

public class GameManager
{
    public FusionManager FManager;
    public int LocalPlayerId;
    public int DealerId;
    public static List<Tile> TileList = new();

    // Game State - only available to host
    public Stack<int> Wall = new();
    public List<List<int>> Racks = new() { new(), new(), new(), new() };

    public int WaitTime { get; set; } = 2000;

    public static int NextPlayer(int playerId) => (playerId + 1) % 4;
}

