using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;
using UnityEngine.Rendering;

public class CharlestonClient
{
    readonly ClassReferences refs;
    TileTrackerClient tileTracker { get => refs.TileTrackerClient; }
    IMonoWrapper mono { get => refs.Mono; }
    ICharlestonFusion charlestonFusion { get => refs.CFusion; }
    ObservableCollection<int> Rack { get => tileTracker.LocalPrivateRack; }

    public int[] ClientPassArr = new int[3] { -1, -1, -1 };
    readonly int[] StealPasses = new int[2] { 2, 5 }; // FIXME: figure out how to make this constant if appropriate
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
        if (Rack.Contains(tileId))
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
                MoveTileFromRackToCharleston(tileId, i);
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
            SwapCharles(SpotIx(start), SpotIx(end));
            return;
        }

        // drag - move tile from rack to charleston
        if (start == MonoObject.PrivateRack)
        {
            Debug.Assert(Rack.Contains(tileId));

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
        Rack.Remove(tileId);
        mono.MoveTile(tileId, CharlestonSpots[spotIx]);
    }

    public void MoveTileFromCharlestonToRack(int tileId, int newIx = -1)
    {
        // LEFTOFF: we're getting here when we should be rearrange tiles on the rack...
        // TODO: change passArr to nullable and replace -1s with null
        ClientPassArr[Array.IndexOf(ClientPassArr, tileId)] = -1;
        if (newIx == -1) Rack.Add(tileId);
        else Rack.Insert(newIx, tileId);
    }

    void SwapCharles(int spotIx1, int spotIx2)
    {
        (ClientPassArr[spotIx1], ClientPassArr[spotIx2]) = (ClientPassArr[spotIx2], ClientPassArr[spotIx1]);
        if (ClientPassArr[spotIx1] > -1)
        {
            mono.MoveTile(ClientPassArr[spotIx1], CharlestonSpots[spotIx1]);
        }
        if (ClientPassArr[spotIx2] > -1)
        {
            mono.MoveTile(ClientPassArr[spotIx2], CharlestonSpots[spotIx2]);
        }
    }

    int SpotIx(MonoObject spot)
    {
        Debug.Assert(CharlestonSpots.Contains(spot));
        return CharlestonSpots.IndexOf(spot);
    }
}
