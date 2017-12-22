using System;


namespace EW.Activities
{
    public class CallFunc:Activity
    {
        Action a;

        public CallFunc(Action a) { this.a = a; }


        public CallFunc(Action a ,bool interruptible)
        {
            this.a = a;
            IsInterruptible = interruptible;
        }


        public override Activity Tick(Actor self)
        {
            if (a != null)
                a();
            return NextActivity;
        }
    }
}