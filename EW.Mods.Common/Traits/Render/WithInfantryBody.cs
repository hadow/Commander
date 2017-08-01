using System;
using System.Collections.Generic;
using EW.Traits;
namespace EW.Mods.Common.Traits
{


    public class WithInfantryBodyInfo : UpgradableTraitInfo
    {
        public override object Create(ActorInitializer init)
        {
            return new WithInfantryBody(init, this);
        }
    }
    public class WithInfantryBody:UpgradableTrait<WithInfantryBodyInfo>
    {

        public WithInfantryBody(ActorInitializer init,WithInfantryBodyInfo info) : base(info) { }
    }
}