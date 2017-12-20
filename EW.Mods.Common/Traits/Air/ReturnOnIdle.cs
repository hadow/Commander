using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    public class ReturnOnIdleInfo:ITraitInfo
    {

        public object Create(ActorInitializer init)
        {
            return new ReturnOnIdle();
        }
    }

    public class ReturnOnIdle
    {

    }
}