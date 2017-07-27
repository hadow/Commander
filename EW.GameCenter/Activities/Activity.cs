using System;
using System.Collections.Generic;
using EW.Traits;
namespace EW.Activities
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class Activity
    {

        public Activity NextActivity { get; set; }

        protected bool IsCanceled { get; private set; }


        public abstract Activity Tick(Actor self);

        public virtual void Cancel(Actor self)
        {
            IsCanceled = true;
            NextActivity = null;
        }

        public virtual void Queue(Activity activity)
        {
            if (NextActivity != null)
                NextActivity.Queue(activity);
            else
                NextActivity = activity;
        }

        public virtual IEnumerable<Target> GetTargets(Actor self)
        {
            yield break;
        }

    }
}