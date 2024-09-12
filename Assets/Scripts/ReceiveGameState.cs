using System.Linq;

public class ReceiveGameState
{
    ClassReferences Refs;

    public ReceiveGameState(ClassReferences refs)
    {
        Refs = refs;
        Refs.ReceiveGame = this;
    }

    public void ReceiveRackUpdate(int[] newRack)
    {
        Refs.TileTrackerClient.PrivateRack = new(newRack);
    }
}
