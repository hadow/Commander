using System;
using System.Collections.Generic;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    public interface IConditionTimerWatcher
    {
        string Condition { get; }

        void Update(int duration, int remaining);
    }

    /// <summary>
    /// Attach this to a unit to enable dynamic conditions by warheads,experience,crates,support powers.
    /// </summary>
    public class ConditionManagerInfo:TraitInfo<ConditionManager>,Requires<IObservesVariablesInfo>
    {


    }


    public class ConditionManager:ITick,INotifyCreated
    {
        /// <summary>
        /// Value used to represent an invalid token.
        /// </summary>
        public static readonly int InvalidConditionToken = -1;

        class ConditionTimer
        {
            public readonly int Token;
            public readonly int Duration;
            public int Remaining;

            public ConditionTimer(int token,int duration)
            {
                Token = token;
                Duration = Remaining = duration;
            }
        }

        class ConditionState
        {
            public readonly HashSet<int> Tokens = new HashSet<int>();

            /// <summary>
            /// External callbacks that are to be executed when a timed condition changes.
            /// </summary>
            public readonly List<IConditionTimerWatcher> Watchers = new List<IConditionTimerWatcher>();
        }

        readonly Dictionary<string, List<ConditionTimer>> timers = new Dictionary<string, List<ConditionTimer>>();
        readonly HashSet<int> timersToRemove = new HashSet<int>();
        Dictionary<string, ConditionState> state;


        public int RevokeCondition(Actor self,int token)
        {
            return InvalidConditionToken;
        }
        void ITick.Tick(Actor self)
        {
            foreach(var kv in timers)
            {
                var duration = 0;
                var remaining = 0;
                foreach(var t in kv.Value)
                {
                    if (--t.Remaining <= 0)
                        timersToRemove.Add(t.Token);

                    //Track the duration and remaining time for the longest remaining timer
                    if (t.Remaining > remaining)
                    {
                        remaining = t.Remaining;
                        duration = t.Duration;
                    }
                }

                foreach (var w in state[kv.Key].Watchers)
                    w.Update(duration, remaining);
            }

            foreach (var t in timersToRemove)
                RevokeCondition(self,t);

            timersToRemove.Clear();
        }

        void INotifyCreated.Created(Actor self)
        {
            state = new Dictionary<string, ConditionState>();
        }
    }
}