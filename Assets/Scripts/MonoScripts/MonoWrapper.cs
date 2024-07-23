using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;


public class MonoWrapper : MonoBehaviour, IMonoWrapper
{
    ObjectReferences objRefs;

    TextMeshProUGUI TurnIndicatorText;

    private void Awake()
    {
        objRefs = ObjectReferences.Instance;
        objRefs.ClassRefs.Mono = this;
    }

    private void Start()
    {
        TurnIndicatorText = objRefs.TurnIndicator.transform.GetChild(0)
            .GetComponent<TextMeshProUGUI>();
    }

    public void SetActive(MonoObject monoObject, bool value)
    {
        objRefs.ObjectDict[monoObject].gameObject.SetActive(value);
    }

    public void SetRaycastTarget(MonoObject monoObject, bool value)
    {
        objRefs.ObjectDict[monoObject].transform.GetComponent<Image>().raycastTarget = value;
    }

    public void SetRaycastTargetOnTile(int tileId, bool value)
    {
        Debug.Assert(Tile.IsValidTileId(tileId));
        GameManager.TileList[tileId].tileMono
                   .GetComponentInChildren<Image>()
                   .raycastTarget = value;
    }

    public bool IsButtonInteractable(MonoObject monoObject)
    {
        Button button = objRefs.ObjectDict[monoObject].GetComponent<Button>();
        Debug.Assert(button != null);
        return button.IsInteractable();
    }

    public void SetButtonInteractable(MonoObject monoObject, bool value)
    {
        Button button = objRefs.ObjectDict[monoObject].GetComponent<Button>();
        Debug.Assert(button != null);
        objRefs.ObjectDict[monoObject].GetComponent<Button>().interactable = value;
    }

    public void MoveTile(int tileId, MonoObject destination)
    {
        MoveTile(tileId, objRefs.ObjectDict[destination].transform);
    }

    private void MoveTile(int tileId, Transform destination)
    {
        Debug.Assert(Tile.IsValidTileId(tileId));
        GameManager.TileList[tileId].tileMono
                   .GetComponentInChildren<TileLocomotionMono>()
                   .MoveTile(destination);
    }

    public void UpdateRack(List<int> tileIds)
    {
        List<int> currentTileIds = objRefs.LocalRack
            .GetComponentsInChildren<TileMono>()
            .Select(tileMono => tileMono.tile.Id).ToList();
        // remove old tiles
        foreach (int tileId in currentTileIds)
        {
            if (!tileIds.Contains(tileId)) MoveTile(tileId, objRefs.TilePool);
        }

        foreach (int tileId in tileIds)
        {
            if (!currentTileIds.Contains(tileId)) MoveTile(tileId, objRefs.LocalRack.GetChild(1));
        }
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
        Button button = objRefs.ObjectDict[monoObject].GetComponent<Button>();
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
        Transform exposePlayerRack = objRefs.OtherRacks.transform
            .GetChild(rackId);
        Destroy(exposePlayerRack.GetChild(1).GetChild(0).gameObject);
        MoveTile(tileId, exposePlayerRack.GetChild(0));
    }

    public void UnexposeOtherPlayerTile(int rackId, int tileId)
    {
        Transform exposePlayerRack = objRefs.OtherRacks.transform
            .GetChild(rackId);
        Instantiate(exposePlayerRack.GetChild(1).GetChild(0).gameObject, exposePlayerRack.GetChild(1));
        MoveTile(tileId, objRefs.TilePool.transform);
    }
}
