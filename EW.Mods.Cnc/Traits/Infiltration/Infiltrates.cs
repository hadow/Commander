using System;
using EW.Traits;
using EW.Mods.Common.Traits;
namespace EW.Mods.Cnc.Traits
{
    public class InfiltratesInfo : ConditionalTraitInfo
    {
        public override object Create(ActorInitializer init)
        {
            return new Infiltrates();
        }
    }

    public class Infiltrates
    {

    }
}