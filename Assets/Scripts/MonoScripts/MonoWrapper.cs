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
        Tile.TileList[tileId].tileMono
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

    public void MoveTile(int tileId, MonoObject destination, int pos = -1)
    {
        // Move to non-rack destination or to end of rack
        if (pos == -1)
        {
            MoveTile(tileId, objRefs.ObjectDict[destination].transform);
            return;
        }
        // Move to rack - specific position
        MoveTileToRack(tileId, pos);
    }

    private void MoveTile(int tileId, Transform destination)
    {
        Debug.Assert(Tile.IsValidTileId(tileId));
        Tile.TileList[tileId].tileMono
                   .GetComponentInChildren<TileLocomotionMono>()
                   .MoveTile(destination);
    }

    public void MoveTileToRack(int tileId, int pos)
    {
        Debug.Assert(Tile.IsValidTileId(tileId));
        Debug.Assert(pos > -1 && pos < 14);
        Tile.TileList[tileId].tileMono
                   .GetComponentInChildren<TileLocomotionMono>()
                   .MoveTile(objRefs.PrivateRack, pos);
    }

    // TODO: Spaghetti between TileTrackerClient, TileLocoMono, and MonoWrapper
    public void UpdateRack(List<int> newTileIds)
    {
        // put newTileIds on rack in that order
        for (int i = 0; i < newTileIds.Count; i++)
        {
            MoveTileToRack(newTileIds[i], i);
        }

        // remove any extra tiles
        for (int i = newTileIds.Count; i < objRefs.PrivateRack.childCount; i++)
        {
            int tileId = objRefs.PrivateRack.GetChild(i).GetComponent<TileMono>().tile.Id;
            MoveTile(tileId, MonoObject.TilePool);
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
