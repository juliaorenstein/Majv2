using UnityEngine;
using UnityEngine.EventSystems;

public class SkipCharlestons : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        gameObject.SetActive(false);
        CharlestonFusion cFusion = (CharlestonFusion)ObjectReferences.Instance.ClassRefs.CFusion;
        cFusion.RPC_H2A_SkipCharlestons();
    }
}
