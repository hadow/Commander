using System;
using System.Collections.Generic;
using EW.Traits;

namespace EW.Mods.Common.Traits
{
    /// <summary>
    /// 盔甲
    /// Used to define weapon efficiency modifiers with different percentages per Type.
    /// </summary>
    public class ArmorInfo : ConditionalTraitInfo
    {
        public readonly string Type = null;
        public override object Create(ActorInitializer init)
        {
            return new Armor(init.Self, this);
        }
    }
    public class Armor:ConditionalTrait<ArmorInfo>
    {
        public Armor(Actor self,ArmorInfo info):base(info){}
    }
}