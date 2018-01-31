using System;
using EW.Traits;
namespace EW.Mods.Common.Traits.Render
{
    public class WithDockingAnimationInfo : TraitInfo<WithDockingAnimation>, Requires<WithSpriteBodyInfo>, Requires<HarvesterInfo>
    {
        /// <summary>
        /// Displayed when docking to refinery
        /// </summary>
        [SequenceReference]
        public readonly string DockSequence = "dock";

        /// <summary>
        /// Looped while unloading at refinery.
        /// </summary>
        [SequenceReference]
        public readonly string DockLoopSequence = "dock-loop";
    }
    public class WithDockingAnimation{ }
}