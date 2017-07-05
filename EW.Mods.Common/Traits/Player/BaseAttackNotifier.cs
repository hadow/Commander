using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    public class BaseAttackNotifierInfo : ITraitInfo
    {
        public object Create(ActorInitializer init) { return new BaseAttackNotifier(); }
    }
    class BaseAttackNotifier
    {
    }
}