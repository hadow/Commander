using System;
using System.Collections.Generic;
using System.Linq;
using EW.Traits;
namespace EW.Activities
{

    /// <summary>
    /// 活动存在于自身建立的图形数据结构中.每项活动都有一个父级活动和可选的子级活动(通常是下一个活动),CurrentActivity 是一个指向该图的指针，随着活动的进行而移动。
    /// <summary>
    /// </summary>
    public enum ActivityState 
    { 
        Queued,//闃熷垪涓?
        Active, //娲诲姩涓?
        Done,   //宸插畬鎴?
        Canceled //宸插彇娑?
    }

    /// <summary>
    /// 
    /// </summary>
    public abstract class Activity
    {

        public ActivityState State { get; private set; }    //娲诲姩鐘舵�?

        public bool IsInterruptible { get; protected set; } //鏍囪瘑鏄惁鍙互涓柇

        public Activity()
        {
            IsInterruptible = true; //默认情况下活动是可以被中断
        }

        /// <summary>
        /// 标识活动是否已被取消
        /// </summary>
        public bool IsCanceled
        {
            get
            {
                return State == ActivityState.Canceled;
            }
        }

        /// <summary>
        /// Returns the top-most activity *from the point of view of the calling activity*.
        /// Note that the root activity can and likely will have next activities of its own,which would in turn be the root for their children.
        /// 从调用者活动的角度返回最顶端的活动。
        /// 根源的活动可能而且可能会有自己的下一个子活动，这反过来又是他们子级的根源
        /// </summary>
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

        /// <summary>
        /// 父级活动
        /// </summary>
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


        /// <summary>
        /// 子活动
        /// </summary>
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
        /// <summary>
        /// The getter will return either the next activity or,if there is none,the parent one.
        /// getter 将返回下一个活动，如果没有，则返回父级活动
        /// </summary>
        public virtual Activity NextActivity
        {
            get
            {
                return nextActivity != null ? nextActivity : ParentActivity;
            }
            set
            {
                
                if (value == this || value == ParentActivity || (value != null && value.ParentActivity == this))
                    nextActivity = null;//后续没有正在排队的活动了
                else
                {
                    nextActivity = value;
                    if (nextActivity != null)
                        nextActivity.ParentActivity = ParentActivity;
                }
            }
        }

        /// <summary>
        /// The getter will return the next activity on the same level_only_,in contrast to NextActivity.
        /// Use this to check whether there are any follow-up activities queued.
        /// 与NextActivity相比，getter将返回相同level_only_上的下一个活动。用它来检查是否有任何后续活动排队。
        /// </summary>
        public Activity NextInQueue
        {
            get { return nextActivity; }
            set
            {
                NextActivity = value;
            }
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
                //确保父级的子活动指针随着子队列的前进而向前移动
                //子项的父活动将在分配过程中自动设置
                if (ParentActivity != null && ParentActivity != ret)
                    ParentActivity.ChildActivity = ret;

                if (State != ActivityState.Canceled)
                    State = ActivityState.Done;

                OnLastRun(self);
            }

            return ret;
        }


        public abstract Activity Tick(Actor self);

        /// <summary>
        /// 活动取消
        /// </summary>
        /// <param name="self"></param>
        /// <param name="keepQueue">标识是否保持队列顺序</param>
        /// <returns></returns>
        public virtual bool Cancel(Actor self, bool keepQueue = false)
        {
            if (!IsInterruptible)
                return false;

            if (ChildActivity != null && !ChildActivity.Cancel(self))
                return false;

            if(!keepQueue)
                NextActivity = null;

            ChildActivity = null;
            State = ActivityState.Canceled;
            return true;
        }

        /// <summary>
        /// 一级活动排队
        /// 
        /// </summary>
        /// <param name="activity">如果当前没有在排队的活动，activity 即最后的一个活动.如果有正在排队的活动，activity排在当前活动之后</param>
        public virtual void Queue(Activity activity)
        {

            if (NextInQueue != null)
                NextInQueue.Queue(activity);
            else
                NextInQueue = activity;
        }

        /// <summary>
        /// 二级活动排队
        /// </summary>
        /// <param name="activity"></param>
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

    /// <summary>
    /// In cointrast to the base activity class,which is responsible for running its children itself,
    /// composite activities rely on the actor's activity-running logic for their children.
    /// </summary>
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