using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    public class ProduceActorPowerInfo:SupportPowerInfo
    {

        public override object Create(ActorInitializer init)
        {
            return new ProduceActorPower();
        }
    }

    public class ProduceActorPower
    {

    }
}