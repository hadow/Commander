using System;
using EW.Traits;
namespace EW.Mods.Common.Traits.Sound
{

    public class AnnounceOnKillInfo : ITraitInfo
    {
        public object Create(ActorInitializer init) { return new AnnounceOnKill(init.Self,this); }
    }
    public class AnnounceOnKill
    {

        public AnnounceOnKill(Actor self,AnnounceOnKillInfo info) { }
    }
}