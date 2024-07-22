using UnityEngine;
using UnityEngine.EventSystems;

public class CharlestonMono : MonoBehaviour
    , IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData) =>
    ObjectReferences.Instance.ClassRefs.CClient.InitiatePass();
}
