using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{

    class SeedsResourceInfo : ConditionalTraitInfo
    {
        public override object Create(ActorInitializer init)
        {
            return new SeedsResource(init.Self, this);
        }
    }
    class SeedsResource:ConditionalTrait<SeedsResourceInfo>
    {


        public SeedsResource(Actor self,SeedsResourceInfo info) : base(info) { }
    }

    
}