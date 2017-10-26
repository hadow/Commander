using System;
using System.Collections.Generic;
using EW.Activities;
using EW.Mods.Common.Traits;
using EW.Traits;

namespace EW.Mods.Common.Activities
{
    public class AttackMoveActivity:Activity
    {
        const int ScanInterval = 7;

        Activity inner;

        int scanTicks;

        AutoTarget autoTarget;

        public AttackMoveActivity(Actor self,Activity inner)
        {
            this.inner = inner;
            autoTarget = self.TraitOrDefault<AutoTarget>();
        }

        public override Activity Tick(Actor self)
        {
            if(autoTarget != null && --scanTicks <= 0)
            {
                autoTarget.ScanAndAttack(self, true);
                scanTicks = ScanInterval;
            }

            if (inner == null)
                return NextActivity;
            
            inner = ActivityUtils.RunActivity(self, inner);

            return this;
        }

        public override bool Cancel(Actor self,bool keepQueue = false)
        {
            if (!IsCanceled && inner != null && inner.Cancel(self))
                return false;
            return base.Cancel(self,keepQueue);
        }

        public override IEnumerable<Target> GetTargets(Actor self)
        {
            if (inner != null)
                return inner.GetTargets(self);
            return Target.None;
        }



    }
}