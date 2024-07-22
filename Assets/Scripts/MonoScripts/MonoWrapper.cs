using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class MonoWrapper : MonoBehaviour, IMonoWrapper
{
    ObjectReferences ObjRefs;

    TextMeshProUGUI TurnIndicatorText;

    private void Awake()
    {
        ObjRefs = ObjectReferences.Instance;
        ObjRefs.ClassRefs.Mono = this;
    }

    private void Start()
    {
        TurnIndicatorText = ObjRefs.TurnIndicator.transform.GetChild(0)
            .GetComponent<TextMeshProUGUI>();
    }

    public void SetActive(MonoObject monoObject, bool value)
    {
        ObjRefs.ObjectDict[monoObject].gameObject.SetActive(value);
    }

    public void SetRaycastTarget(MonoObject monoObject, bool value)
    {
        ObjRefs.ObjectDict[monoObject].transform.GetComponent<Image>().raycastTarget = value;
    }

    public void SetRaycastTargetOnTile(int tileId, bool value)
    {
        Debug.Assert(Tile.IsValidTileId(tileId));
        GameManager.TileList[tileId].tileComponent
                   .GetComponentInChildren<Image>()
                   .raycastTarget = value;
    }

    public bool IsButtonInteractable(MonoObject monoObject)
    {
        Button button = ObjRefs.ObjectDict[monoObject].GetComponent<Button>();
        Debug.Assert(button != null);
        return button.IsInteractable();
    }

    public void SetButtonInteractable(MonoObject monoObject, bool value)
    {
        Button button = ObjRefs.ObjectDict[monoObject].GetComponent<Button>();
        Debug.Assert(button != null);
        ObjRefs.ObjectDict[monoObject].GetComponent<Button>().interactable = value;
    }

    public void MoveTile(int tileId, MonoObject destination)
    {
        MoveTile(tileId, ObjRefs.ObjectDict[destination].transform);
    }

    private void MoveTile(int tileId, Transform destination)
    {
        Debug.Assert(Tile.IsValidTileId(tileId));
        GameManager.TileList[tileId].tileComponent
                   .GetComponentInChildren<TileLocomotion>()
                   .MoveTile(destination);
    }

    public void StartNewCoroutine(IEnumerator func)
    {
        StartCoroutine(func);
    }

    public IEnumerator WaitForSeconds(int seconds)
    {
        yield return new WaitForSeconds(seconds);
    }

    public void SetButtonText(MonoObject monoObject, string text)
    {
        Button button = ObjRefs.ObjectDict[monoObject].GetComponent<Button>();
        Debug.Assert(button != null);
        button.GetComponentInChildren<TextMeshProUGUI>()
            .SetText(text);

    }


    // TODO: remove and generalize calls to SetButtonText
    public void SetTurnIndicatorText(int playerId)
    {
        TurnIndicatorText.GetComponent<TextMeshProUGUI>()
            .SetText($"Player {playerId}'s turn");
    }

    public void ExposeOtherPlayerTile(int rackId, int tileId)
    {
        Transform exposePlayerRack = ObjRefs.OtherRacks.transform
            .GetChild(rackId);
        Destroy(exposePlayerRack.GetChild(1).GetChild(0).gameObject);
        MoveTile(tileId, exposePlayerRack.GetChild(0));
    }

    public void UnexposeOtherPlayerTile(int rackId, int tileId)
    {
        Transform exposePlayerRack = ObjRefs.OtherRacks.transform
            .GetChild(rackId);
        Instantiate(exposePlayerRack.GetChild(1).GetChild(0).gameObject, exposePlayerRack.GetChild(1));
        MoveTile(tileId, ObjRefs.TilePool.transform);
    }
}
