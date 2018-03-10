using System;
using System.Collections.Generic;
using EW.Traits;
using EW.NetWork;
using EW.Primitives;
using EW.Mods.Common.Effects;
namespace EW.Mods.Common.Traits
{
    [Desc("This actor's experience increases when it has killed a GivesExperience actor.")]
    public class GainsExperienceInfo : ITraitInfo
    {

        [FieldLoader.Require]
        public readonly Dictionary<int, string> Conditions = null;

        [GrantedConditionReference]
        public IEnumerable<string> LinterConditions{ get { return Conditions.Values; }}

        [Desc("Palette for the level up sprite.")]
        [PaletteReference]
        public readonly string LevelUpPalette = "effect";

        [Desc("Should the level-up animation be suppressed when actor is created?")]
        public readonly bool SuppressLevelupAnimation = true;

        public object Create(ActorInitializer init) { return new GainsExperience(init,this); }
    }

    public class GainsExperience:INotifyCreated,ISync,IResolveOrder
    {

        readonly Actor self;

        readonly GainsExperienceInfo info;
        readonly int initialExperience;

        ConditionManager conditionManager;
        // Stored as a percentage of our value
        [Sync]
        int experience = 0;

        [Sync]
        public int Level { get; private set; }

        readonly List<Pair<int, string>> nextLevel = new List<Pair<int, string>>();

        public readonly int MaxLevel;
        public GainsExperience(ActorInitializer init,GainsExperienceInfo info){

            self = init.Self;
            this.info = info;

            MaxLevel = info.Conditions.Count;
            var cost = self.Info.TraitInfo<ValuedInfo>().Cost;

            foreach(var kv in info.Conditions){
                nextLevel.Add(Pair.New(kv.Key * cost,kv.Value));
            }

            if (init.Contains<ExperienceInit>())
                initialExperience = init.Get<ExperienceInit, int>();


        }

        void INotifyCreated.Created(Actor self){

            conditionManager = self.TraitOrDefault<ConditionManager>();

            if(initialExperience > 0){
                GiveExperience(initialExperience, info.SuppressLevelupAnimation);
            }
        }


        public void GiveLevels(int numLevels,bool silent = false){

            var newLevel = Math.Min(Level + numLevels, MaxLevel);

            GiveExperience(nextLevel[newLevel -1].First - experience,silent);
        }

        public void GiveExperience(int amount,bool silent = false){

            if (amount < 0)
                throw new ArgumentException("Revoking experience  is not implemented","amount");

            experience += amount;

            while(Level < MaxLevel &&   experience >= nextLevel[Level].First){


                if (conditionManager != null)
                    conditionManager.GrantCondition(self, nextLevel[Level].Second);

                Level++;

                if(!silent){
                    WarGame.Sound.PlayNotification(self.World.Map.Rules, self.Owner, "Sounds", "LevelUp", self.Owner.Faction.InternalName);
                    self.World.AddFrameEndTask(w=>w.Add(new CrateEffect(self,"levelup",info.LevelUpPalette)));
                }
            }
        }

        void IResolveOrder.ResolveOrder(Actor self,Order order){


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