using System.Linq;
using System.Diagnostics;

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

    public CharlestonClient(ClassReferences refs)
    {
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

    public void CharlestonTileMover(int tileId)
    {
        bool tileInLocalRack = gameManagerClient.PrivateRack.Contains(tileId);
        if (tileInLocalRack) MoveTileFromRackToCharleston(tileId);
        else MoveTileFromCharlestonToRack(tileId);
    }

    public void CharlestonTileMover(int tileId, MonoObject start, MonoObject end)
    {
        // swap two tiles in Charleston
        if ((objRefs.CharlestonSpots.Contains(start)
            && objRefs.CharlestonSpots.Contains(end))
            || start == MonoObject.CharlestonBox)
        {
            Debug.Assert(ClientPassArr.Contains(tileId));

            int startIx = SpotIx(start);
            int endIx = SpotIx(end);
            int tmp = ClientPassArr[startIx];
            ClientPassArr[startIx] = ClientPassArr[endIx];
            ClientPassArr[endIx] = tmp;
            return;
        }

        // move tile from rack to charleston
        if (objRefs.CharlestonSpots.Contains(end))
        {
            Debug.Assert(gameManagerClient.PrivateRack.Contains(tileId));

            // check if that spot is already populated
            int tileAlreadyInSpot = ClientPassArr[SpotIx(end)];
            if (Tile.IsValidTileId(ClientPassArr[SpotIx(end)]))
            {
                MoveTileFromCharlestonToRack(tileAlreadyInSpot);
            }

            MoveTileFromRackToCharleston(tileId);
        }
    }

    void MoveTileFromRackToCharleston(int tileId)
    {
        // find an empty spot in passArr to place tile. If there is none
        // put the tile in the last spot ( i = 2 )
        for (int i = 0; i < 3; i++)
        {
            if (!Tile.IsValidTileId(ClientPassArr[i]) || i == 2)
            {
                ClientPassArr[i] = tileId;
                gameManagerClient.PrivateRack.Remove(tileId);
            }
        }
    }

    void MoveTileFromCharlestonToRack(int tileId)
    {
        for (int i = 0; i < 3; i++)     // TODO: change passArr to nullable and replace -1s with null
        {
            if (ClientPassArr[i] == tileId) ClientPassArr[i] = -1;
        }
        gameManagerClient.PrivateRack.Add(tileId);
    }

    int SpotIx(MonoObject spot)
    {
        Debug.Assert(objRefs.CharlestonSpots.Contains(spot));
        return objRefs.CharlestonSpots.IndexOf(spot);
    }
}
