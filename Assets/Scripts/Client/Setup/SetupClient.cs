public class SetupClient
{
    readonly ClassReferences refs;
    readonly SetupMono setupMono;
    public SetupClient(ClassReferences refs, SetupMono _setupMono)
    {
        this.refs = refs;
        setupMono = _setupMono;
    }

    public void SetupDriver()
    {
        refs.GManager = new(refs)
        {
            DealerId = 3 // TODO: rotate dealer each game
        };

        refs.GManagerClient = new(refs)
        {
            LocalPlayer = refs.Fusion.LocalPlayerId,
        };
        new TileTrackerClient(refs);
        new GameManagerClient(refs);

        //Refs.EventSystem.gameObject.AddComponent<Navigation>(); // TODO: remove this?

        HideButtons();                      // hide start buttons

        // show the other player's racks
        bool isDealer = refs.GManager.DealerId == refs.GManager.LocalPlayerId;
        setupMono.PopulateOtherRacks(isDealer);
        refs.Nav.SetNetworkCallbacks(refs.NetworkCallbacks);
    }

    void HideButtons() => refs.Mono.SetActive(MonoObject.StartButtons, false);
}
