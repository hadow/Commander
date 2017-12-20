using System;
using EW.Traits;
namespace EW.Mods.Common.Traits.Render
{
    public class WithDockedOverlayInfo:ITraitInfo
    {
        public object Create(ActorInitializer init)
        {
            return new WithDockedOverlay();
        }
    }

    public class WithDockedOverlay
    {

    }
}