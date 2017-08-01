using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{

    public abstract class ConditionalTraitInfo:IConditionConsumerInfo{

        public abstract object Create(ActorInitializer init);
    }

    public abstract class ConditionalTrait<T>:ISync where T:ConditionalTraitInfo
    {
        public ConditionalTrait(T info)
        {
        }
    }
}
