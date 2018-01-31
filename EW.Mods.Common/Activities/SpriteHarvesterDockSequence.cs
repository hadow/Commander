using System;
using EW.Activities;
using EW.Mods.Common.Traits;
using EW.Mods.Common.Traits.Render;

namespace EW.Mods.Common.Activities
{
    public class SpriteHarvesterDockSequence:HarvesterDockSequence
    {

        readonly WithSpriteBody wsb;
        readonly WithDockingAnimationInfo wda;

        public SpriteHarvesterDockSequence(Actor self,Actor refinery,int dockAngle,bool isDragRequired,WVec dragOffset,int dragLength) : base(self, refinery, dockAngle, isDragRequired, dragOffset, dragLength)
        {

            wsb = self.Trait<WithSpriteBody>();
            wda = self.Info.TraitInfo<WithDockingAnimationInfo>();
        }

        public override Activity OnStateDock(Actor self)
        {
            foreach (var trait in self.TraitsImplementing<INotifyHarvesterAction>())
                trait.Docked();

            wsb.PlayCustomAnimation(self, wda.DockSequence, () => wsb.PlayCustomAnimationRepeating(self, wda.DockLoopSequence));
            dockingState = DockingState.Loop;
            return this;
        }

        public override Activity OnStateUndock(Actor self)
        {
            wsb.PlayCustomAnimationBackwards(self, wda.DockSequence, () =>
            {
                dockingState = DockingState.Complete;
                foreach (var trait in self.TraitsImplementing<INotifyHarvesterAction>())
                    trait.Undocked();
            });
            dockingState = DockingState.Wait;
            return this;
        }


    }
}