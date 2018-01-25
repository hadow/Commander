using System;
using EW.Traits;
namespace EW.Mods.Common.Traits.Sound
{

    public class AnnounceOnKillInfo : ITraitInfo
    {
        public object Create(ActorInitializer init) { return new AnnounceOnKill(init.Self,this); }
    }
    public class AnnounceOnKill:INotifyAppliedDamage
    {
        readonly AnnounceOnKillInfo info;

        public AnnounceOnKill(Actor self,AnnounceOnKillInfo info)
        {
            this.info = info;

        }


        public void AppliedDamage(Actor self,Actor damaged,AttackInfo attackInfo)
        {

        }


    }
}