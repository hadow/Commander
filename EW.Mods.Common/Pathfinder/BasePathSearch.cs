using System;
using System.Collections.Generic;


namespace EW.Mods.Common.Pathfinder
{
    public interface IPathSearch : IDisposable
    {
        IGraph<CellInfo> Graph { get; }

        IEnumerable<KeyValuePair<CPos,int>> Considered { get; }

        Player Owner { get; }

        IPathSearch Reverse();

        IPathSearch WithCustomBlocker(Func<CPos, bool> customBlock);

        IPathSearch WithIgnoredActor(Actor b);

        IPathSearch WithHeuristic(Func<CPos, int> h);

        IPathSearch WithCustomCost(Func<CPos, int> w);

        IPathSearch WithoutLaneBias();

        IPathSearch FromPoint(CPos from);

        bool IsTarget(CPos location);

        bool CanExpand { get; }

        CPos Expand();
    }
    public abstract class BasePathSearch:IPathSearch
    {

        protected BasePathSearch(IGraph<CellInfo> graph)
        {

        }
    }
}