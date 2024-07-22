using System.Linq;

public class CharlestonClient 
{
    ClassReferences Refs;
    IMonoWrapper mono;
    ICharlestonFusion CFusion;

    public int[] ClientPassArr;
    int[] StealPasses = new int[2] { 2, 5 };
    bool BlindAllowed { get => StealPasses.Contains(CFusion.Counter); }

    public CharlestonClient(ClassReferences refs)
    {
        Refs = refs;
        mono = refs.Mono;
        CFusion = refs.CFusion;
    }

    /*
    public CharlestonClient(IMonoWrapper monoWrapper, ICharlestonFusion cFusion)
    {
        mono = monoWrapper;
        ClassReferences.Instance.CClient = this;
        CFusion = cFusion;
    }
    */

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
        CFusion.RPC_C2H_StartPass(ClientPassArr);
    }

    public void ReceiveRackUpdate(int[] newRack)
    {
        Refs.GManagerClient.PrivateRack = newRack.ToList();
    }

    void UpdateButton() { UpdateButton(CFusion.Counter); }

    public void UpdateButton(int counter)
    {
        // if Counter is -1 or 7, remove the button and start main gameplay
        if (counter == -1 || counter == 7)
        {
            mono.SetActive(MonoObject.CharlestonBox, false);
            Refs.TManager.C_StartGamePlay();
            return;
        }

        mono.SetButtonInteractable(MonoObject.CharlestonPassButton, false);

        // set the direction text
        mono.SetButtonText(MonoObject.CharlestonBox, $"Pass {Direction(counter)}");
    }

    string Direction()
    {
        return Direction(CFusion.Counter);
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
}
