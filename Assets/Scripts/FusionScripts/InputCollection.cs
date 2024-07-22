using Fusion;

public struct CallInputStruct : INetworkInput
{
    public NetworkButtons turnOptions;
}

public enum TurnButtons
{
    call = 0,
    wait = 1,
    pass = 2,
    nevermind = 3
}

public class InputCollection : NetworkBehaviour
{
    NetworkButtons previousTurnOptions;
    public bool wait;
    public bool pass;
    public bool call;
    public bool nevermind;

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out CallInputStruct input))
        {
            wait = input.turnOptions.WasPressed(previousTurnOptions, TurnButtons.wait);
            pass = input.turnOptions.WasPressed(previousTurnOptions, TurnButtons.pass);
            call = input.turnOptions.WasPressed(previousTurnOptions, TurnButtons.call);
            nevermind = input.turnOptions.WasPressed(previousTurnOptions, TurnButtons.nevermind);
        }

        previousTurnOptions = input.turnOptions;
    }
}
