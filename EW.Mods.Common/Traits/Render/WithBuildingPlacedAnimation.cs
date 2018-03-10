using System;
using EW.Traits;

namespace EW.Mods.Common.Traits.Render
{


    public class WithBuildingPlacedAnimationInfo : ITraitInfo
    {

        [Desc("Sequence name to use"), SequenceReference]
        public readonly string Sequence = "build";

        public object Create(ActorInitializer init) { return new WithBuildingPlacedAnimation(init.Self,this); }
    }



    public class WithBuildingPlacedAnimation:INotifyBuildingPlaced,INotifyBuildComplete,INotifyTransform
    {

        readonly WithBuildingPlacedAnimationInfo info;
        readonly WithSpriteBody wsb;
        bool buildComplete;

        public WithBuildingPlacedAnimation(Actor self, WithBuildingPlacedAnimationInfo info)
        {
            this.info = info;
            wsb = self.Trait<WithSpriteBody>();
            buildComplete = !self.Info.HasTraitInfo<BuildingInfo>();
        }

        void INotifyBuildComplete.BuildingComplete(Actor self)
        {
            buildComplete = true;
        }


        void INotifyTransform.BeforeTransform(Actor self)
        {
            buildComplete = false;
        }

        void INotifyTransform.OnTransform(Actor self) { }
        void INotifyTransform.AfterTransform(Actor self) { }

        void INotifyBuildingPlaced.BuildingPlaced(Actor self)
        {
            if (buildComplete)
                wsb.PlayCustomAnimation(self, info.Sequence);
        }

    }
}