public class GameManager
{
    public int LocalPlayerId;
    public int DealerId;

    public int WaitTime { get; set; } = 2000;

    public static int NextPlayer(int playerId) => (playerId + 1) % 4;

    // instantiated in SetupHost
    public GameManager(ClassReferences Refs)
    {
        Refs.GManager = this;
    }
}

public enum TurnPhase
{
    Discarding,
    LoggingCallers,
    Exposing,
}