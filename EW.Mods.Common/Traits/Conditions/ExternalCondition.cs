using System;
using System.Collections.Generic;
using EW.Traits;
using System.Linq;
namespace EW.Mods.Common.Traits
{

    public interface IConditionTimerWatcher{

        string Condition { get; }
        void Update(int duration, int remaining);
    }

    /// <summary>
    /// Allows a condition to be granted from an external source(Lua,warheads,etc.).
    /// </summary>
    public class ExternalConditionInfo : ITraitInfo, Requires<ConditionManagerInfo>
    {
        [GrantedConditionReference]
        [FieldLoader.Require]
        public readonly string Condition = null;

        /// <summary>
        /// If >0,restrict the number of  times that this condition can be granted by a single source.
        /// </summary>
        public readonly int SourceCap = 0;

        /// <summary>
        /// If >0,restrict the number of times that this condition can be granted by any source.
        /// </summary>
        public readonly int TotalCap = 0;


        public object Create(ActorInitializer init) { return new ExternalCondition(init.Self,this); }
    }
    public class ExternalCondition:ITick,INotifyCreated
    {
        struct TimedToken{

            public readonly int Expires;
            public readonly int Token;
            public readonly object Source;

            public TimedToken(int token,Actor self,object source,int duration){
                Token = token;
                Expires = self.World.WorldTick + duration;
                Source = source;
            }
        }


        public readonly ExternalConditionInfo Info;

        readonly ConditionManager conditionManager;

        readonly Dictionary<object, HashSet<int>> permanentTokens = new Dictionary<object, HashSet<int>>();

        readonly List<TimedToken> timedTokens = new List<TimedToken>();


        IConditionTimerWatcher[] watchers;
        int duration;
        int expires;

        public ExternalCondition(Actor self,ExternalConditionInfo info)
        {
            Info = info;
            conditionManager = self.Trait<ConditionManager>();
        }


        public bool CanGrantCondition(Actor self,object source){

            if (conditionManager == null || source == null)
                return false;

            if(Info.SourceCap>0)
            {
                HashSet<int> permanentTokensForSource;
                if (permanentTokens.TryGetValue(source, out permanentTokensForSource) && permanentTokensForSource.Count >= Info.SourceCap)
                    return false;
            }

            if (Info.TotalCap > 0 && permanentTokens.Values.Sum(t => t.Count) >= Info.TotalCap)
                return false;

            return true;

        }


        public int GrantCondition(Actor self,object source,int duration =0){

            if (!CanGrantCondition(self, source))
                return ConditionManager.InvalidConditionToken;


            var token = conditionManager.GrantCondition(self, Info.Condition);
            HashSet<int> permanent;//常驻
            permanentTokens.TryGetValue(source, out permanent);

            if (duration > 0)
            {

                if (Info.SourceCap > 0)
                {
                    var timedCount = timedTokens.Count(t => t.Source == source);
                    if((permanent != null ? permanent.Count + timedCount:timedCount)>=Info.SourceCap){

                        var expireIndex = timedTokens.FindIndex(t => t.Source == source);
                        if(expireIndex>=0){

                            var expireToken = timedTokens[expireIndex].Token;
                            timedTokens.RemoveAt(expireIndex);
                            if (conditionManager.TokenValid(self, expireToken))
                                conditionManager.RevokeCondition(self, expireToken);
                        }
                    }
                }

                if (Info.TotalCap > 0)
                {
                    var totalCount = permanentTokens.Values.Sum(t => t.Count) + timedTokens.Count;
                    if(totalCount>= Info.TotalCap){

                        if(timedTokens.Count>0){

                            var expire = timedTokens[0].Token;

                            if (conditionManager.TokenValid(self, expire))
                                conditionManager.RevokeCondition(self, expire);

                            timedTokens.RemoveAt(0);
                        }
                    }
                }

                var timedToken = new TimedToken(token, self, source, duration);

                var index = timedTokens.FindIndex(t => t.Expires >= timedToken.Expires);

                if (index >= 0)
                    timedTokens.Insert(index, timedToken);
                else
                {
                    timedTokens.Add(timedToken);

                    expires = timedToken.Expires;
                    this.duration = duration;
                }
            }
            else if (permanent == null)
                permanentTokens.Add(source, new HashSet<int> { token });
            else
                permanent.Add(token);

            return token;
        }


        void ITick.Tick(Actor self)
        {
            if (timedTokens.Count == 0)
                return;

            //Remove expired tokens
            var worldTick = self.World.WorldTick;

            var count = 0;
            while(count< timedTokens.Count&& timedTokens[count].Expires < worldTick)
            {
                var token = timedTokens[count].Token;
                if (conditionManager.TokenValid(self, token))
                    conditionManager.RevokeCondition(self, token);
                count++;
            }

            if(count >0){
                timedTokens.RemoveRange(0,count);
                if(timedTokens.Count == 0){

                    foreach (var w in watchers)
                        w.Update(0, 0);
                    return;
                }
            }

            if(timedTokens.Count>0){
                var remaining = expires - worldTick;
                foreach (var w in watchers)
                    w.Update(duration, remaining);
            }
        }


        bool Notifies(IConditionTimerWatcher watcher) { return watcher.Condition == Info.Condition; }
        void INotifyCreated.Created(Actor self)
        {
            watchers = self.TraitsImplementing<IConditionTimerWatcher>().Where(Notifies).ToArray();
        }


    }
}