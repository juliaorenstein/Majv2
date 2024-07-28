using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using System;

public class CharlestonClient 
{
    ClassReferences refs;
    GameManagerClient gameManagerClient;
    IMonoWrapper mono;
    ICharlestonFusion charlestonFusion;
    ObjectReferences objRefs;

    public int[] ClientPassArr;
    int[] StealPasses = new int[2] { 2, 5 };
    bool BlindAllowed { get => StealPasses.Contains(charlestonFusion.Counter); }

    public List<MonoObject> CharlestonSpots = new()
        {
            MonoObject.CharlestonSpot0,
            MonoObject.CharlestonSpot1,
            MonoObject.CharlestonSpot2
        };

    public CharlestonClient(ClassReferences refs)
    {
        refs.CClient = this;
        this.refs = refs;
        mono = refs.Mono;
        charlestonFusion = refs.CFusion;
        gameManagerClient = refs.GManagerClient;
        objRefs = ObjectReferences.Instance;
    }

    public bool CheckReadyToPass()
    {
        // TODO: instead of joker validation here, just make it impossible to drop a joker on the charleston box
        return ClientPassArr.All(tileId => Tile.IsValidTileId(tileId)) || BlindAllowed;
    }

    public void InitiatePass()
    {
        if (!mono.IsButtonInteractable(MonoObject.CharlestonPassButton))
        {
            return; // quit out right away if somebody clicked the button when
            // it shouldn't be interactable
        }

        // gray out the button and set the text
        mono.SetButtonInteractable(MonoObject.CharlestonPassButton, false);
        mono.SetButtonText(MonoObject.CharlestonPassButton, "Waiting for others");

        // move the tiles off the screen
        foreach (int tileId in ClientPassArr)
        {
            mono.MoveTile(tileId, MonoObject.TilePool);
        }

        // give the tiles to the host
        charlestonFusion.RPC_C2H_StartPass(ClientPassArr);
    }

    void UpdateButton() { UpdateButton(charlestonFusion.Counter); }

    public void UpdateButton(int counter)
    {
        // if Counter is -1 or 7, remove the button and start main gameplay
        if (counter == -1 || counter == 7)
        {
            mono.SetActive(MonoObject.CharlestonBox, false);
            refs.TManager.C_StartGamePlay();
            return;
        }

        mono.SetButtonInteractable(MonoObject.CharlestonPassButton, false);

        // set the direction text
        mono.SetButtonText(MonoObject.CharlestonBox, $"Pass {Direction(counter)}");
    }

    string Direction()
    {
        return Direction(charlestonFusion.Counter);
    }

    string Direction(int counter)
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

    public void DoubleClickCharlestonTileMover(int tileId)
    {
        if (gameManagerClient.PrivateRack.Contains(tileId))
        {
            DoubleClickRackToCharleston(tileId);
            return;
        }

        if (ClientPassArr.Contains(tileId))
        {
            MoveTileFromCharlestonToRack(tileId);
            return;
        }

        throw new Exception("Trying to move a tile from invalid location for charleston.");
    }

    void DoubleClickRackToCharleston(int tileId)
    {
        // find an empty spot in passArr to place tile. If there is none
        // put the tile in the last spot ( i = 2 )
        for (int i = 0; i < 3; i++)
        {
            if (!Tile.IsValidTileId(ClientPassArr[i]))
            {
                ClientPassArr[i] = tileId;
                gameManagerClient.PrivateRack.Remove(tileId);
                return;
            }
        }

        // if we reach here, all boxes were filled, so replace the tile at ix 2
        MoveTileFromCharlestonToRack(ClientPassArr[2]);
        MoveTileFromRackToCharleston(tileId, 2);
    }

    public void DragCharlestonTileMover(int tileId, MonoObject start, MonoObject end)
    {
        // swap two tiles in Charleston
        if (CharlestonSpots.Contains(start))
        {
            Debug.Assert(ClientPassArr.Contains(tileId));

            int startIx = SpotIx(start);
            int endIx = SpotIx(end);
            int tmp = ClientPassArr[startIx];
            ClientPassArr[startIx] = ClientPassArr[endIx];
            ClientPassArr[endIx] = tmp;
            return;
        }

        // drag - move tile from rack to charleston
        if (start == MonoObject.PrivateRack)
        {
            Debug.Assert(gameManagerClient.PrivateRack.Contains(tileId));

            // check if that spot is already populated
            int tileAlreadyInSpot = ClientPassArr[SpotIx(end)];
            if (Tile.IsValidTileId(ClientPassArr[SpotIx(end)]))
            {
                MoveTileFromCharlestonToRack(tileAlreadyInSpot);
            }

            MoveTileFromRackToCharleston(tileId, SpotIx(end));
            return;
        }

        throw new Exception("bad start location");
    }

    void MoveTileFromRackToCharleston(int tileId, int spotIx)
    {
        ClientPassArr[spotIx] = tileId;
        gameManagerClient.PrivateRack.Remove(tileId);
    }

    public void MoveTileFromCharlestonToRack(int tileId)
    {
        // TODO: change passArr to nullable and replace -1s with null
        ClientPassArr[Array.IndexOf(ClientPassArr, tileId)] = -1;
        gameManagerClient.PrivateRack.Add(tileId);
    }

    int SpotIx(MonoObject spot)
    {
        Debug.Assert(CharlestonSpots.Contains(spot));
        return CharlestonSpots.IndexOf(spot);
    }
}
