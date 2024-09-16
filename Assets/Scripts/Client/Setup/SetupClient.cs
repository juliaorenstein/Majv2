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
        new TileTrackerClient(refs);

        HideButtons();                      // hide start buttons

        // show the other player's racks
        setupMono.PopulateOtherRacks(refs.FManager.IsDealer);
        refs.Nav.SetNetworkCallbacks(refs.NetworkCallbacks);
    }

    void HideButtons() => refs.Mono.SetActive(MonoObject.StartButtons, false);
}
