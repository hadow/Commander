using System;
using System.Collections.Generic;
using EW.Traits;

namespace EW.Mods.Common.Traits
{

    public class GainsExperienceInfo : ITraitInfo
    {
        [FieldLoader.Require]
        public readonly Dictionary<int, string> Conditions = null;

        [GrantedConditionReference]
        public IEnumerable<string> LinterConditions{ get { return Conditions.Values; }}

        [PaletteReference]
        public readonly string LevelUpPalette = "effect";


        public readonly bool SuppressLevelupAnimation = true;

        public object Create(ActorInitializer init) { return new GainsExperience(init,this); }
    }

    public class GainsExperience:INotifyCreated,ISync
    {

        readonly Actor self;

        readonly GainsExperienceInfo info;

        ConditionManager conditionManager;

        [Sync]
        int experience = 0;

        [Sync]
        public int Level { get; private set; }

        public readonly int MaxLevel;
        public GainsExperience(ActorInitializer init,GainsExperienceInfo info){

            self = init.Self;
            this.info = info;

            MaxLevel = info.Conditions.Count;
            var cost = self.Info.TraitInfo<ValuedInfo>().Cost;

        }

        void INotifyCreated.Created(Actor self){

            conditionManager = self.TraitOrDefault<ConditionManager>();

        }

    }

    class ExperienceInit:IActorInit<int>{

        [FieldFromYamlKey]
        readonly int value;
        public ExperienceInit(){}

        public ExperienceInit(int init) { value = init; }
        public int Value(World world) { return value; }
    }
}