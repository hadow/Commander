using System;
namespace EW.Mods.Common.Traits
{

    public abstract class ConditionalTraitInfo:IConditionConsumerInfo{

        public abstract object Create(ActorInitializer init);
    }

    public class ConditionalTrait
    {
        public ConditionalTrait()
        {
        }
    }
}
