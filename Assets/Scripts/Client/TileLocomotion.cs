using System.Linq;
using System.Collections.Generic;
using System;
using System.Diagnostics;

public class TileLocomotion
{
    readonly ClassReferences refs;
    readonly ObjectReferences objRefs;
    readonly GameManagerClient gameManagerClient;
    readonly FusionManager fusionManager;
    readonly TileLocomotionMono tileLocoMono;
    readonly int tileId;

    public TileLocomotion(ClassReferences refs, TileLocomotionMono tileLocoMono)
    {
        Debug.Assert(Tile.IsValidTileId(tileId));
        this.tileLocoMono = tileLocoMono;
        tileId = tileLocoMono.tileId;
        this.refs = refs;
        objRefs = ObjectReferences.Instance;
        gameManagerClient = refs.GManagerClient;
        fusionManager = refs.FManager;
    }

    public void OnPointerClick(bool doubleClick)
    {
        if (doubleClick)
        {
            if (fusionManager.GamePhase == GamePhase.Charleston)
            {
                DoubleClickCharleston();
                return;
            }
            if (fusionManager.GamePhase == GamePhase.Gameplay)
            {
                switch (fusionManager.TurnPhase)
                {
                    case TurnPhase.Exposing: // TODO: need to fall out of this to discard when applicable
                        DoubleClickExpose(); break;
                    case TurnPhase.Discarding:
                        DoubleClickDiscard(); break;
                    case TurnPhase.LoggingCallers:
                        break;
                    default:
                        break;
                }
            }
        }
    }

    public void DoubleClickCharleston() => refs.CClient.CharlestonTileMover(tileId);

    public void DoubleClickExpose() => refs.TManager.C_Expose(tileId);

    public void DoubleClickDiscard() => refs.TManager.C_Discard(tileId);

    public void OnEndDrag(List<MonoObject> raycastTargets, float xPos)
    {
        refs.Mono.SetRaycastTargetOnTile(tileId, true);
        if (TileIsOnTopOfRack(raycastTargets))
        {
            DropOnRack();
            return;
        }
        if (TileIsOnTopOfCharleston(raycastTargets)) 
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
        MoveBack();

        void DropOnRack()
        {
            Debug.Assert(fusionManager.GamePhase > GamePhase.Setup);

            float thisDist;
            float prevDist = float.MaxValue;
            int closestTileIndex = gameManagerClient.PrivateRack.Count - 1;
            int newIndex;

            /*
            subtract current position from the position of each tile starting
            from the left. this value will decrease and then start increasing.
            when it starts increasing, then the previous tile was the closest
            one. save off that index to closesttileindex.if the increase is
            never reached, then the tile should go to the end of the rack,
            which is why closesttileindex is initialized there.
            */
            for (int i = 0; i < gameManagerClient.PrivateRack.Count; i++)
            {
                thisDist = Math.Abs(xPos - tileLocoMono.TilePositions[i]);

                if (thisDist > prevDist)
                {
                    closestTileIndex = i - 1;
                    break;
                }
                prevDist = thisDist;
            }

            // now check if we're to the left or right of the tile and set accordingly.
            newIndex = xPos > tileLocoMono.TilePositions[closestTileIndex]
               ? closestTileIndex + 1 : closestTileIndex;
            // offset one if we're moving to the right.
            // TODO: verify this works as expected for moving within rack
            // TODO: verify this works as expected for moving from charleston box
            if (newIndex > gameManagerClient.PrivateRack.IndexOf(tileId)) { newIndex--; }

            gameManagerClient.PrivateRack.Remove(tileId);
            gameManagerClient.PrivateRack.Insert(newIndex, tileId);
            // tileLocoMono.MoveTile(MonoObject.PrivateRack, newIndex); // TODO: take care of this with eventmonitor
        }

        void DropOnCharleston()
        {
            MonoObject end = raycastTargets.First(target
                    => objRefs.CharlestonSpots.Contains(target));
            MonoObject start;

            if (refs.CClient.ClientPassArr.Contains(tileId))
            {
                start = MonoObject.CharlestonBox;
            }

            else if (gameManagerClient.PrivateRack.Contains(tileId))
            {
                start = MonoObject.PrivateRack;
            }

            else throw new Exception("invalid start position");

            refs.CClient.CharlestonTileMover(tileId, start, end);
        }

        void Discard() => refs.TManager.C_Discard(tileId);

        void Expose() => throw new NotImplementedException();

        void MoveBack() => tileLocoMono.MoveBack();
    }

    public bool TileIsOnTopOfRack(List<MonoObject> raycastTargets) =>
        raycastTargets.Contains(MonoObject.PrivateRack);

    bool TileIsOnTopOfCharleston()
    {
        return (fusionManager.GamePhase == GamePhase.Charleston);
    }

    bool TileIsOnTopOfCharleston(List<MonoObject> raycastTargets)
    {
        if (!TileIsOnTopOfCharleston()) return false;
        if (!raycastTargets.Any(target
            => objRefs.CharlestonSpots.Contains(target))) return false;
        return true;
    }

    public bool Discardable()
    {
        if (fusionManager.TurnPhase != TurnPhase.Discarding) return false;
        if (!gameManagerClient.IsActivePlayer) return false;
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
        if (fusionManager.TurnPhase != TurnPhase.Exposing) return false;
        if (!gameManagerClient.IsExposingPlayer) return false;
        return true;
    }

    bool Exposable(List<MonoObject> raycastTargets)
    {
        if (!Exposable()) return false;
        if (!raycastTargets.Contains(MonoObject.PublicRack)) return false;
        return true;
    }
}
