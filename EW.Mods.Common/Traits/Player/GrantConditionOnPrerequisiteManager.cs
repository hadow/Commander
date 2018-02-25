using System;
using System.Collections.Generic;
using EW.Traits;
using System.Linq;
using EW.Primitives;

namespace EW.Mods.Common.Traits
{


    public class GrantConditionOnPrerequisiteManagerInfo : ITraitInfo,Requires<TechTreeInfo>
    {
        public object Create(ActorInitializer init) { return new GrantConditionOnPrerequisiteManager(init); }
    }

    public class GrantConditionOnPrerequisiteManager:ITechTreeElement
    {
        readonly Actor self;
        readonly Dictionary<string, List<Pair<Actor, GrantConditionOnPrerequisite>>> upgradables = new Dictionary<string, List<Pair<Actor, GrantConditionOnPrerequisite>>>();
        readonly TechTree techTree;


        public GrantConditionOnPrerequisiteManager(ActorInitializer init)
        {
            self = init.Self;
            techTree = self.Trait<TechTree>();
        }

        static string MakeKey(string[] prerequisites)
        {
            return "condition_" + string.Join("_", prerequisites.OrderBy(a => a));
        }

        public void Register(Actor actor,GrantConditionOnPrerequisite u,string[] prerequisites)
        {
            var key = MakeKey(prerequisites);
            if (!upgradables.ContainsKey(key))
            {
                upgradables.Add(key, new List<Pair<Actor, GrantConditionOnPrerequisite>>());
                techTree.Add(key, prerequisites, 0, this);
            }

            upgradables[key].Add(Pair.New(actor, u));
            u.PrerequisitesUpdated(actor, techTree.HasPrerequisites(prerequisites));
            
        }

        public void Unregister(Actor actor,GrantConditionOnPrerequisite u,string[] prerequisites)
        {
            var key = MakeKey(prerequisites);

            var list = upgradables[key];

            list.RemoveAll(x => x.First == actor && x.Second == u);

            if (!list.Any())
            {
                upgradables.Remove(key);
                techTree.Remove(key);
            }

        }

        public void PrerequisitesAvailable(string key)
        {
            List<Pair<Actor, GrantConditionOnPrerequisite>> list;
            if (!upgradables.TryGetValue(key, out list))
                return;

            foreach (var u in list)
                u.Second.PrerequisitesUpdated(u.First, true);
        }

        public void PrerequisitesUnavailable(string key)
        {
            List<Pair<Actor, GrantConditionOnPrerequisite>> list;
            if (!upgradables.TryGetValue(key, out list))
                return;

            foreach (var u in list)
                u.Second.PrerequisitesUpdated(u.First, false);
        }

        public void PrerequisitesItemHidden(string key) { }

        public void PrerequisitesItemVisible(string key) { }


    }
}