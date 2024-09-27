using System.Collections.Generic;
using System.Collections;
using System;

class FakeMonoWrapper : IMonoWrapper
{
    public FakeMonoWrapper(ClassReferences refs)
    {
        refs.Mono = this;
    }

    public void ExposeOtherPlayerTile(int rackId, int tileId)
    {
        UnityEngine.Debug.Log("FakeMonoWrapper.ExposeOtherPlayertile()");
    }

    public Dictionary<int, MonoObject> TileLocations = new();
    public void MoveTile(int tileId, MonoObject destination, int post = -1)
    {
        TileLocations[tileId] = destination;
    }

    public List<MonoObject> ActiveList = new();
    public void SetActive(MonoObject monoObject, bool value)
    {
        if (value) ActiveList.Add(monoObject);
        else ActiveList.Remove(monoObject);
    }

    public bool IsButtonInteractableValue { get; private set; }
    public bool IsButtonInteractable(MonoObject monoObject)
    {
        return IsButtonInteractableValue;
    }

    public void SetButtonInteractable(MonoObject monoObject, bool value)
    {
        IsButtonInteractableValue = value;
    }

    public string ButtonText;
    public void SetButtonText(MonoObject monoObject, string text)
    {
        ButtonText = text;
    }

    public List<MonoObject> RaycastTargetList = new();
    public void SetRaycastTarget(MonoObject monoObject, bool value)
    {
        if (value) RaycastTargetList.Add(monoObject);
        else RaycastTargetList.Remove(monoObject);
    }

    public List<int> TileRaycastTargetList = new();
    public void SetRaycastTargetOnTile(int tileId, bool value)
    {
        if (value) TileRaycastTargetList.Add(tileId);
        else TileRaycastTargetList.Remove(tileId);
    }

    public void SetTurnIndicatorText(int playerId)
    {
        throw new System.NotImplementedException();
    }

    public void StartNewCoroutine(IEnumerator func)
    {
        throw new System.NotImplementedException();
    }

    public void UnexposeOtherPlayerTile(int rackId, int tileId)
    {
        throw new System.NotImplementedException();
    }

    public IEnumerator WaitForSeconds(int seconds)
    {
        yield return new();
    }

    public void UpdateRack(List<int> tileIds)
    {
        UnityEngine.Debug.Log($"FakeMonoWrapper.UpdateRack([{string.Join(", ", tileIds)}])");
    }
}