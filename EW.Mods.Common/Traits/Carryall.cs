using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    public class CarryallInfo : ITraitInfo
    {
        public virtual object Create(ActorInitializer init)
        {
            return new Carryall();
        }
    }
    public class Carryall
    {
    }
}