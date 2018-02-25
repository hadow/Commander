using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    public class ResourceStorageWarningInfo : ITraitInfo, Requires<PlayerResourcesInfo>
    {

        public readonly int AdviceInterval = 20;

        public readonly int Threshold = 80;

        public readonly string Notification = "SilosNeeded";

        public object Create(ActorInitializer init) { return new ResourceStorageWarning(init.Self,this); }
    }
    public class ResourceStorageWarning:ITick
    {
        readonly ResourceStorageWarningInfo info;
        readonly PlayerResources resources;

        public ResourceStorageWarning(Actor self,ResourceStorageWarningInfo info)
        {
            this.info = info;
            resources = self.Trait<PlayerResources>();

        }


        void ITick.Tick(Actor self)
        {

        }
    }
}