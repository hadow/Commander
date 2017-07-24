﻿using System;
using System.Collections.Generic;
using EW.Primitives;

namespace EW.Mods.Common.Pathfinder
{
    public interface IPathSearch : IDisposable
    {
        IGraph<CellInfo> Graph { get; }

        IEnumerable<Pair<CPos,int>> Considered { get; }

        Player Owner { get; }

        IPathSearch Reverse();

        IPathSearch WithCustomBlocker(Func<CPos, bool> customBlock);

        IPathSearch WithIgnoredActor(Actor a);

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
        public IGraph<CellInfo> Graph { get; set; }

        public abstract IEnumerable<Pair<CPos,int>> Considered { get; }

        protected IPriorityQueue<GraphConnection> OpenQueue { get; private set; }

        protected readonly IPriorityQueue<GraphConnection> StartPoints;

        public Player Owner { get { return Graph.Actor.Owner; } }

        public int MaxCost { get; private set; }
        protected BasePathSearch(IGraph<CellInfo> graph)
        {
            Graph = graph;
            OpenQueue = new PriorityQueue<GraphConnection>(GraphConnection.ConnectionCostComparer);
            StartPoints = new PriorityQueue<GraphConnection>(GraphConnection.ConnectionCostComparer);
        }


        public IPathSearch Reverse()
        {
            Graph.InReverse = true;
            return this;
        }

        public IPathSearch WithCustomBlocker(Func<CPos,bool> customBlock)
        {
            return this;
        }

        public IPathSearch WithIgnoredActor(Actor a)
        {
            Graph.IgnoredActor = a;
            return this;
        }

        public IPathSearch WithHeuristic(Func<CPos,int> h)
        {
            heuristic = h;
            return this;
        }

        public IPathSearch WithCustomCost(Func<CPos,int> f)
        {
            Graph.CustomCost = f;
            return this;
        }

        public IPathSearch WithoutLaneBias()
        {
            return this;
        }

        public IPathSearch FromPoint(CPos from)
        {
            if (Graph.World.Map.Contains(from))
                AddInitialCell(from);
            return this;
        }

        public bool IsTarget(CPos location)
        {
            return false;
        }

        public abstract CPos Expand();

        public bool CanExpand { get { return !OpenQueue.Empty; } }

        public void Dispose() { }

        protected Func<CPos, int> heuristic;    //启发式，下探
        protected Func<CPos, bool> isGoal;

        /// <summary>
        /// 预估委托
        /// </summary>
        /// <param name="destination"></param>
        /// <returns> a gelegate that calcuates the estimation for a node</returns>
        protected static Func<CPos,int> DefaultEstimator(CPos destination)
        {
            return here =>
            {
                var diag = Math.Min(Math.Abs(here.X - destination.X), Math.Abs(here.Y - destination.Y));

                var straight = Math.Abs(here.X - destination.X) + Math.Abs(here.Y - destination.Y);

                return Constants.CellCost * straight + (Constants.DiagonalCellCost - 2 * Constants.CellCost) * diag;
            };
        }


        protected abstract void AddInitialCell(CPos cell);
    }
}