public class SetupClient
{
    ObjectReferences Refs;
    public SetupClient(ObjectReferences refs)
    {
        Refs = refs;
    }

    public void SetupDriver()
    {
        Refs.GManager.LocalPlayerId = Refs.Fusion.LocalPlayerId;
        Refs.GManager.DealerId = 3; // : rotate dealer each game
        //Refs.EventSystem.gameObject.AddComponent<Navigation>(); // TODO: remove this?

        HideButtons();                      // hide start buttons
        // show the other player's racks
        bool isDealer = Refs.GManager.DealerId == Refs.GManager.LocalPlayerId;
        Refs.setupMono.PopulateOtherRacks(isDealer);
    }

    void HideButtons() => Refs.Mono.SetActive(MonoObject.StartButtons, false);
}
