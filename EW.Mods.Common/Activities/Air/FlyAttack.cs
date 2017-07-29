using System;
using System.Collections.Generic;
using EW.Traits;
using EW.Mods.Common.Traits;
using EW.Activities;
namespace EW.Mods.Common.Activities
{
    public class FlyAttack:Activity
    {
        public FlyAttack(Actor self,Target target)
        {

        }

        public override Activity Tick(Actor self)
        {
            throw new NotImplementedException();
        }

        public override void Cancel(Actor self)
        {
            base.Cancel(self);
        }




    }
}