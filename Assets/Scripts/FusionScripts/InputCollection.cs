using Fusion;

public struct InputFromClient : INetworkInput { public NetworkButtons turnOptions; }

public class InputForServer { public Buttons input; }

public enum Buttons
{
    none = 0,
    call = 1,
    wait = 2,
    pass = 3,
    nevermind = 4
}

public class InputCollection : NetworkBehaviour
{
    public InputForServer Input;
    NetworkButtons previousTurnOptions;
    /*public bool wait;
    public bool pass;
    public bool call;
    public bool nevermind;*/

    public override void FixedUpdateNetwork()
    {
        /*
        if (GetInput(out InputFromClient input))
        {
            wait = input.turnOptions.WasPressed(previousTurnOptions, TurnButtons.wait);
            pass = input.turnOptions.WasPressed(previousTurnOptions, TurnButtons.pass);
            call = input.turnOptions.WasPressed(previousTurnOptions, TurnButtons.call);
            nevermind = input.turnOptions.WasPressed(previousTurnOptions, TurnButtons.nevermind);
        }
        */

        if (GetInput(out InputFromClient clientInput))
        {
            if (clientInput.turnOptions.WasPressed(previousTurnOptions, Buttons.wait))
            {
                Input.input = Buttons.wait;
                return;
            }
            if (clientInput.turnOptions.WasPressed(previousTurnOptions, Buttons.pass))
            {
                Input.input = Buttons.pass;
                return;
            }
            if (clientInput.turnOptions.WasPressed(previousTurnOptions, Buttons.call))
            {
                Input.input = Buttons.call;
                return;
            }
            if (clientInput.turnOptions.WasPressed(previousTurnOptions, Buttons.nevermind))
            {
                Input.input = Buttons.nevermind;
                return;
            }
        }

        previousTurnOptions = clientInput.turnOptions;
    }
}