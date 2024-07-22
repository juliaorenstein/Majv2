using System.Collections.Generic;
using System.Linq;

public class CharlestonManager
{
    /*

    readonly ObjectReferences ObjectRefs;
    readonly ClassReferences Refs;
    
    public CharlestonFusion CFusion;

    public int[] ClientPassArr = new int[3];
    public List<List<int>> HostPassArr = new() { new(), new(), new(), new() };
    public List<List<int>> HostRecArr = new() { new(), new(), new(), new() };
    bool BlindAllowed { get => StealList.Contains(Counter); }
    int PlayersReady;
    public int Counter;
    readonly int OptPass = 6;
    readonly List<int> StealList = new() { 2, 5 };

    public CharlestonManager()
    {
        Refs = ClassReferences.Instance;
        Refs.CManager = this;
        ClientPassArr = new int[3] { -1, -1, -1 };
    }

    public CharlestonManager(ClassReferences refs)
    {

    }

    public bool C_CheckReady()
    {
        bool ready = true;
        bool jokers = false;

        if (ClientPassArr.Any(tileId => !Tile.IsValidTileId(tileId)) && !BlindAllowed)
        {
            ready = false;
        }

        else if (ClientPassArr.Any(tileId => Tile.IsJoker(tileId)))
        {
            jokers = true;
            ready = false;
            NoJokers();
        }

        // if you reopen this class comment out the below
        foreach (int tileId in ClientPassArr)
        {
            if (!Tile.IsValidTileId(tileId) && !BlindAllowed) // if a spot is empty and this isn't an optional pass
            {
                ready = false;
                continue;
            }

            if (GameManager.TileList[tileId].IsJoker())
            {
                jokers = true;
                ready = false;
                NoJokers();
                break;
            }
        }
        

        if (!jokers) { UpdateButton(); }

       
        return ready;
    }

    // client presses the button to start the pass, and the tiles in the
    // Charleston box get sent to the host
    public void C_StartPass()
    {
        foreach (int tileId in ClientPassArr)
        {
            Refs.mono.MoveTile(tileId, MonoObject.TilePool);
        }

        // give the tiles to the host
        CFusion.RPC_C2H_StartPass(ClientPassArr);
    }

    public void H_StartPass(int sourcePlayerId, int[] tileIDsToPass)
    {
        // if there are AI players, figure out their passes when the first player is ready
        if (PlayersReady == 0)
        {
            for (int playerId = 0; playerId < 4; playerId++)
            {
                if (Refs.fusion.IsPlayerAI(playerId)) { H_AICharlestonPass(playerId); }
            }
        }

        // update pass array
        HostPassArr[sourcePlayerId] = tileIDsToPass.ToList();
        PlayersReady += 1;
        if (PlayersReady == 4) { H_Pass(); }
    }

    // AI passes last three tiles every time
    void H_AICharlestonPass(int playerID)
    {
        List<int> aiRack = Refs.GManager.Racks[playerID];
        for (int i = 0; i < 3; i++)
        {
            HostPassArr[playerID] = aiRack.GetRange(aiRack.Count - 3, 3);
        }
        PlayersReady++;
    }

    // host does all the pass logic and reveals new tiles to players
    void H_Pass()
    {
        if (Counter != OptPass) { PassLogic(); }
        else { OptPassLogic(); }

        // now update racklists again and send to clients
        for (int targetId = 0; targetId < 4; targetId++)
        {
            Refs.GManager.Racks[targetId].AddRange(HostRecArr[targetId]);
            if (Refs.fusion.IsPlayerAI(targetId)) // prevent host from receiving all the AI tiles
            {
                CFusion.RPC_H2C_SendTiles(targetId, HostRecArr[targetId].ToArray());
            }

            for (int i = 0; i < 3; i++) HostRecArr[targetId][i] = -1;
        }

        // prep for next pass
        PlayersReady = 0;
        Counter++;
    }

    void PassLogic()
    {
        int targetID;
        List<List<int>> PassArr = HostPassArr.Select(subArr => subArr.ToList()).ToList();
        List<List<int>> RecArr = new() { new(), new(), new(), new() };

        List<int> WaitingForTilesList = PassArr.Select(list => list.Count).ToList();
        // first remove things from racklists (and set up RecArr while we're at it)
        for (int sourceId = 0; sourceId < 4; sourceId++)
        {
            foreach (int tileId in HostPassArr[sourceId])
            {
                Refs.GManager.Racks[sourceId].Remove(tileId);
            }
        }

        // now rearrange everything
        while (HostPassArr.Any(subArr => subArr.Any())) // while any subArrays are not empty
        {
            for (int sourceID = 0; sourceID < 4; sourceID++)
            {
                targetID = PassTargetID(sourceID, Direction());
                foreach (int tileID in HostPassArr[sourceID])
                {
                    if (RecArr[targetID].Count() < WaitingForTilesList[targetID])
                    {
                        RecArr[targetID].Add(tileID);
                    }
                    else { PassArr[targetID].Add(tileID); }
                }
                PassArr[sourceID].Clear();
            }
        }

        // helper function to determine the target of the pass
        static int PassTargetID(int sourceID, string direction)
        {
            // calculate the target off the pass rpc based on the local
            // player id and the charleston counter
            var shift = direction switch
            {
                "Right" => 3,
                "Over" => 2,
                _ => 1,
            };
            return (sourceID + shift) % 4;
        }
    }

    void OptPassLogic()
    {
        for (int i = 0; i < 2; i++)
        {
            int countA = HostPassArr[i].Count;
            int countB = HostPassArr[i + 2].Count;

            if (countA == countB)
            {
                HostRecArr[i] = HostPassArr[i + 2];
                HostRecArr[i + 2] = HostPassArr[i];
            }
            else
            {
                // TODdO: implement functionality for when optional pass is not the same number
            }
        }
    }

    public void C_ReceiveTiles(int[] tileIDsToReceive)
    {
        foreach (int tileID in tileIDsToReceive)
        { Refs.mono.MoveTile(tileID, MonoObject.PrivateRack); }

        UpdateButton(Counter + 1);
    }

    string Direction()
    {
        return Direction(Counter);
    }

    public string Direction(int counter)
    {
        return counter switch
        {
            // first right
            0 or 5 => "Right",
            // first over
            1 or 4 or 6 => "Over",
            // first left
            2 or 3 => "Left",
            _ => "Done",
        };
    }

    public void InitiatePass()
    {
        if (Refs.mono.IsButtonInteractable(MonoObject.CharlestonPassButton))
        {
            Refs.mono.SetButtonInteractable(MonoObject.CharlestonPassButton, false);
            Refs.mono.SetButtonText(MonoObject.CharlestonPassButton, "Waiting for others");
            C_StartPass();
        }
    }


    // Merging in from CharlestonPassButton
    public void UpdateButton() { UpdateButton(Counter); }

    public void UpdateButton(int counter)
    {
        // if Counter is -1 or 7, remove the button and start main gameplay
        if (counter == -1 || counter == 7)
        {
            Refs.mono.SetActive(MonoObject.CharlestonBox, false);
            Refs.TManager.C_StartGamePlay();
            return;
        }

        Refs.mono.SetButtonInteractable(MonoObject.CharlestonPassButton, false);

        // set the direction text
        Refs.mono.SetButtonText(MonoObject.CharlestonBox, $"Pass {Direction(counter)}");
    }

    public void NoJokers()
    {
        Refs.mono.SetButtonText(MonoObject.CharlestonBox, "You can't pass jokers");
    }
    */
}