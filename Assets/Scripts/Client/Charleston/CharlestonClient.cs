using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;
using Codice.Client.BaseCommands;

public class CharlestonClient
{
    readonly ClassReferences refs;
    TileTrackerClient TileTracker { get => refs.TileTrackerClient; }
    IMonoWrapper Mono { get => refs.Mono; }
    ICharlestonFusion CharlestonFusion { get => refs.CFusion; }
    ObservableCollection<int> Rack { get => TileTracker.LocalPrivateRack; }

    public int[] ClientPassArr = new int[3] { -1, -1, -1 };
    readonly int[] StealPasses = new int[2] { 2, 5 };
    bool BlindAllowed { get => StealPasses.Contains(CharlestonFusion.Counter); }

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
        return ClientPassArr.All(tileId => Tile.IsValidTileId(tileId)) || BlindAllowed;
    }

    public void InitiatePass()
    {
        // quit out right away if somebody clicked the button when
        // it shouldn't be interactable
        if (!Mono.IsButtonInteractable(MonoObject.CharlestonPassButton)) return;

        // gray out the button and set the text
        Mono.SetButtonInteractable(MonoObject.CharlestonPassButton, false);
        Mono.SetButtonText(MonoObject.CharlestonPassButton, "Waiting for others");

        // move the tiles off the screen and create array that will be passed (w/out invalid tiles)
        List<int> toPass = new();
        foreach (int tileId in ClientPassArr)
        {
            if (Tile.IsValidTileId(tileId))
            {
                Mono.MoveTile(tileId, MonoObject.TilePool);
                toPass.Add(tileId);
            }
        }

        // give the tiles to the host
        CharlestonFusion.RPC_C2H_StartPass(toPass.ToArray());
    }

    void UpdateButton() { UpdateButton(CharlestonFusion.Counter); }

    public void UpdateButton(int counter)
    {
        // if Counter is -1 or 7, remove the button and start main gameplay
        if (counter == -1 || counter == 7)
        {
            FinishCharleston();
            return;
        }

        // if client isn't ready to pass, quit out
        if (!CheckReadyToPass())
        {
            Mono.SetButtonInteractable(MonoObject.CharlestonPassButton, false);
            return;
        }

        // FIXME: you can double-click and space bar a joker onto the charleston box

        // otherwise, get button ready for next pass
        NextPass();

        void FinishCharleston()
        {
            Mono.SetActive(MonoObject.CharlestonBox, false);
            Mono.SetActive(MonoObject.CharlestonPassButton, false);
            refs.TManager.C_StartGamePlay();
        }

        void NextPass()
        {
            Mono.SetButtonInteractable(MonoObject.CharlestonPassButton, true);
            Mono.SetButtonText(MonoObject.CharlestonPassButton, $"Pass {Direction(counter)}");
        }
    }

    string Direction()
    {
        return Direction(CharlestonFusion.Counter);
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
        Mono.MoveTile(tileId, CharlestonSpots[spotIx]);
        UpdateButton();
    }

    public void MoveTileFromCharlestonToRack(int tileId, int newIx = -1)
    {
        ClientPassArr[Array.IndexOf(ClientPassArr, tileId)] = -1;
        if (newIx == -1) Rack.Add(tileId);
        else Rack.Insert(newIx, tileId);
        UpdateButton();
    }

    void SwapCharles(int spotIx1, int spotIx2)
    {
        (ClientPassArr[spotIx1], ClientPassArr[spotIx2]) = (ClientPassArr[spotIx2], ClientPassArr[spotIx1]);
        if (Tile.IsValidTileId(ClientPassArr[spotIx1]))
        {
            Mono.MoveTile(ClientPassArr[spotIx1], CharlestonSpots[spotIx1]);
        }
        if (Tile.IsValidTileId(ClientPassArr[spotIx2]))
        {
            Mono.MoveTile(ClientPassArr[spotIx2], CharlestonSpots[spotIx2]);
        }
        UpdateButton();
    }

    int SpotIx(MonoObject spot)
    {
        Debug.Assert(CharlestonSpots.Contains(spot));
        return CharlestonSpots.IndexOf(spot);
    }

    public void ClearClientPassArr()
    {
        for (int i = 0; i < 3; i++)
        {
            ClientPassArr[i] = -1;
        }
    }
}
