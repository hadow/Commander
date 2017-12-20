using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    public class ExplosionOnDamageTransitionInfo:ITraitInfo
    {

        public object Create(ActorInitializer init)
        {
            return new ExplosionOnDamageTransition();
        }
    }


    public class ExplosionOnDamageTransition
    {

    }
}