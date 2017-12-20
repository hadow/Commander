using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    class BridgePlaceholderInfo:ITraitInfo
    {

        public object Create(ActorInitializer init)
        {
            return new BridgePlaceholder();

        }
    }

    class BridgePlaceholder
    {

    }
}