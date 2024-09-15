using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TileLocomotionMono : MonoBehaviour
    , IPointerClickHandler
    , ISelectHandler
    , IBeginDragHandler
    , IDragHandler
    , IEndDragHandler
    , ITileLocomotionMono
{
    public TileLocomotion tileLoco;
    ObjectReferences objRefs;
    ClassReferences classRefs;
    EventSystem ESystem;

    public Transform TileTF;
    Transform RackPrivateTF;
    Transform RackPublicTF;
    Transform OtherRacksTF;
    Transform DiscardTF;
    public int TileId { get; private set; }
    List<Transform> RebuildLayoutTransforms;
    public List<float> TilePositions
    {
        get
        {
            List<float> value = new();
            foreach (Transform childTf in RackPrivateTF)
            {
                value.Add(childTf.position.x);
            }
            return value;
        }
    }

    // lerp stuff
    bool Lerping = false;
    Vector3 StartPos;
    Vector3 EndPos;
    readonly float TotalLerpTime = 0.2f;
    float CurrentLerpTime = 0.2f;


    // FIXME: when dragging a tile on the rack, it moves smoothly from twice the distance it was dragged instead of from where it was dropped
    private void Start()
    {
        TileId = GetComponentInParent<TileMono>().tile.Id;
        objRefs = ObjectReferences.Instance;
        classRefs = objRefs.ClassRefs;
        tileLoco = new(classRefs, this);
        ESystem = objRefs.EventSystem.GetComponent<EventSystem>();
        TileTF = transform.parent;
        RackPrivateTF = objRefs.LocalRack.transform.GetChild(1);
        RackPublicTF = objRefs.LocalRack.transform.GetChild(0);
        OtherRacksTF = objRefs.OtherRacks.transform;
        DiscardTF = objRefs.Discard.transform;
        RebuildLayoutTransforms = new() { DiscardTF, RackPrivateTF, RackPublicTF };
        foreach (Transform rack in OtherRacksTF)
        {
            RebuildLayoutTransforms.Add(rack.GetChild(0));
        }
    }

    public void OnPointerClick(PointerEventData eventData) =>
        tileLoco.OnPointerClick(eventData.clickCount == 2);

    public void OnSelect(BaseEventData eventData) { }

    public void OnBeginDrag(PointerEventData eventData)
    {
        classRefs.Mono.SetRaycastTargetOnTile(TileId, false);
        transform.SetParent(objRefs.Dragging);
    }

    public void OnDrag(PointerEventData eventData) =>
        transform.position += (Vector3)eventData.delta;

    public void OnEndDrag(PointerEventData eventData)
    {
        transform.SetParent(TileTF);        // undo OnBeginDrag things

        List<RaycastResult> raycastResults = new();
        ESystem.RaycastAll(eventData, raycastResults);

        List<MonoObject> raycastTargets = new();
        int dropIx = -1;
        bool rightOfTile = false;

        // check for tile hits

        foreach (RaycastResult res in raycastResults)
        {
            // check for MonoObject hits
            if (objRefs.ReverseObjectDict.TryGetValue(
                res.gameObject.transform, out MonoObject obj))
            {
                raycastTargets.Add(obj);
            }

            // check for tile hits
            // FIXME: dropIx doesn't work if dropped perfectly between two tiles
            if (res.gameObject.transform.parent.CompareTag("Tile"))
            {
                Transform droppedOnTile = res.gameObject.transform.parent;
                rightOfTile = transform.position.x > droppedOnTile.position.x;
                dropIx = droppedOnTile.GetSiblingIndex();
            }
        }
        StringBuilder debugRes = new();
        foreach (var res in raycastTargets) debugRes.Append(res + ", ");

        tileLoco.OnEndDrag(raycastTargets, dropIx, rightOfTile);
    }

    public void MoveBack() =>
        MoveTile(TileTF.parent, TileTF.GetSiblingIndex());

    void MoveTile(MonoObject newParent, int newSibIx) =>
        MoveTile(objRefs.ObjectDict[newParent], newSibIx);

    public void MoveTile(Transform newParentTF, int newSibIx)
    {
        if (transform.IsChildOf(objRefs.TilePool.transform))
        {
            StartPos = new Vector3(100, 100, 100);
        }
        else { StartPos = transform.position; }
        EndPos = newParentTF.position;

        TileTF.SetParent(newParentTF);
        TileTF.SetSiblingIndex(newSibIx);
        TileTF.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 0);

        if (RebuildLayoutTransforms.Contains(newParentTF))
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)newParentTF);
            EndPos = TileTF.position;
        }

        else transform.parent.position = newParentTF.position;

        Lerping = true;
    }

    // overload without a sibling index. sends tile to last spot
    public void MoveTile(Transform newParent) =>
        MoveTile(newParent, newParent.childCount);

    // FIXME: you can discard multiple tiles during discard 2 sec

    void Update()
    {
        if (Lerping)
        {
            float t = CurrentLerpTime / TotalLerpTime;
            transform.position = Vector3.Lerp(StartPos, EndPos, t);

            CurrentLerpTime += Time.deltaTime;
            if (CurrentLerpTime > TotalLerpTime)
            {
                transform.position = transform.parent.position; // not EndPos because of weird rack shift bug
                CurrentLerpTime = 0f;
                Lerping = false;
            }
        }
    }
}
