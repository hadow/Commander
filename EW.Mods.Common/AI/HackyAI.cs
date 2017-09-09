using System;
using System.Collections.Generic;
using System.Linq;
using EW.Traits;
namespace EW.Mods.Common.AI
{


    public sealed class HackyAIInfo : ITraitInfo
    {
        public object Create(ActorInitializer init) { return new HackyAI(this, init); }
    }

    public sealed class HackyAI:ITick
    {
        readonly Func<Actor, bool> isEnemyUnit;

        public readonly World World;
        public HackyAI(HackyAIInfo info,ActorInitializer init)
        {

        }

        public void Tick(Actor self)
        {

        }

        internal Actor FindClosestEnemy(WPos pos)
        {
            return World.Actors.Where(isEnemyUnit).ClosestTo(pos);
        }
    }
}