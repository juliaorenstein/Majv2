using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

public class CharlestonHost
{
    readonly ClassReferences Refs;
    readonly IFusionWrapper fusion;
    readonly ICharlestonFusion CFusion;
    
    public readonly List<List<int>> PassList;
    public readonly List<List<int>> RecList;

    public bool AiPassed;
    int PlayersReady;
    int Offset
    {
        get
        {
            return CFusion.Counter switch
            {
                0 or 5 => 3, // right
                1 or 4 => 2, // over
                2 or 3 => 1, // left
                _ => throw new Exception("invalid Counter")
            };
        }
    }

    public CharlestonHost(ClassReferences refs)
    {
        Refs = refs;
        Refs.CHost = this;
     
        fusion = refs.Fusion;
        CFusion = refs.CFusion;

        PassList = new() { new(), new(), new(), new() };
        RecList = new() { new(), new(), new(), new() };

        AiPassed = false;
        PlayersReady = 0;
    }

    public void PassDriver(int sourcePlayerId, int[] tileIdsToPass)
    {
        foreach (int tile in tileIdsToPass)
        {
            Debug.Assert(Tile.IsValidTileId(tile)); // valid tiles
            Debug.Assert(!Tile.IsJoker(tile));      // not jokers
            Debug.Assert(Refs.GManager.Racks[sourcePlayerId].Contains(tile)); // tile is in player's rack
        }

        if (!AiPassed) AiTilesToPass();
        AddClientTilesToPassList(sourcePlayerId, tileIdsToPass);
        if (PlayersReady != 4) return; //don't continue until everybody's ready
        CalculateRecList();
        UpdateGameManagerRacks();
        SendNewRacksToClients();
        Reset();
    }

    void AiTilesToPass()
    {
        // loop through players and for AIs select first three tiles to pass
        for (int playerId = 0; playerId < 4; playerId++)
        {
            if (!fusion.IsPlayerAI(playerId)) continue;

            // This player's HostPassArr entry is the first three tiles of their rack
            PassList[playerId] = Refs.GManager.Racks[playerId].GetRange(0, 3);
            PlayersReady++;
        }
        AiPassed = true;
    }

    void AddClientTilesToPassList(int sourcePlayerId, int[] tileIdsToPass)
    {
        // add client's contribution to the PassList
        PassList[sourcePlayerId] = tileIdsToPass.ToList();
        PlayersReady++;
    }

    void CalculateRecList()
    {
        // Calculate RecList based on PassList
        for (int sourcePlayerId = 0; sourcePlayerId < 4; sourcePlayerId++)
        {
            int targetPlayerId = TargetPlayerId(sourcePlayerId);
            RecList[targetPlayerId] = new(PassList[sourcePlayerId]);
        }

        // make sure the amount of tiles passed/received matches for each player
        // (only applicable to steal passes). The loop needs to happen twice to
        // catch all possible arrangements of steals, which is why i goes to 8.
        for (int i = 0; i < 8; i++)
        {
            int playerId = i % 4;

            // for each extra tile received vs given, pass that extra tile onward
            while (RecList[playerId].Count() > PassList[playerId].Count())
            {
                int extraTileId = RecList[playerId].Last();
                RecList[playerId].Remove(extraTileId);
                RecList[TargetPlayerId(playerId)].Add(extraTileId);
            }
        }
    }

    void UpdateGameManagerRacks()
    {
        // Each player needs the tiles they passed removed from their rack and
        // the tiles they received added to their rack
        for (int playerId = 0; playerId < 4; playerId++)
        {
            foreach (int tileId in PassList[playerId])
            {
                Refs.GManager.Racks[playerId].Remove(tileId);
            }
            foreach (int tileId in RecList[playerId])
            {
                Refs.GManager.Racks[playerId].Add(tileId);
            }
        }
    }

    void SendNewRacksToClients()
    {
        for (int playerId = 0; playerId < 4; playerId++)
        {
            fusion.RPC_H2C_SendRack(playerId, RecList[playerId].ToArray());
        }
    }

    void Reset()
    {
        for (int i = 0; i < 4; i++)
        {
            PassList[i].Clear();
            RecList[i].Clear();
        }
        AiPassed = false;
        PlayersReady = 0;
        CFusion.Counter++;
    }

    int TargetPlayerId(int sourcePlayerId) =>
        TargetPlayerId(sourcePlayerId, Offset);

    int TargetPlayerId(int sourcePlayerId, int offset) =>
        (sourcePlayerId + offset) % 4;
}
