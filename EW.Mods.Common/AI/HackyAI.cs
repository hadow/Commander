using System;
using System.Collections.Generic;
using EW.Traits;
namespace EW.Mods.Common.AI
{


    public sealed class HackyAIInfo : ITraitInfo
    {
        public object Create(ActorInitializer init) { return new HackyAI(this, init); }
    }

    public sealed class HackyAI:ITick
    {

        public HackyAI(HackyAIInfo info,ActorInitializer init)
        {

        }

        public void Tick(Actor self)
        {

        }

    }
}