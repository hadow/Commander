using System;
using System.Collections.Generic;
using System.Linq;
using EW.Traits;
namespace EW.Activities
{

    public enum ActivityState { Queued,Active,Done,Canceled}

    /// <summary>
    /// 
    /// </summary>
    public abstract class Activity
    {

        public ActivityState State { get; private set; }

        public bool IsInterruptible { get; protected set; }

        public bool IsCanceled
        {
            get
            {
                return State == ActivityState.Canceled;
            }
        }

        public Activity RootActivity
        {
            get
            {
                var p = this;
                while (p.ParentActivity != null)
                    p = p.ParentActivity;

                return p;
            }
        }

        Activity parentActivity;

        public Activity ParentActivity
        {
            get { return parentActivity; }
            set
            {
                parentActivity = value;

                var next = NextInQueue;
                if (next != null)
                    next.ParentActivity = parentActivity;
            }
        }


        Activity childActivity;
        protected Activity ChildActivity
        {
            get
            {
                return childActivity != null && childActivity.State < ActivityState.Done ? childActivity : null;
            }
            set
            {
                if (value == this || value == ParentActivity || value == NextInQueue)
                    childActivity = null;
                else
                {
                    childActivity = value;

                    if (childActivity != null)
                        childActivity.ParentActivity = this;
                }
            }
        }

        Activity nextActivity;

        public virtual Activity NextActivity
        {
            get
            {
                return nextActivity != null ? nextActivity : ParentActivity;
            }
            set
            {
                if (value == this || value == ParentActivity || (value != null && value.ParentActivity == this))
                    nextActivity = null;
                else
                {
                    nextActivity = value;

                    if (nextActivity != null)
                        nextActivity.ParentActivity = ParentActivity;
                }
            }
        }

        public Activity NextInQueue
        {
            get { return nextActivity; }
            set
            {
                NextActivity = value;
            }
        }

        

        public Activity()
        {
            IsInterruptible = true;
        }


        public Activity TickOuter(Actor self)
        {
            if (State == ActivityState.Done && WarGame.Settings.Debug.StrictActivityChecking)
                throw new InvalidOperationException("Actor {0} attempted to tick activity {1} after it had already completed.".F(self, this.GetType()));

            if(State == ActivityState.Queued)
            {
                OnFirstRun(self);
                State = ActivityState.Active;
            }

            var ret = Tick(self);
            if(ret == null || (ret!=this && ret.ParentActivity != this))
            {
                //Make sure that the Parent's ChildActivity pointer is moved forwards as the child queue advances.
                //The Child's ParentActivity will be set automatically during assignment.
                if (ParentActivity != null && ParentActivity != ret)
                    ParentActivity.ChildActivity = ret;

                if (State != ActivityState.Canceled)
                    State = ActivityState.Done;

                OnLastRun(self);
            }

            return ret;
        }


        public abstract Activity Tick(Actor self);

        public virtual bool Cancel(Actor self, bool keepQueue = false)
        {
            if (!IsInterruptible)
                return false;

            if(!keepQueue)
                NextActivity = null;

            ChildActivity = null;
            State = ActivityState.Canceled;
            return true;
        }

        public virtual void Queue(Activity activity)
        {

            if (NextInQueue != null)
                NextInQueue.Queue(activity);
            else
                NextInQueue = activity;
        }

        public virtual void QueueChild(Activity activity)
        {
            if (ChildActivity != null)
                ChildActivity.Queue(activity);
            else
                ChildActivity = activity;
        }

        public virtual IEnumerable<Target> GetTargets(Actor self)
        {
            yield break;
        }

        protected virtual void OnFirstRun(Actor self) { }

        protected virtual void OnLastRun(Actor self) { }

        protected void PrintActivityTree(Activity origin = null,int level = 0)
        {
            if (origin == null)
                RootActivity.PrintActivityTree(this);
            else
            {

            }
        }

    }

    public  abstract class CompositeActivity : Activity
    {
        public override Activity NextActivity
        {
            get
            {
                if (ChildActivity != null)
                    return ChildActivity;
                else if (NextInQueue != null)
                    return NextInQueue;
                else
                    return ParentActivity;
            }
        }
    }

    public static class ActivityExts
    {
        public static IEnumerable<Target> GetTargetQueue(this Actor self)
        {
            return self.CurrentActivity.Iterate(u => u.NextActivity).TakeWhile(u => u != null).SelectMany(u => u.GetTargets(self));
        }
    }


}