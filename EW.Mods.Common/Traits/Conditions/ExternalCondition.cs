using System;
using System.Collections.Generic;
using EW.Traits;
using System.Linq;
namespace EW.Mods.Common.Traits
{

    /// <summary>
    /// Allows a condition to be granted from an external source(Lua,warheads,etc.).
    /// </summary>
    public class ExternalConditionInfo : ITraitInfo, Requires<ConditionManagerInfo>
    {
        [GrantedConditionReference]
        [FieldLoader.Require]
        public readonly string Condition = null;

        public readonly int SourceCap = 0;

        public readonly int TotalCap = 0;


        public object Create(ActorInitializer init) { return new ExternalCondition(init.Self,this); }
    }
    public class ExternalCondition:ITick,INotifyCreated
    {
        public readonly ExternalConditionInfo Info;
        readonly ConditionManager conditionManager;
        readonly Dictionary<object, HashSet<int>> permanentTokens = new Dictionary<object, HashSet<int>>();

        public ExternalCondition(Actor self,ExternalConditionInfo info)
        {
            Info = info;
            conditionManager = self.Trait<ConditionManager>();
        }
        void ITick.Tick(Actor self)
        {

        }

        void INotifyCreated.Created(Actor self)
        {

        }


    }
}