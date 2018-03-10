using System;
using System.Collections.Generic;
using System.Linq;
using EW.Activities;
using EW.Mods.Common.Traits;
using EW.Traits;

namespace EW.Mods.Common.Activities
{
    public enum EnterBehaviour { Exit,Suicide,Dispose}
    public abstract class Enter:Activity
    {
        public enum ReserveStatus { None, TooFar, Pending, Ready }

        enum EnterState{
            ApproachingOrEntering,
            Inside,
            Exiting,
            Done
        }

        readonly IMove move;
        readonly int maxTries = 0;
        readonly EnterBehaviour enterBehaviour;
        readonly bool repathWhileMoving;

        Target target;

        public Target Target { get { return target; } }

        EnterState nextState = EnterState.ApproachingOrEntering;

        bool isEnteringOrInside = false;

        WPos savedPos;

        Activity inner;

        bool firstApproach = true;

        protected Enter(Actor self,Actor target,EnterBehaviour enterBehaviour,int maxTries = 1,bool repathWhileMoving = true)
        {

            move = self.Trait<IMove>();
            this.target = Target.FromActor(target);
            this.maxTries = maxTries;
            this.enterBehaviour = enterBehaviour;
            this.repathWhileMoving = repathWhileMoving;

        }


        protected virtual void AbortOrExit(Actor self){

            if (nextState == EnterState.Done)
                return;

            nextState = isEnteringOrInside ? EnterState.Exiting : EnterState.Done;

            if (inner == this)
                inner = null;
            else if (inner != null)
                inner.Cancel(self);

            if (isEnteringOrInside)
                Unreserve(self, true);


        }

        protected virtual bool TryGetAlternateTarget(Actor self, int tries, ref Target target) { return false; }

        protected virtual void Unreserve(Actor self,bool abort){}

        protected virtual bool CanReserve(Actor self) { return true; }

        protected virtual void OnInside(Actor self) { }

        protected virtual ReserveStatus Reserve(Actor self)
        {
            return !CanReserve(self) ? ReserveStatus.None : move.CanEnterTargetNow(self, target) ? ReserveStatus.Ready : ReserveStatus.TooFar;

        }
        public override Activity Tick(Actor self)
        {
            if (IsCanceled)
                return CanceledTick(self);


            // Check target validity if not exiting or done
            if (nextState != EnterState.Done && (target.Type != TargetT.Actor || !target.IsValidFor(self)))
                AbortOrExit(self);

            // If no current activity, tick next activity
            if (inner == null && FindAndTransitionToNextState(self) == EnterState.Done)
                return CanceledTick(self);

            // Run inner activity/InsideTick
            inner = inner == this ? InsideTick(self) : ActivityUtils.RunActivity(self, inner);

            // If we are finished, move on to next activity
            if (inner == null && nextState == EnterState.Done)
                return NextActivity;
            


            return this;
        }
        // Called when inner activity is this and returns inner activity for next tick.
        protected virtual Activity InsideTick(Actor self) { return null; }


        EnterState FindAndTransitionToNextState(Actor self){

            switch(nextState){

                case EnterState.ApproachingOrEntering:
                    // Reserve to enter or approach
                    isEnteringOrInside = false;
                    switch(TryReserveElseTryAlternateReserve(self)){

                        case ReserveStatus.None:
                            return EnterState.Done; // No available target -> abort to next activity
                        case ReserveStatus.TooFar:
                            {
                                var moveTarget = repathWhileMoving ? target : Target.FromPos(target.Positions.PositionClosestTo(self.CenterPosition));
                                inner = move.MoveToTarget(self, moveTarget);//Approach
                                return EnterState.ApproachingOrEntering;
                            }
                        case ReserveStatus.Pending:
                            return EnterState.ApproachingOrEntering;//Retry next tick
                        case ReserveStatus.Ready:
                            break;              // Reserved targt -> start entering target.
                    }

                    //Entering
                    isEnteringOrInside = true;
                    savedPos = self.CenterPosition;// Save position of self, before entering, for returning on exit

                    inner = move.MoveIntoTarget(self, target);//Enter

                    if(inner!= null){

                        nextState = EnterState.Inside;// Should be inside once inner activity is null
                        return EnterState.ApproachingOrEntering;
                    }

                    // Can enter but there is no activity for it, so go inside without one
                    goto case EnterState.Inside;
                case EnterState.Inside:
                    // Might as well teleport into target if there is no MoveIntoTarget activity
                    if (nextState == EnterState.ApproachingOrEntering)
                        nextState = EnterState.Inside;
                    // Otherwise, try to recover from moving target
                    else if(target.Positions.PositionClosestTo(self.CenterPosition) != self.CenterPosition){


                        nextState = EnterState.ApproachingOrEntering;
                        Unreserve(self, false);
                        if(Reserve(self) == ReserveStatus.Ready){

                            inner = move.MoveIntoTarget(self, target);//Enter
                            if (inner != null)
                                return EnterState.ApproachingOrEntering;

                            nextState = EnterState.ApproachingOrEntering;
                            goto case EnterState.ApproachingOrEntering;
                        }


                        nextState = EnterState.ApproachingOrEntering;
                        isEnteringOrInside = false;
                        inner = move.MoveIntoWorld(self, self.World.Map.CellContaining(savedPos));

                        return EnterState.ApproachingOrEntering;
                    }

                    OnInside(self);

                    if (enterBehaviour == EnterBehaviour.Suicide)
                        self.Kill(self);
                    else if (enterBehaviour == EnterBehaviour.Dispose)
                        self.Dispose();


                    // Return if Abort(Actor) or Done(self) was called from OnInside.
                    if (nextState >= EnterState.Exiting)
                        return EnterState.Inside;

                    inner = this; //Start inside activity
                    nextState = EnterState.Exiting; // Exit once inner activity is null (unless Done(self) is called)
                    return EnterState.Inside;

                case EnterState.Exiting:
                    inner = move.MoveIntoWorld(self, self.World.Map.CellContaining(savedPos));

                    // If not successfully exiting, retry on next tick
                    if (inner == null)
                        return EnterState.Exiting;
                    isEnteringOrInside = false;
                    nextState = EnterState.Done;
                    return EnterState.Exiting;

                case EnterState.Done:
                    return EnterState.Done;

            }
            return EnterState.Done; // dummy to quiet dumb compiler

        }


        ReserveStatus TryReserveElseTryAlternateReserve(Actor self)
            {

            for (var tries = 0; ;){

                switch(Reserve(self)){

                    case ReserveStatus.None:
                            if (++tries > maxTries || !TryGetAlternateTarget(self, tries, ref target))
                                return ReserveStatus.None;
                            continue;
                    case ReserveStatus.TooFar:
                        // Always goto to transport on first approach
                        if(firstApproach){
                                firstApproach = false;
                                return ReserveStatus.TooFar;
                        }

                        if (++tries > maxTries)
                            return ReserveStatus.TooFar;
                        Target t = target;
                        if (!TryGetAlternateTarget(self, tries, ref t))
                            return ReserveStatus.TooFar;

                        var targetPosition = target.Positions.PositionClosestTo(self.CenterPosition);
                        var alternatePosition = t.Positions.PositionClosestTo(self.CenterPosition);
                        if ((targetPosition - self.CenterPosition).HorizontalLengthSquared <= (alternatePosition - self.CenterPosition).HorizontalLengthSquared)
                                return ReserveStatus.TooFar;
                            target = t;
                            continue;
                    case ReserveStatus.Pending:
                            return ReserveStatus.Pending;
                        case ReserveStatus.Ready:
                            return ReserveStatus.Ready;

                }

            }

        }

        Activity CanceledTick(Actor self){

            if (inner == null)
                return ActivityUtils.RunActivity(self, NextActivity);

            inner.Cancel(self);
            inner.Queue(NextActivity);
            return ActivityUtils.RunActivity(self, inner);
        }




    }
}