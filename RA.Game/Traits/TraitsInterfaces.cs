using System;
using System.Collections.Generic;
using RA.Game.Graphics;

namespace RA.Game
{
    class TraitsInterfaces
    {
    }


    public interface ITraitInfoInterface { }

    public interface Requires<T> where T : class, ITraitInfoInterface { }

    public interface ITraitInfo : ITraitInfoInterface
    {
        object Create();
    }


    public interface ITick { void Tick(Actor self); }


    public interface IWorldLoaded { void WorldLoaded(World w,WorldRenderer render); }
}