using System;
using EW.Activities;

namespace EW.Mods.Common.Activities
{

    public class Wait : Activity
    {
        int remainingTicks;

        public Wait(int period)
        {
            remainingTicks = period;
        }

        public Wait(int period,bool interruptible)
        {
            remainingTicks = period;
            IsInterruptible = interruptible;
        }

        public override Activity Tick(Actor self)
        {
            return (remainingTicks-- == 0) ? NextActivity : this;
        }

        public override bool Cancel(Actor self, bool keepQueue = false)
        {
            if (!base.Cancel(self, keepQueue))
                return false;

            remainingTicks = 0;
            return true;
        }
    }

    public class WaitFor : Activity
    {

        Func<bool> f;

        public WaitFor(Func<bool> f)
        {
            this.f = f;
        }


        public WaitFor(Func<bool> f, bool interruptible)
        {
            this.f = f;
            IsInterruptible = interruptible;
        }

        public override Activity Tick(Actor self)
        {
            return (f == null || f()) ? NextActivity : this;
        }


        public override bool Cancel(Actor self, bool keepQueue = false)
        {
            if (!base.Cancel(self, keepQueue))
                return false;
            f = null;
            return true;
        }
    }
}