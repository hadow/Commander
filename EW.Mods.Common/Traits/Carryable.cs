using System;
using EW.Traits;

namespace EW.Mods.Common.Traits
{
    public class CarryableInfo:ConditionalTraitInfo
    {
        public override object Create(ActorInitializer init)
        {
            return new Carryable(init.Self,this);
        }

    }

    public class Carryable : ConditionalTrait<CarryableInfo>
    {
        public Carryable(Actor self,CarryableInfo info) : base(info) { }
    }
}