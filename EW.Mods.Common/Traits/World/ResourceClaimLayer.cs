using System;
using EW.Traits;
using System.Collections.Generic;
using System.Linq;
using EW.Graphics;

namespace EW.Mods.Common.Traits
{

    public sealed class ResourceClaimLayerInfo : TraitInfo<ResourceClaimLayer>{}

    /// <summary>
    /// 资源收割
    /// </summary>
    public sealed class ResourceClaimLayer
    {
        readonly Dictionary<CPos, List<Actor>> claimByCell = new Dictionary<CPos, List<Actor>>(32);
        readonly Dictionary<Actor, CPos> claimByActor = new Dictionary<Actor, CPos>(32);

        /// <summary>
        /// Attempt to reserve the resource in a cell for the given actor.
        /// </summary>
        /// <param name="claimer"></param>
        /// <param name="cell"></param>
        /// <returns></returns>
        public bool TryClaimCell(Actor claimer,CPos cell)
        {
            var claimers = claimByCell.GetOrAdd(cell);

            //Clean up any stale claims
            claimers.RemoveAll(a => a.IsDead);

            //Prevent harvesters from the player or their allies fighting over the same cell
            //防止玩家或其盟友的收割者在同一个单元格内争夺资源
            if (claimers.Any(c => c != claimer && claimer.Owner.IsAlliedWith(c.Owner)))
                return false;

            //Remove the actor's last claim,if it has one
            CPos lastClaim;
            if (claimByActor.TryGetValue(claimer, out lastClaim))
                claimByCell.GetOrAdd(lastClaim).Remove(claimer);

            claimers.Add(claimer);
            claimByActor[claimer] = cell;
            return true;
        }


        /// <summary>
        /// Returns false if the cell is already reserved by an allied actor.
        /// </summary>
        /// <param name="claimer"></param>
        /// <param name="cell"></param>
        /// <returns></returns>
        public bool CanClaimCell(Actor claimer,CPos cell)
        {
            return !claimByCell.GetOrAdd(cell).Any(c => c != claimer && !c.IsDead && claimer.Owner.IsAlliedWith(c.Owner));
        }

        public void RemoveClaim(Actor claimer)
        {
            CPos lastClaim;
            if (claimByActor.TryGetValue(claimer, out lastClaim))
                claimByCell.GetOrAdd(lastClaim).Remove(claimer);

            claimByActor.Remove(claimer);

        }



    }
}