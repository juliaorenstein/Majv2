using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

public class Navigation : MonoBehaviour
{
    public ObjectReferences ObjRefs;
    private EventSystem ESystem;
    private Transform RackPrivate;
    private Transform RackPublic;
    private Transform Selected;
    private Transform Charleston;
    private Transform Discard;
    private GameObject WaitButton;
    private GameObject PassButton;
    private GameObject CallButton;
    private GameObject NeverMindButton;
    private NetworkCallbacks NCallbacks;

    private void Start()
    {
        ObjRefs = ObjectReferences.Instance;
        ObjRefs.ClassRefs.Nav = this;
        RackPrivate = ObjRefs.LocalRack.transform.GetChild(1);
        RackPublic = ObjRefs.LocalRack.transform.GetChild(0);
        Charleston = ObjRefs.CharlestonBox;
        Discard = ObjRefs.Discard;
        WaitButton = ObjRefs.CallWaitButtons.transform.GetChild(0).gameObject;
        PassButton = ObjRefs.CallWaitButtons.transform.GetChild(1).gameObject;
        CallButton = ObjRefs.CallWaitButtons.transform.GetChild(2).gameObject;
        NeverMindButton = ObjRefs.CallWaitButtons.transform.GetChild(3).gameObject;
        NCallbacks = ObjRefs.ClassRefs.NetworkCallbacks;
        ESystem = EventSystem.current;
    }

    private void Update()
    {
        if (!NCallbacks) return;
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (!Selected)
            {
                Select(RackPrivate.GetChild(0));
                return;
            }

            if (Selected.IsChildOf(RackPrivate))
            {
                int ix = Selected.GetSiblingIndex() + 1;
                if (ix == RackPrivate.childCount) { ix = 0; }

                // if shift is down, move tile. if not, change selection
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                { Selected.SetSiblingIndex(ix); }
                else { Select(RackPrivate.GetChild(ix)); }
                return;
            }

            if (Selected.IsChildOf(Charleston))
            {
                TileMono SelectedTile = Selected.GetComponent<TileMono>();
                TileMono[] tilesInCharleston = Charleston.GetComponentsInChildren<TileMono>();
                if (tilesInCharleston[^1] != SelectedTile)
                {
                    int curIx = Array.IndexOf(tilesInCharleston, SelectedTile);
                    Select(tilesInCharleston[curIx + 1].transform);
                }
                return;
            }
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (!Selected)
            {
                Select(RackPrivate.GetChild(0));
                return;
            }

            if (Selected.IsChildOf(RackPrivate))
            {
                int ix = Selected.GetSiblingIndex() - 1;
                if (ix < 0) { ix = RackPrivate.childCount - 1; }

                // if shift is down, move tile. if not, change selection
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                { Selected.SetSiblingIndex(ix); }
                else { Select(RackPrivate.GetChild(ix)); }
                return;
            }

            if (Selected.IsChildOf(Charleston))
            {
                TileMono SelectedTile = Selected.GetComponent<TileMono>();
                TileMono[] tilesInCharleston = Charleston.GetComponentsInChildren<TileMono>();
                if (tilesInCharleston[0] != SelectedTile)
                {
                    int curIx = Array.IndexOf(tilesInCharleston, SelectedTile);
                    Select(tilesInCharleston[curIx - 1].transform);
                }
                return;
            }
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (!Selected)
            {
                Select(RackPrivate.GetChild(0));
                return;
            }

            if (Selected.IsChildOf(Charleston)
                || Selected.IsChildOf(RackPublic))
            {
                Select(RackPrivate.GetChild(0));
                return;
            }
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (!Selected)
            {
                Select(RackPrivate.GetChild(0));
                return;
            }

            if (Selected.IsChildOf(RackPrivate))
            {
                if (Charleston.gameObject.activeInHierarchy)
                {

                    TileMono charlestonTile = Charleston.GetComponentInChildren<TileMono>();
                    if (charlestonTile)
                    {
                        Select(charlestonTile.transform);
                        return;
                    }
                }

                else
                {
                    TileMono exposedTile = RackPublic.GetComponentInChildren<TileMono>();
                    if (exposedTile)
                    {
                        Select(exposedTile.transform);
                        return;
                    }
                }
            }


        }

        if (Input.GetKeyDown(KeyCode.Space))
        {

            if (WaitButton.activeInHierarchy)
            {
                InputWait();
                return;
            }

            if (PassButton.activeInHierarchy)
            {
                InputPass();
                return;
            }

            if (!Selected)
            {
                Select(RackPrivate.GetChild(0));
                return;
            }

            if (Charleston.gameObject.activeInHierarchy)
            {
                Selected.GetComponentInChildren<TileLocomotionMono>().tileLoco.DoubleClickCharleston();
                return;
            }

            if (Selected.GetComponentInChildren<TileLocomotionMono>().tileLoco.Discardable())
            {
                Selected.GetComponentInChildren<TileLocomotionMono>().tileLoco.DoubleClickDiscard();
                Unselect();
                return;
            }

            if (Selected.GetComponentInChildren<TileLocomotionMono>()
                .tileLoco.Exposable())
            {
                Selected.GetComponentInChildren<TileLocomotionMono>()
                    .tileLoco.Expose();
                return;
            }
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (Charleston.gameObject.activeInHierarchy)
            {
                ObjRefs.ClassRefs.CClient.InitiatePass();
                Unselect();
                return;
            }

            if (CallButton.activeInHierarchy)
            {
                InputCall();
                return;
            }
        }

        if (ESystem.currentSelectedGameObject == WaitButton)
        {
            InputWait();
            return;
        }

        if (ESystem.currentSelectedGameObject == PassButton)
        {
            InputPass();
            return;
        }

        if (ESystem.currentSelectedGameObject == CallButton)
        {
            InputCall();
            return;
        }

        if (ESystem.currentSelectedGameObject == NeverMindButton)
        {
            InputNeverMind();
            return;
        }
        // TODO: test space bar to expose
    }

    public void Select(Transform tileTF)
    {
        if (Selected)
        {
            Selected.GetChild(0).GetChild(0).gameObject.SetActive(false); // unhighlight previous
        }
        ESystem.SetSelectedGameObject(tileTF.gameObject);
        tileTF.GetChild(0).GetChild(0).gameObject.SetActive(true); // highlight current
        Selected = tileTF;
    }

    public void Unselect()
    {
        if (Selected)
        {
            Selected.GetChild(0).GetChild(0).gameObject.SetActive(false); // unhighlight previous
        }
        ESystem.SetSelectedGameObject(null);
        Selected = null;
    }

    void InputWait()
    {
        NCallbacks.inputStruct.turnOptions.Set(Buttons.wait, true);
        WaitButton.SetActive(false);
        PassButton.SetActive(true);
        Unselect(); // in if statement to avoid unselecting unrelated things
    }

    void InputPass()
    {
        NCallbacks.inputStruct.turnOptions.Set(Buttons.pass, true);
        PassButton.SetActive(false);
        WaitButton.SetActive(true);
        CallButton.transform.parent.gameObject.SetActive(false);
        Unselect();
    }

    void InputCall()
    {
        NCallbacks.inputStruct.turnOptions.Set(Buttons.call, true);
        Unselect();
    }

    void InputNeverMind()
    {
        NCallbacks.inputStruct.turnOptions.Set(Buttons.nevermind, true);
        // TODO: not sure yet if these are all needed. Clear once tested
        PassButton.SetActive(false);
        WaitButton.SetActive(true);
        CallButton.transform.parent.gameObject.SetActive(false);
        Unselect();
    }

    public void SetNetworkCallbacks(NetworkCallbacks networkCallbacks)
    {
        NCallbacks = networkCallbacks;
    }
}
