using System;
using EW.Traits;

namespace EW.Mods.Common.Traits.Sound
{
    public class AnnounceOnBuildInfo : ITraitInfo
    {

        public object Create(ActorInitializer init) { return new AnnounceOnBuild(init.Self, this); }
    }

    public class AnnounceOnBuild
    {

        public AnnounceOnBuild(Actor self,AnnounceOnBuildInfo info)
        {

        }
    }
}