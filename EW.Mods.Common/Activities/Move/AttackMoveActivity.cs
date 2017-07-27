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
            throw new NotImplementedException();
        }

        public override void Cancel(Actor self)
        {
            base.Cancel(self);
        }

        public override IEnumerable<Target> GetTargets(Actor self)
        {
            return base.GetTargets(self);
        }



    }
}