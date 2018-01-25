using System;
using System.Collections.Generic;
using EW.Traits;

namespace EW.Mods.Common.Traits
{

    public class WithDeathAnimationInfo : ConditionalTraitInfo,Requires<RenderSpritesInfo>
    {
        
        public override object Create(ActorInitializer init)
        {
            return new WithDeathAnimation(init.Self, this);
        }
    }
    public class WithDeathAnimation:ConditionalTrait<WithDeathAnimationInfo>,INotifyKilled,INotifyCrushed
    {

        readonly RenderSprites rs;

        bool crushed;

        public WithDeathAnimation(Actor self,WithDeathAnimationInfo info) : base(info)
        {
            rs = self.Trait<RenderSprites>();
        }


        public void Killed(Actor self,AttackInfo attackInfo)
        {

        }

        void INotifyCrushed.OnCrush(Actor self, Actor crusher, HashSet<string> crushClasses)
        {

        }


        void INotifyCrushed.WarnCrush(Actor self, Actor crusher, HashSet<string> crushClasses)
        {

        }




    }
}