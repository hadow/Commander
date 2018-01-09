using System;

using EW.Traits;
namespace EW.Mods.Cnc.Traits.Render
{
    public class WithBuildingBibInfo:ITraitInfo
    {
        public object Create(ActorInitializer init) { return new WithBuildingBib(init.Self, this); }
    }


    public class WithBuildingBib : INotifyAddedToWorld, INotifyRemovedFromWorld
    {

        public WithBuildingBib(Actor self,WithBuildingBibInfo info)
        {

        }

        public void AddedToWorld(Actor self)
        {

        }

        public void RemovedFromWorld(Actor self)
        {

        }

    }
}