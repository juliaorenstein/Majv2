using System.Linq;
using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;

public class TileLocomotion
{
    readonly ClassReferences refs;
    IFusionManager FusionManager
    {
        get => refs.FManager;
    }
    readonly ITileLocomotionMono tileLocoMono;
    readonly int tileId;

    // initiated in tileLocoMono
    public TileLocomotion(ClassReferences refs, ITileLocomotionMono tileLocoMono)
    {
        UnityEngine.Debug.Assert(Tile.IsValidTileId(tileId));
        this.tileLocoMono = tileLocoMono;
        tileId = tileLocoMono.TileId;
        this.refs = refs;
    }

    public void OnPointerClick(bool doubleClick)
    {
        if (!doubleClick) return;

        if (FusionManager.GamePhase == GamePhase.Charleston)
        {
            // don't fuck with jokers during charlestons
            if (Tile.IsJoker(tileId)) return;

            DoubleClickCharleston();
            return;
        }

        if (FusionManager.GamePhase == GamePhase.Gameplay)
        {
            if (Exposable())
            {
                Expose();
                return;
            }
            if (Discardable())
            {
                Discard();
                return;
            }

            switch (FusionManager.TurnPhase)
            {
                case TurnPhase.Exposing:
                    Expose(); break; // FIXME: deal with discard during expose
                case TurnPhase.Discarding:
                    Discard(); break;
                case TurnPhase.LoggingCallers:
                    break;
                default:
                    break;
            }
        }

        throw new Exception("This tile probably should not have been raycast enabled.");
    }

    public void DoubleClickCharleston() => refs.CClient.DoubleClickCharlestonTileMover(tileId);

    public void DoubleClickDiscard() => refs.TManagerClient.RequestDiscard(tileId);

    public void OnEndDrag(List<MonoObject> raycastTargets, int dropIx = -1, bool rightOfTile = false)
    {
        refs.Mono.SetRaycastTargetOnTile(tileId, true);
        if (TileIsOnTopOfRack(raycastTargets))
        {
            DropOnRack();
            return;
        }
        if (Charlestonable(raycastTargets))
        {
            DropOnCharleston();
            return;
        }
        if (Discardable(raycastTargets))
        {
            Discard();
            return;
        }
        if (Exposable(raycastTargets))
        {
            Expose();
            return;
        }

        // if none of the above worked, move tile back to where it came from
        tileLocoMono.MoveBack();

        void DropOnRack()
        {
            UnityEngine.Debug.Assert(FusionManager.GamePhase > GamePhase.Pregame);

            List<int> rack = refs.TileTrackerClient.LocalPrivateRack;

            int curIxOnRack = rack.ToList().IndexOf(tileId);
            bool comingFromCharles = refs.CClient.ClientPassArr.Contains(tileId);

            int newIx = dropIx;
            if (!comingFromCharles)
            {
                // if dropped to the right of all other tiles, just add on the end
                if (dropIx == -1)
                {
                    rack.Remove(tileId);
                    rack.Add(tileId);
                    return;
                }
                if (curIxOnRack < dropIx) newIx--; // moving right - decrease final index
            }
            if (rightOfTile) newIx++; // dropped to the right of the center of the tile - increase final index

            // assumes only two cases are rack to rack and charleston to rack. not sure if others will come up
            if (curIxOnRack < 0) refs.CClient.MoveTileFromCharlestonToRack(tileId, newIx);
            else
            {
                refs.TileTrackerClient.LocalPrivateRack.Remove(tileId);
                refs.TileTrackerClient.LocalPrivateRack.Insert(newIx, tileId);
            }
        }

        // TODO: if using logging in the future, this class would be a good place to start

        void DropOnCharleston()
        {
            UnityEngine.Debug.Assert(!Tile.IsJoker(tileId));

            MonoObject start;
            MonoObject end = raycastTargets.First(target
                    => refs.CClient.CharlestonSpots.Contains(target));

            if (refs.CClient.ClientPassArr.Contains(tileId))
            {
                start = refs.CClient.CharlestonSpots[Array.IndexOf(refs.CClient.ClientPassArr, tileId)];
            }

            else if (refs.TileTrackerClient.LocalPrivateRack.Contains(tileId))
            {
                start = MonoObject.PrivateRack;
            }

            else throw new Exception("invalid start position");

            refs.CClient.DragCharlestonTileMover(tileId, start, end);
        }
    }

    public bool TileIsOnTopOfRack(List<MonoObject> raycastTargets) =>
        raycastTargets.Contains(MonoObject.PrivateRack);

    bool Charlestonable()
    {
        if (Tile.IsJoker(tileId)) return false;
        return FusionManager.GamePhase == GamePhase.Charleston;
    }

    bool Charlestonable(List<MonoObject> raycastTargets)
    {
        if (!Charlestonable()) return false;
        if (!raycastTargets.Any(target
            => refs.CClient.CharlestonSpots.Contains(target))) return false;
        return true;
    }

    public bool Discardable()
    {
        if (FusionManager.TurnPhase != TurnPhase.Discarding) return false; // FIXME: deal with expose turn discards
        if (!refs.FManager.IsActivePlayer) return false;
        return true;
    }

    bool Discardable(List<MonoObject> raycastTargets)
    {
        if (!Discardable()) return false;
        if (!raycastTargets.Contains(MonoObject.Discard)) return false;
        return true;
    }

    public bool Exposable()
    {
        if (FusionManager.TurnPhase != TurnPhase.Exposing) return false;
        if (!refs.FManager.IsExposingPlayer) return false;
        if (!Tile.TileList[tileId].Equals(refs.TManagerServer.LastDiscarded)) return false;
        return true;
    }

    bool Exposable(List<MonoObject> raycastTargets)
    {
        if (!Exposable()) return false;
        if (!raycastTargets.Contains(MonoObject.PublicRack)) return false;
        return true;
    }

    public void Discard() => refs.TManagerClient.RequestDiscard(tileId);

    public void Expose() => throw new NotImplementedException();
}
