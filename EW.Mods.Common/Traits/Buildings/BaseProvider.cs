using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{

    public class BaseProviderInfo : ITraitInfo
    {
        public object Create(ActorInitializer init) { return new BaseProvider(); }

    }
    public class BaseProvider
    {
    }
}