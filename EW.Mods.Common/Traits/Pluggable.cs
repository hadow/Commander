using System;
using System.Collections.Generic;
using EW.Traits;
using EW.Support;
namespace EW.Mods.Common.Traits
{
    public class PluggableInfo:ITraitInfo,UsesInit<PlugsInit>
    {
        public readonly CVec Offset = CVec.Zero;

        [FieldLoader.Require]
        public readonly Dictionary<string, string> Conditions = null;


        public readonly Dictionary<string, BooleanExpression> Requirements = new Dictionary<string, BooleanExpression>();


        public object Create(ActorInitializer init)
        {
            return new Pluggable(init,this);
        }
    }

    public class Pluggable:IObservesVariables,INotifyCreated
    {
        public readonly PluggableInfo Info;

        readonly string initialPlug;
        ConditionManager conditionManager;
        int conditionToken = ConditionManager.InvalidConditionToken;
        Dictionary<string, bool> plugTypesAvailability = null;

        string active;

        public Pluggable(ActorInitializer init,PluggableInfo info)
        {
            Info = info;

            var plugInit = init.Contains<PlugsInit>() ? init.Get<PlugsInit, Dictionary<CVec, string>>() : new Dictionary<CVec, string>();
            if (plugInit.ContainsKey(Info.Offset))
                initialPlug = plugInit[Info.Offset];

            if(info.Requirements.Count > 0)
            {
                plugTypesAvailability = new Dictionary<string, bool>();
                foreach (var plug in info.Requirements)
                    plugTypesAvailability[plug.Key] = true;
            }
        }

        void INotifyCreated.Created(Actor self)
        {
            conditionManager = self.TraitOrDefault<ConditionManager>();

        }

        public bool AcceptsPlug(Actor self,string type)
        {
            if (!Info.Conditions.ContainsKey(type))
                return false;

            if (!Info.Requirements.ContainsKey(type))
                return active == null;

            return plugTypesAvailability[type];
        }

        IEnumerable<VariableObserver> IObservesVariables.GetVariableObservers()
        {
            foreach (var req in Info.Requirements)
                yield return new VariableObserver((self, variables) => plugTypesAvailability[req.Key] = req.Value.Evaluate(variables), req.Value.Variables);
        }
    }


    public class PlugsInit : IActorInit<Dictionary<CVec, string>>
    {
        [DictionaryFromYamlKey]
        readonly Dictionary<CVec, string> value = new Dictionary<CVec, string>();

        public PlugsInit() { }

        public PlugsInit(Dictionary<CVec,string> init)
        {
            value = init;
        }

        public Dictionary<CVec,string> Value(World world) { return value; }
    }
}