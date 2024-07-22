using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;

public class SendGameState : NetworkBehaviour
{
    ObjectReferences Refs = ObjectReferences.Instance;

    private void Awake()
    {
        Refs.SendGame = this;
    }
}
