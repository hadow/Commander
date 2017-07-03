using System;
using EW.Activities;
namespace EW.Mods.Common.Activities
{
    public class Move:Activity
    {


        public override void Cancel(Actor self)
        {
            base.Cancel(self);
        }

        public override void Queue(Activity activity)
        {
            base.Queue(activity);
        }


        public override Activity Tick(Actor self)
        {
            throw new NotImplementedException();

        }

    }
}