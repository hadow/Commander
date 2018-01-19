using System;
using EW.Traits;
using System.Collections.Generic;
using System.Linq;
using EW.Graphics;

namespace EW.Mods.Common.Traits
{

    public sealed class ResourceClaimLayerInfo : TraitInfo<ResourceClaimLayer>{}


    public sealed class ResourceClaimLayer
    {
        readonly Dictionary<CPos, List<Actor>> claimByCell = new Dictionary<CPos, List<Actor>>(32);
        readonly Dictionary<Actor, CPos> claimByActor = new Dictionary<Actor, CPos>(32);


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





    }
}