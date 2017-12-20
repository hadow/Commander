using System;
using System.Collections.Generic;
using EW.Primitives;

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

        public void Tick(World world){

            var removed = actors.RemoveWhere(a => !a.IsInWorld || (!a.Owner.IsAlliedWith(world.RenderPlayer) && world.FogObscures(a)));

            if (removed > 0)
                UpdateHash();

            foreach(var cg in controlGroups.Values){
                cg.RemoveAll(a=>a.Disposed || a.Owner != world.LocalPlayer);
            }


        }
    }
}