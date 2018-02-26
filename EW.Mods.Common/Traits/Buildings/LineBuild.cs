using System;
using System.Collections.Generic;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    public interface INotifyLineBuildSegmentsChanged
    {
        void SegmentAdded(Actor self, Actor segment);

        void SegmentRemoved(Actor self, Actor segment);
    }

    public class LineBuildInfo : ITraitInfo
    {
        public readonly int Range = 5;

        public readonly HashSet<string> NodeType = new HashSet<string> { "wall" };

        [ActorReference(typeof(LineBuildInfo))]
        public readonly string SegmentType = null;

        public readonly bool SegmentsRequireNode = false;

        public object Create(ActorInitializer init) { return new LineBuild(init, this); }
    }


    public class LineBuild:INotifyKilled,INotifyAddedToWorld,INotifyRemovedFromWorld
    {
        readonly LineBuildInfo info;
        readonly Actor[] parentNodes = new Actor[0];
        HashSet<Actor> segments;
        
        public LineBuild(ActorInitializer init,LineBuildInfo info)
        {
            this.info = info;

        }


        void INotifyKilled.Killed(Actor self, AttackInfo attackInfo)
        {

        }

        void INotifyAddedToWorld.AddedToWorld(Actor self)
        {

        }

        void INotifyRemovedFromWorld.RemovedFromWorld(Actor self)
        {

        }
    }
}