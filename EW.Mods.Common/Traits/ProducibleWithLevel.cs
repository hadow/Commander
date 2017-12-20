using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    public class ProducibleWithLevelInfo:ITraitInfo
    {

        public object Create(ActorInitializer init)
        {
            return new ProducibleWithLevel();
        }
    }

    public class ProducibleWithLevel
    {

    }
}