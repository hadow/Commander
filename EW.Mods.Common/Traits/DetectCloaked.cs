using System;
using System.Collections.Generic;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    /// <summary>
    /// Actor can reveal Cloak actors in a specified range.
    /// </summary>
    public class DetectCloakedInfo : ConditionalTraitInfo
    {

        public readonly HashSet<string> CloakTypes = new HashSet<string> { "Cloak" };

        public readonly WDist Range = WDist.FromCells(5);


        public override object Create(ActorInitializer init)
        {
            return new DetectCloaked(this);
        }
    }
    public class DetectCloaked:ConditionalTrait<DetectCloakedInfo>
    {
        public DetectCloaked(DetectCloakedInfo info) : base(info) { }

    }
}