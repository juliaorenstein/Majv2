using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Start : MonoBehaviour, IPointerClickHandler
{
    private NetworkCallbacks spawner;

    private void Awake()
    {
        spawner = GameObject.Find("NetworkRunner").GetComponent<NetworkCallbacks>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Set mode based on whether the network or offline button was pressed
        GameMode mode = gameObject.transform.GetSiblingIndex() == 0 ? GameMode.AutoHostOrClient : GameMode.Single;
        spawner.StartGame(mode); // start the game
        GetComponent<Button>().interactable = false;
        GetComponentInChildren<TextMeshProUGUI>().SetText("Connecting...");
    }
}
