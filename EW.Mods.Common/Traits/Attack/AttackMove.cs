using System;
using System.Collections.Generic;
using EW.Traits;
namespace EW.Mods.Common.Traits
{

    class AttackMoveInfo : ITraitInfo
    {
        public object Create(ActorInitializer init)
        {
            return new AttackMove();
        }
    }
    class AttackMove
    {
    }
}