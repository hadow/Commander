using System.Collections.Generic;
using EW.Traits;

namespace EW.Mods.Common.Traits
{
    /// <summary>
    /// Specifies the target types and relative priority used by AutoTarget to decide what to target.
    /// </summary>
    public class AutoTargetPriorityInfo : ConditionalTraitInfo, Requires<AutoTargetInfo>
    {

        public readonly HashSet<string> ValidTargets = new HashSet<string> { "Ground", "Water", "Air" };

        public readonly HashSet<string> InvalidTargets = new HashSet<string>();

        public readonly int Priority = 1;
        
        public override object Create(ActorInitializer init)
        {
            return new AutoTargetPriority(this);
        }
    }

    public class AutoTargetPriority : ConditionalTrait<AutoTargetPriorityInfo>
    {
        public AutoTargetPriority(AutoTargetPriorityInfo info) : base(info) { }
    }
}