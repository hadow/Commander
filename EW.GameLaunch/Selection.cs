using System;
using System.Collections.Generic;
using System.Linq;
using EW.Primitives;
using EW.Traits;
namespace EW
{
    public class Selection
    {
        /// <summary>
        /// Tracking Selection Changes
        /// </summary>
        public int Hash { get; private set; }

        readonly HashSet<Actor> actors = new HashSet<Actor>();
        public IEnumerable<Actor> Actors { get { return actors; } }
        Cache<int, List<Actor>> controlGroups = new Cache<int, List<Actor>>(_=>new List<Actor>());

        void UpdateHash(){

            Hash++;
        }

        public void Add(World w,Actor a){

            actors.Add(a);
            UpdateHash();

            foreach (var sel in a.TraitsImplementing<INotifySelected>())
                sel.Selected(a);
            foreach (var ns in w.WorldActor.TraitsImplementing<INotifySelection>())
                ns.SelectionChanged();
        }

        public void Tick(World world){

            var removed = actors.RemoveWhere(a => !a.IsInWorld || (!a.Owner.IsAlliedWith(world.RenderPlayer) && world.FogObscures(a)));

            if (removed > 0)
                UpdateHash();

            foreach(var cg in controlGroups.Values){
                cg.RemoveAll(a=>a.Disposed || a.Owner != world.LocalPlayer);
            }


        }


        public void Combine(World world,IEnumerable<Actor> newSelection,bool isCombine,bool isClick)
        {
            if (isClick)
            {

                var adjNewSelection = newSelection.Take(1);
                if (isCombine)
                    actors.SymmetricExceptWith(adjNewSelection);
                else
                {
                    actors.Clear();
                    actors.UnionWith(adjNewSelection);
                }
            }
            else
            {
                if (isCombine)
                    actors.UnionWith(newSelection);
                else
                {
                    actors.Clear();
                    actors.UnionWith(newSelection);
                }
            }

            UpdateHash();

            foreach (var a in newSelection)
                foreach (var sel in a.TraitsImplementing<INotifySelected>())
                    sel.Selected(a);

            foreach (var ns in world.WorldActor.TraitsImplementing<INotifySelection>())
                ns.SelectionChanged();

            if (world.IsGameOver)
                return;

            foreach(var actor in actors)
            {
                if (actor.Owner != world.LocalPlayer || !actor.IsInWorld)
                    continue;

                var selectable = actor.Info.TraitInfoOrDefault<SelectableInfo>();
                if (selectable == null || !actor.HasVoice(selectable.Voice))
                    continue;

                actor.PlayVoice(selectable.Voice);
                break;
            }
        }


        public void Clear()
        {
            actors.Clear();
            UpdateHash();
        }



        public bool Contains(Actor a)
        {
            return actors.Contains(a);
        }

        public void AddToControlGroup(Actor a, int group)
        {
            if (!controlGroups[group].Contains(a))
                controlGroups[group].Add(a);
        }

        public int? GetControlGroupForActor(Actor a){


            return controlGroups.Where(g => g.Value.Contains(a))
                                .Select(g => (int?)g.Key).FirstOrDefault();
        }
    }
}