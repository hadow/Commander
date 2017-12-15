using System;

namespace EW.Mods.Common.Traits
{


    public class CashTricklerInfo : ConditionalTraitInfo
    {
        public override object Create(ActorInitializer init)
        {
            return new CashTrickler(this);
        }
    }
    public class CashTrickler:ConditionalTrait<CashTricklerInfo>
    {
        public CashTrickler(CashTricklerInfo info) : base(info)
        {

        }

    }
}