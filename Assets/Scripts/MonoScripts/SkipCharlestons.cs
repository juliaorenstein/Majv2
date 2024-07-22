using UnityEngine;
using UnityEngine.EventSystems;

public class SkipCharlestons : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        ObjectReferences.Instance.ClassRefs.CClient.UpdateButton(-1);
        gameObject.SetActive(false);
    }
}
