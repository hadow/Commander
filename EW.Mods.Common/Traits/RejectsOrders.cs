using System;
using System.Collections.Generic;

using EW.Traits;
namespace EW.Mods.Common.Traits
{
    public class RejectsOrdersInfo : UpgradableTraitInfo
    {
        public override object Create(ActorInitializer init)
        {
            return new RejectsOrders(this);
        }
    }
    public class RejectsOrders : UpgradableTrait<RejectsOrdersInfo>
    {

        public RejectsOrders(RejectsOrdersInfo info) : base(info) { }
    }
}