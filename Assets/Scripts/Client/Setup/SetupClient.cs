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

        // just for host, allow skipping charlestons for debugging
        if (refs.FManager.IsServer)
        {
            ObjectReferences.Instance.CharlestonBox.parent.GetChild(2).gameObject.SetActive(true);
        }

        void HideButtons() => refs.Mono.SetActive(MonoObject.StartButtons, false);
    }
}
