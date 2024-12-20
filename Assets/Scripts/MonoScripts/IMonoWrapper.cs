﻿using System.Collections;
using System.Collections.Generic;

public interface IMonoWrapper
{
    void ExposeOtherPlayerTile(int rackId, int tileId);
    bool IsButtonInteractable(MonoObject monoObject);
    void MoveTile(int tileId, MonoObject destination, int pos = -1);
    void UpdateRack(List<int> tileIds);
    void SetActive(MonoObject monoObject, bool value);
    void SetButtonInteractable(MonoObject monoObject, bool value);
    void SetButtonText(MonoObject monoObject, string text);
    void SetRaycastTarget(MonoObject monoObject, bool value);
    void SetRaycastTargetOnTile(int tileId, bool value);
    void SetTurnIndicatorText(int playerId);
    void StartNewCoroutine(IEnumerator func);
    void UnexposeOtherPlayerTile(int rackId, int tileId);
    IEnumerator WaitForSeconds(int seconds);
}