using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;

public class TurnManagerTests
{
    [Test]
    public void StartGamePlay_WhenDealerIsAI_AITurn()
    {
        var (turnManagerServer, fakeFusionManager) = MakeVariablesForTest();

        turnManagerServer.StartGamePlay();
    }

    (TurnManagerServer, FakeFusionManager) MakeVariablesForTest()
    {
        ClassReferences refs = new();
        FakeFusionManager fakeFusionManager = new(refs);
        TurnManagerServer turnManagerServer = new(refs);
        return (turnManagerServer, fakeFusionManager);
    }
}
