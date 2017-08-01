using System;
using EW.Traits;
namespace EW.Mods.Common.Traits.Render
{
    public class WithDockingAnimationInfo : TraitInfo<WithDockingAnimation>, Requires<WithSpriteBodyInfo>, Requires<HarvesterInfo>
    {

    }
    public class WithDockingAnimation
    {
    }
}