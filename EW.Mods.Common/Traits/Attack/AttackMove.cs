using System;
using System.Collections.Generic;

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