using System;
using System.Collections.Generic;

using EW.Traits;
namespace EW.Mods.Common.Traits
{

    public class ProductionQueueInfo : ITraitInfo
    {
        /// <summary>
        /// What kind of production will be added (e.g. Building,Infantry,Vehicle)
        /// </summary>
        [FieldLoader.Require]
        public readonly string Type = null;

        public readonly string Group = null;

        /// <summary>
        /// Only enable this queue for certain factions.
        /// </summary>
        public readonly HashSet<string> Factions = new HashSet<string>();

        public readonly int BuildDurationModifier = 100;



        public virtual object Create(ActorInitializer init) { return new ProductionQueue(init,init.Self,this); } 
    }
    public class ProductionQueue
    {


        public ProductionQueue(ActorInitializer init,Actor playerActor,ProductionQueueInfo info)
        {

        }
    }
}