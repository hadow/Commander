using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using EW.Traits;
using EW.Primitives;
namespace EW.Mods.Common.Traits
{
    [Desc("Manages build limits and pre-requisites.")]
    public class TechTreeInfo : ITraitInfo
    {
        public object Create(ActorInitializer init) { return new TechTree(init); }
    }
    public class TechTree
    {
        class Watcher
        {
            public readonly string Key;

            readonly ITechTreeElement watcher;

            readonly string[] prerequisites;

            public ITechTreeElement RegisteredBy { get { return watcher; } }

            bool hasPrerequisites;
            int limit;
            bool hidden;
            bool initialized = false;

            public Watcher(string key,string[] prerequisites,int limit,ITechTreeElement watcher)
            {
                Key = key;
                this.prerequisites = prerequisites;
                this.watcher = watcher;
                hasPrerequisites = false;
                this.limit = limit;
                hidden = false;

            }
            public void Update(Cache<string,List<Actor>> ownedPrerequisites)
            {
                var hasReachedLimit = limit > 0 && ownedPrerequisites.ContainsKey(Key) && ownedPrerequisites[Key].Count >= limit;

                var nowHasPrerequisites = HasPrerequisites(ownedPrerequisites) && !hasReachedLimit;
                var nowHidden = IsHidden(ownedPrerequisites);

                if(!initialized)
                {
                    initialized = true;
                    hasPrerequisites = !nowHasPrerequisites;
                    hidden = !nowHidden;
                }

                // Hide the item from the UI if a prereq annotated with '~' is not met.
                if (nowHidden && !hidden)
                    watcher.PrerequisitesItemHidden(Key);

                if (!nowHidden && hidden)
                    watcher.PrerequisitesItemVisible(Key);

                if (nowHasPrerequisites && !hasPrerequisites)
                    watcher.PrerequisitesAvailable(Key);

                if (!nowHasPrerequisites && hasPrerequisites)
                    watcher.PrerequisitesUnavailable(Key);

                hidden = nowHidden;
                hasPrerequisites = nowHasPrerequisites;

            }


            bool HasPrerequisites(Cache<string,List<Actor>> ownedPrerequisites)
            {

                //PERF:Avoid LINQ
                foreach(var prereq in prerequisites)
                {
                    var withoutTilde = prereq.Replace("~", "");
                    if (withoutTilde.StartsWith("!", StringComparison.Ordinal) ^ !ownedPrerequisites.ContainsKey(withoutTilde.Replace("!", "")))
                        return false;


                }

                return true;
            }

            bool IsHidden(Cache<string,List<Actor>> ownedPrerequisites)
            {
                // PERF: Avoid LINQ.
                foreach (var prereq in prerequisites)
                {
                    if (!prereq.StartsWith("~", StringComparison.Ordinal))
                        continue;
                    var withoutTilde = prereq.Replace("~", "");
                    if (withoutTilde.StartsWith("!", StringComparison.Ordinal) ^ !ownedPrerequisites.ContainsKey(withoutTilde.Replace("!", "")))
                        return true;
                }


                return false;
            }

        }

        readonly Player player;

        readonly List<Watcher> watchers = new List<Watcher>();



        public TechTree(ActorInitializer init)
        {
            player = init.Self.Owner;

            init.World.ActorAdded += ActorChanged;
            init.World.ActorRemoved += ActorChanged;
        }


        public void ActorChanged(Actor a)
        {
            var bi = a.Info.TraitInfoOrDefault<BuildableInfo>();
            if (a.Owner == player && (a.Info.HasTraitInfo<ITechTreePrerequisiteInfo>() || (bi != null && bi.BuildLimit > 0)))
                Update();
        }

        public void Update()
        {
            var ownedPrerequisites = GatherOwnedPrerequisites(player);
            foreach (var w in watchers)
                w.Update(ownedPrerequisites);
        }


        static Cache<string,List<Actor>> GatherOwnedPrerequisites(Player player)
        {

            var ret = new Cache<string, List<Actor>>(x => new List<Actor>());
            if (player == null)
                return ret;


            var prerequisites = player.World.ActorsWithTrait<ITechTreePrerequisite>()
                .Where(a => a.Actor.Owner == player && a.Actor.IsInWorld && !a.Actor.IsDead);

            foreach(var b in prerequisites)
            {
                foreach(var p in b.Trait.ProvidesPrerequisites)
                {
                    if (p == null)
                        continue;

                    ret[p].Add(b.Actor);
                }
            }

            player.World.ActorsWithTrait<Buildable>()
                .Where(a => a.Actor.Owner == player &&
                        a.Actor.IsInWorld &&
                        !a.Actor.IsDead &&
                        !ret.ContainsKey(a.Actor.Info.Name) &&
                        a.Actor.Info.TraitInfo<BuildableInfo>().BuildLimit > 0).Do(b => ret[b.Actor.Info.Name].Add(b.Actor));
            return ret;
        }


        public bool HasPrerequisites(IEnumerable<string> prerequisites)
        {
            var ownedPrereqs = GatherOwnedPrerequisites(player);

            return prerequisites.All(p=>!(p.Replace("~","").StartsWith("!",StringComparison.Ordinal) 
            ^ !ownedPrereqs.ContainsKey(p.Replace("!","").Replace("~",""))));
        }


        public void Add(string key,string[] prerequisites,int limit,ITechTreeElement tte)
        {
            watchers.Add(new Watcher(key, prerequisites, limit, tte));
        }

        public void Remove(string key)
        {
            watchers.RemoveAll(x => x.Key == key);
        }

        public void Remove(ITechTreeElement tte)
        {
            watchers.RemoveAll(x => x.RegisteredBy == tte);
        }


    }
}