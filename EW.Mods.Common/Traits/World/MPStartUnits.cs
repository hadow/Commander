using System;
using System.Collections.Generic;
using EW.Traits;
namespace EW.Mods.Common.Traits
{

    /// <summary>
    /// Used by SpawnMPUnits
    /// </summary>
    public class MPStartUnitsInfo : TraitInfo<MPStartUnits>
    {
        /// <summary>
        /// Internal class Id;
        /// </summary>
        public readonly string Class = "none";

        public readonly string ClassName = "Unlabeled";

        public readonly HashSet<string> Factions = new HashSet<string>();

        /// <summary>
        /// The mobile construction vehicle
        /// </summary>
        public readonly string BaseActor = null;

        /// <summary>
        /// A group of units ready to defend or scout.
        ///一组准备防御或侦察的单位。
        /// </summary>
        public readonly string[] SupportActors = { };


        /// <summary>
        /// Inner radius for spawning support actors
        /// </summary>
        public readonly int InnerSupportRadius = 2;

        /// <summary>
        /// Outer radius for spawning support actors.
        /// </summary>
        public readonly int OuterSupportRadius = 4;

        /// <summary>
        /// Initial facing of BaseActor, -1 means random
        /// </summary>
        public readonly int BaseActorFacing = 128;

        /// <summary>
        /// Initial facing of SupportActors,-1 means random
        /// </summary>
        public readonly int SupportActorsFacing = -1;
    }

    public class MPStartUnits{}
}