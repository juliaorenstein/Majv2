public class GameManagerClient
{
    // initiated in SetupClient
    public GameManagerClient(ClassReferences refs)
    {
        refs.GManagerClient = this;
    }

    public int LocalPlayer { get; set; }
    public int? ActivePlayer { get; set; }
    public bool IsActivePlayer { get => ActivePlayer == LocalPlayer; }
    public int? ExposingPlayer { get; set; }
    public bool IsExposingPlayer { get => ExposingPlayer == LocalPlayer; }
}
