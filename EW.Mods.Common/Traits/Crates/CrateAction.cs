using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    public class CrateActionInfo : ITraitInfo
    {
        public virtual object Create(ActorInitializer init) { return new CrateAction(init.Self,this); }
    }
    public class CrateAction
    {

        public CrateAction(Actor self,CrateActionInfo info)
        {

        }
    }
}