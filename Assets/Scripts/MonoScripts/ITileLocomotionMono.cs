using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public interface ITileLocomotionMono
{
    int TileId { get; }
    List<float> TilePositions { get; }

    void MoveBack();
    void MoveTile(Transform newParent);
    void OnBeginDrag(PointerEventData eventData);
    void OnDrag(PointerEventData eventData);
    void OnEndDrag(PointerEventData eventData);
    void OnPointerClick(PointerEventData eventData);
    void OnSelect(BaseEventData eventData);
}