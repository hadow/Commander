using System;
using EW.Activities;

namespace EW.Mods.Common.Activities
{
    public class RemoveSelf:Activity
    {

        public override Activity Tick(Actor self)
        {
            if (IsCanceled) return NextActivity;
            self.Dispose();
            return null;
        }
    }
}