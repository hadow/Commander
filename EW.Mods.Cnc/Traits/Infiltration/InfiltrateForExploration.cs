using System;
using System.Collections.Generic;
using System.Linq;
using EW.Mods.Common.Traits;
using EW.Traits;
namespace EW.Mods.Cnc.Traits
{
    [Desc("Steal and reset the owner's exploration.")]

    class InfiltrateForExplorationInfo : ITraitInfo
    {
        public readonly HashSet<string> Types = new HashSet<string>();

        public object Create(ActorInitializer init)
        {
            return new InfiltrateForExploration(init.Self, this);
        }
    }
    class InfiltrateForExploration:INotifyInfiltrated
    {

        readonly InfiltrateForExplorationInfo info;

        public InfiltrateForExploration(Actor self,InfiltrateForExplorationInfo info){

            this.info = info;
        }


        void INotifyInfiltrated.Infiltrated(Actor self,Actor infiltrator,HashSet<string> types){


            if (!info.Types.Overlaps(types))
                return;

            infiltrator.Owner.Shroud.Explore(self.Owner.Shroud);

            var preventReset = self.Owner.PlayerActor.TraitsImplementing<IPreventsShroudReset>().Any(p => p.PreventShroudReset(self));

            if (!preventReset)
                self.Owner.Shroud.ResetExploration();
            
        }


    }
}