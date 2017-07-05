using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    public class PowerManagerInfo : ITraitInfo
    {
        public object Create(ActorInitializer init) { return new PowerManager(); }
    }
    class PowerManager
    {
    }
}