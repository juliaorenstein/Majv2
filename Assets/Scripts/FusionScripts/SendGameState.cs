using Fusion;

public class SendGameState : NetworkBehaviour
{
    private void Awake()
    {
        ObjectReferences.Instance.ClassRefs.SendGame = this;
    }
}
