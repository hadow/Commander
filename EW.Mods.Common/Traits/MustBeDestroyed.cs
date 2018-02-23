using System;
using System.Collections.Generic;

using EW.Traits;
namespace EW.Mods.Common.Traits
{
    [Desc("Actors with this trait must be destroyed  for a game to end.")]
    public class MustBeDestroyedInfo : ITraitInfo
    {
        public bool RequiredForShortGame = false;

        public object Create(ActorInitializer init)
        {
            return new MustBeDestroyed(this);
        }
    }

    public class MustBeDestroyed
    {

        public readonly MustBeDestroyedInfo Info;

        public MustBeDestroyed(MustBeDestroyedInfo info){
            Info = info;
        }
    }
}