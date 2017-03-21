using System;
using System.Collections.Generic;
using RA.Graphics;

namespace RA
{
    class TraitsInterfaces
    {
    }


    public interface ITraitInfoInterface { }

    public interface Requires<T> where T : class, ITraitInfoInterface { }

    public interface ITraitInfo : ITraitInfoInterface
    {
        object Create(ActorInitializer init);
    }


    public interface IRulesetLoaded<TInfo> { void RulesetLoaded(Ruleset rules, TInfo info); }

    public interface IRulesetLoaded : IRulesetLoaded<ActorInfo>, ITraitInfoInterface { }


    public interface ITick { void Tick(Actor self); }


    public interface IWorldLoaded { void WorldLoaded(World w,WorldRenderer render); }
}