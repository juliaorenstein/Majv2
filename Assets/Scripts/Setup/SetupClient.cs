public class SetupClient
{
    readonly ClassReferences Refs;
    readonly SetupMono setupMono;
    public SetupClient(ClassReferences refs, SetupMono _setupMono)
    {
        Refs = refs;
        setupMono = _setupMono;
    }

    public void SetupDriver()
    {
        Refs.GManager = new(Refs)
        {
            LocalPlayerId = Refs.Fusion.LocalPlayerId,
            DealerId = 3 // TODO: rotate dealer each game
        };
        Refs.GManagerClient = new(Refs);
        new EventMonitor(Refs);

        //Refs.EventSystem.gameObject.AddComponent<Navigation>(); // TODO: remove this?

        HideButtons();                      // hide start buttons
        // show the other player's racks
        bool isDealer = Refs.GManager.DealerId == Refs.GManager.LocalPlayerId;
        setupMono.PopulateOtherRacks(isDealer);
        Refs.Nav.SetNetworkCallbacks(Refs.NetworkCallbacks);
        Refs.ReceiveGame = new(Refs);
    }

    void HideButtons() => Refs.Mono.SetActive(MonoObject.StartButtons, false);
}
