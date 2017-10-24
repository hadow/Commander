using System;
using System.Collections.Generic;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    //public interface IConditionTimerWatcher
    //{
    //    string Condition { get; }

    //    void Update(int duration, int remaining);
    //}

    /// <summary>
    /// Attach this to a unit to enable dynamic conditions by warheads,experience,crates,support powers.
    /// </summary>
    public class ConditionManagerInfo:TraitInfo<ConditionManager>,Requires<IObservesVariablesInfo>{}


    public class ConditionManager:INotifyCreated
    {
        /// <summary>
        /// Value used to represent an invalid token.
        /// </summary>
        public static readonly int InvalidConditionToken = -1;

        //class ConditionTimer
        //{
        //    public readonly int Token;
        //    public readonly int Duration;
        //    public int Remaining;

        //    public ConditionTimer(int token,int duration)
        //    {
        //        Token = token;
        //        Duration = Remaining = duration;
        //    }
        //}

        /// <summary>
        ///  条件状态
        /// </summary>
        class ConditionState
        {
            public readonly List<VariableObserverNotifier> Notifiers = new List<VariableObserverNotifier>();

            public readonly HashSet<int> Tokens = new HashSet<int>();

            /// <summary>
            /// External callbacks that are to be executed when a timed condition changes.
            /// </summary>
            //public readonly List<IConditionTimerWatcher> Watchers = new List<IConditionTimerWatcher>();
        }

        //readonly Dictionary<string, List<ConditionTimer>> timers = new Dictionary<string, List<ConditionTimer>>();
        //readonly HashSet<int> timersToRemove = new HashSet<int>();

        Dictionary<string, ConditionState> state;

        Dictionary<int, string> tokens = new Dictionary<int, string>();

        int nextToken = 1;

        readonly Dictionary<string, int> conditionCache = new Dictionary<string, int>();

        IReadOnlyDictionary<string, int> readOnlyConditionCache;

        public int GrantCondition(Actor self,string condition)
        {
            var token = nextToken++;
            tokens.Add(token, condition);

            if (state != null)
                UpdateConditionState(self, condition, token, false);

            return token;
        }

        /// <summary>
        /// 撤销
        /// </summary>
        /// <param name="self"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public int RevokeCondition(Actor self,int token)
        {
            string condition;
            if (!tokens.TryGetValue(token, out condition))
                throw new InvalidOperationException("Attempting to revoke condition with invalid token {0} for {1}".F(token, self));

            tokens.Remove(token);

            if (state != null)
                UpdateConditionState(self, condition, token, true);

            return InvalidConditionToken;
        }
        //void ITick.Tick(Actor self)
        //{
        //    foreach(var kv in timers)
        //    {
        //        var duration = 0;
        //        var remaining = 0;
        //        foreach(var t in kv.Value)
        //        {
        //            if (--t.Remaining <= 0)
        //                timersToRemove.Add(t.Token);

        //            //Track the duration and remaining time for the longest remaining timer
        //            if (t.Remaining > remaining)
        //            {
        //                remaining = t.Remaining;
        //                duration = t.Duration;
        //            }
        //        }

        //        foreach (var w in state[kv.Key].Watchers)
        //            w.Update(duration, remaining);
        //    }

        //    foreach (var t in timersToRemove)
        //        RevokeCondition(self,t);

        //    timersToRemove.Clear();
        //}

        void INotifyCreated.Created(Actor self)
        {
            state = new Dictionary<string, ConditionState>();

            readOnlyConditionCache = new ReadOnlyDictionary<string, int>(conditionCache);

            var allObservers = new HashSet<VariableObserverNotifier>();

            foreach(var provider in self.TraitsImplementing<IObservesVariables>())
            {
                foreach(var variableUser in provider.GetVariableObservers())
                {
                    allObservers.Add(variableUser.Notifier);

                    foreach(var variable in variableUser.Variables)
                    {
                        var cs = state.GetOrAdd(variable);
                        cs.Notifiers.Add(variableUser.Notifier);
                        conditionCache[variable] = 0;
                    }
                }
            }

            foreach(var kv in tokens)
            {
                ConditionState conditionState;
                if (!state.TryGetValue(kv.Value, out conditionState))
                    continue;

                conditionState.Tokens.Add(kv.Key);
                conditionCache[kv.Value] = conditionState.Tokens.Count;
            }

            foreach(var consumer in allObservers)
            {
                consumer(self, readOnlyConditionCache);
            }
        }

        void UpdateConditionState(Actor self,string condition,int token,bool isRevoke)
        {
            ConditionState conditionState;

            if (!state.TryGetValue(condition, out conditionState))
                return;

            if (isRevoke)
                conditionState.Tokens.Remove(token);
            else
                conditionState.Tokens.Add(token);

            conditionCache[condition] = conditionState.Tokens.Count;

            foreach (var notify in conditionState.Notifiers)
                notify(self, readOnlyConditionCache);
        }

        public bool TokenValid(Actor self,int token)
        {
            return tokens.ContainsKey(token);
        }
    }
}