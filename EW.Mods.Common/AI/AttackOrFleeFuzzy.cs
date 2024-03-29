﻿using System;
using System.Collections.Generic;
using System.Linq;
using AI.Fuzzy.Library;
using EW.Traits;
using EW.Mods.Common.Traits;
using EW.Mods.Common.Warheads;
namespace EW.Mods.Common.AI
{
    sealed class AttackOrFleeFuzzy
    {
        

        static readonly string[] DefaultRulesNormalOwnHealth = new[]
        {
            "if ((OwnHealth is Normal) " +
            "and ((EnemyHealth is NearDead) or (EnemyHealth is Injured) or (EnemyHealth is Normal)) " +
            "and ((RelativeAttackPower is Weak) or (RelativeAttackPower is Equal) or (RelativeAttackPower is Strong)) " +
            "and ((RelativeSpeed is Slow) or (RelativeSpeed is Equal) or (RelativeSpeed is Fast))) " +
            "then AttackOrFlee is Attack"
        };

        static readonly string[] DefaultRulesInjuredOwnHealth = new[]
        {
            "if ((OwnHealth is Injured) " +
            "and (EnemyHealth is NearDead) " +
            "and ((RelativeAttackPower is Weak) or (RelativeAttackPower is Equal) or (RelativeAttackPower is Strong)) " +
            "and ((RelativeSpeed is Slow) or (RelativeSpeed is Equal) or (RelativeSpeed is Fast))) " +
            "then AttackOrFlee is Attack",

            "if ((OwnHealth is Injured) " +
            "and ((EnemyHealth is Injured) or (EnemyHealth is Normal)) " +
            "and ((RelativeAttackPower is Equal) or (RelativeAttackPower is Strong)) " +
            "and ((RelativeSpeed is Slow) or (RelativeSpeed is Equal) or (RelativeSpeed is Fast))) " +
            "then AttackOrFlee is Attack",

            "if ((OwnHealth is Injured) " +
            "and ((EnemyHealth is Injured) or (EnemyHealth is Normal)) " +
            "and (RelativeAttackPower is Weak) " +
            "and (RelativeSpeed is Slow)) " +
            "then AttackOrFlee is Attack",

            "if ((OwnHealth is Injured) " +
            "and ((EnemyHealth is Injured) or (EnemyHealth is Normal)) " +
            "and (RelativeAttackPower is Weak) " +
            "and ((RelativeSpeed is Equal) or (RelativeSpeed is Fast))) " +
            "then AttackOrFlee is Flee",

            "if ((OwnHealth is Injured) " +
            "and ((EnemyHealth is NearDead) or (EnemyHealth is Injured) or (EnemyHealth is Normal)) " +
            "and ((RelativeAttackPower is Weak) or (RelativeAttackPower is Equal) or (RelativeAttackPower is Strong)) " +
            "and (RelativeSpeed is Slow)) " +
            "then AttackOrFlee is Attack"
        };

        static readonly string[] DefaultRulesNearDeadOwnHealth = new[]
        {
            "if ((OwnHealth is NearDead) " +
            "and ((EnemyHealth is NearDead) or (EnemyHealth is Injured)) " +
            "and ((RelativeAttackPower is Equal) or (RelativeAttackPower is Strong)) " +
            "and ((RelativeSpeed is Slow) or (RelativeSpeed is Equal))) " +
            "then AttackOrFlee is Attack",

            "if ((OwnHealth is NearDead) " +
            "and ((EnemyHealth is NearDead) or (EnemyHealth is Injured)) " +
            "and (RelativeAttackPower is Weak) " +
            "and ((RelativeSpeed is Equal) or (RelativeSpeed is Fast))) " +
            "then AttackOrFlee is Flee",

            "if ((OwnHealth is NearDead) " +
            "and (EnemyHealth is Normal) " +
            "and (RelativeAttackPower is Weak) " +
            "and ((RelativeSpeed is Equal) or (RelativeSpeed is Fast))) " +
            "then AttackOrFlee is Flee",

            "if (OwnHealth is NearDead) " +
            "and (EnemyHealth is Normal) " +
            "and ((RelativeAttackPower is Equal) or (RelativeAttackPower is Strong)) " +
            "and (RelativeSpeed is Fast) " +
            "then AttackOrFlee is Flee",

            "if (OwnHealth is NearDead) " +
            "and (EnemyHealth is Injured) " +
            "and (RelativeAttackPower is Equal) " +
            "and (RelativeSpeed is Fast) " +
            "then AttackOrFlee is Flee"
        };

        readonly MamdaniFuzzySystem fuzzyEngine = new MamdaniFuzzySystem();
        public static readonly AttackOrFleeFuzzy Default = new AttackOrFleeFuzzy(null, null, null);

        public static readonly AttackOrFleeFuzzy Rush = new AttackOrFleeFuzzy(new[]{

            "if ((OwnHealth is Normal) " +
            "and ((EnemyHealth is NearDead) or (EnemyHealth is Injured) or (EnemyHealth is Normal)) " +
            "and (RelativeAttackPower is Strong) " +
            "and ((RelativeSpeed is Slow) or (RelativeSpeed is Equal) or (RelativeSpeed is Fast))) " +
            "then AttackOrFlee is Attack",

            "if ((OwnHealth is Normal) " +
            "and ((EnemyHealth is NearDead) or (EnemyHealth is Injured) or (EnemyHealth is Normal)) " +
            "and ((RelativeAttackPower is Weak) or (RelativeAttackPower is Equal)) " +
            "and ((RelativeSpeed is Slow) or (RelativeSpeed is Equal) or (RelativeSpeed is Fast))) " +
            "then AttackOrFlee is Flee"
        }, null, null);
        public AttackOrFleeFuzzy(IEnumerable<string> rulesForNormalOwnHealth,
                                 IEnumerable<string> rulesForInjuredOwnHealth,
                                 IEnumerable<string> rulesForNeadDeadOwnHealth)
        {
            lock (fuzzyEngine)
            {
                var playerHealthFuzzy = new FuzzyVariable("OwnHealth", 0.0, 100.0);
                playerHealthFuzzy.Terms.Add(new FuzzyTerm("NearDead", new TrapezoidMembershipFunction(0, 0, 20, 40)));
                playerHealthFuzzy.Terms.Add(new FuzzyTerm("Injured", new TrapezoidMembershipFunction(30, 50, 50, 70)));
                playerHealthFuzzy.Terms.Add(new FuzzyTerm("Normal", new TrapezoidMembershipFunction(50, 80, 100, 100)));
                fuzzyEngine.Input.Add(playerHealthFuzzy);

                var enemyHealthFuzzy = new FuzzyVariable("EnemyHealth", 0.0, 100.0);
                enemyHealthFuzzy.Terms.Add(new FuzzyTerm("NearDead", new TrapezoidMembershipFunction(0, 0, 20, 40)));
                enemyHealthFuzzy.Terms.Add(new FuzzyTerm("Injured", new TrapezoidMembershipFunction(30, 50, 50, 70)));
                enemyHealthFuzzy.Terms.Add(new FuzzyTerm("Normal", new TrapezoidMembershipFunction(50, 80, 100, 100)));
                fuzzyEngine.Input.Add(enemyHealthFuzzy);

                var relativeAttackPowerFuzzy = new FuzzyVariable("RelativeAttackPower", 0.0, 1000.0);
                relativeAttackPowerFuzzy.Terms.Add(new FuzzyTerm("Weak", new TrapezoidMembershipFunction(0, 0, 70, 90)));
                relativeAttackPowerFuzzy.Terms.Add(new FuzzyTerm("Equal", new TrapezoidMembershipFunction(85, 100, 100, 115)));
                relativeAttackPowerFuzzy.Terms.Add(new FuzzyTerm("Strong", new TrapezoidMembershipFunction(110, 150, 150, 1000)));
                fuzzyEngine.Input.Add(relativeAttackPowerFuzzy);

                var relativeSpeedFuzzy = new FuzzyVariable("RelativeSpeed", 0.0, 1000.0);
                relativeSpeedFuzzy.Terms.Add(new FuzzyTerm("Slow", new TrapezoidMembershipFunction(0, 0, 70, 90)));
                relativeSpeedFuzzy.Terms.Add(new FuzzyTerm("Equal", new TrapezoidMembershipFunction(85, 100, 100, 115)));
                relativeSpeedFuzzy.Terms.Add(new FuzzyTerm("Fast", new TrapezoidMembershipFunction(110, 150, 150, 1000)));
                fuzzyEngine.Input.Add(relativeSpeedFuzzy);

                var attackOrFleeFuzzy = new FuzzyVariable("AttackOrFlee", 0.0, 50.0);
                attackOrFleeFuzzy.Terms.Add(new FuzzyTerm("Attack", new TrapezoidMembershipFunction(0, 15, 15, 30)));
                attackOrFleeFuzzy.Terms.Add(new FuzzyTerm("Flee", new TrapezoidMembershipFunction(25, 35, 35, 50)));
                fuzzyEngine.Output.Add(attackOrFleeFuzzy);

                foreach (var rule in rulesForNormalOwnHealth ?? DefaultRulesNormalOwnHealth)
                    AddFuzzyRule(rule);
                foreach (var rule in rulesForInjuredOwnHealth ?? DefaultRulesInjuredOwnHealth)
                    AddFuzzyRule(rule);
                foreach (var rule in rulesForNeadDeadOwnHealth ?? DefaultRulesNearDeadOwnHealth)
                    AddFuzzyRule(rule);
            }

        }

        void AddFuzzyRule(string rule)
        {
            fuzzyEngine.Rules.Add(fuzzyEngine.ParseRule(rule));
        }




        static float NormalizedHealth(IEnumerable<Actor> actors,float normalizeByValue)
        {
            var sumOfMaxHp = 0;
            var sumOfHp = 0;
            foreach(var a in actors)
            {
                if (a.Info.HasTraitInfo<HealthInfo>())
                {
                    sumOfMaxHp += a.Trait<Health>().MaxHP;
                    sumOfHp += a.Trait<Health>().HP;
                }
            }

            if (sumOfMaxHp == 0)
                return 0.0f;
            return (sumOfHp * normalizeByValue) / sumOfMaxHp;
        }


        public bool CanAttack(IEnumerable<Actor> ownUnits,IEnumerable<Actor> enemyUnits){

            double attackChance;
            var inputValues = new Dictionary<FuzzyVariable, double>();
            lock (fuzzyEngine)
            {
                inputValues.Add(fuzzyEngine.InputByName("OwnHealth"), NormalizedHealth(ownUnits, 100));
                inputValues.Add(fuzzyEngine.InputByName("EnemyHealth"), NormalizedHealth(enemyUnits, 100));
                inputValues.Add(fuzzyEngine.InputByName("RelativeAttackPower"), RelativePower(ownUnits, enemyUnits));
                inputValues.Add(fuzzyEngine.InputByName("RelativeSpeed"), RelativeSpeed(ownUnits, enemyUnits));

                var result = fuzzyEngine.Calculate(inputValues);
                attackChance = result[fuzzyEngine.OutputByName("AttackOrFlee")];
            }
            return !double.IsNaN(attackChance) && attackChance < 30.0;
        }


        static float RelativeSpeed(IEnumerable<Actor> own,IEnumerable<Actor> enemy){

            return RelativeValue(own, enemy, 100, Average<MobileInfo>, a => a.Info.TraitInfo<MobileInfo>().Speed);
        }

        static float RelativePower(IEnumerable<Actor> own,IEnumerable<Actor> enemy){

            return RelativeValue(own, enemy, 100, SumOfValues<AttackBaseInfo>, a =>
            {

                var sumOfDamage = 0;
                var arms = a.TraitsImplementing<Armament>();
                foreach(var arm in arms){

                    var burst = arm.Weapon.Burst;

                    var totalReloadDelay = arm.Weapon.ReloadDelay + (arm.Weapon.BurstDelays[0] * (burst - 1)).Clamp(1, 200);
                    var damageWarheads = arm.Weapon.Warheads.OfType<DamageWarhead>();
                    foreach (var warhead in damageWarheads)
                        sumOfDamage += (warhead.Damage * burst / totalReloadDelay) * 100;

                }

                return sumOfDamage;
            });

        }

        static float RelativeValue(IEnumerable<Actor> own,IEnumerable<Actor> enemy,float normalizeByValue,
                                  Func<IEnumerable<Actor>,Func<Actor,int>,float> relativeFunc,Func<Actor,int> getValue){

            if (!enemy.Any())
                return 999.0f;

            if (!own.Any())
                return 0.0f;

            var relative = (relativeFunc(own, getValue) / relativeFunc(enemy, getValue)) * normalizeByValue;
            return relative.Clamp(0.0f, 999.0f);
        }

        static float SumOfValues<TTraitInfo>(IEnumerable<Actor> actors,Func<Actor,int> getValue) where TTraitInfo:ITraitInfo{

            var sum = 0;
            foreach(var a in actors){
                if (a.Info.HasTraitInfo<TTraitInfo>())
                    sum += getValue(a);
                
            }
            return sum;
        }



        static float Average<TTraitInfo>(IEnumerable<Actor> actors,Func<Actor,int> getValue) where TTraitInfo:ITraitInfo{

            var sum = 0;

            var countActors = 0;

            foreach(var a in actors){

                if(a.Info.HasTraitInfo<TTraitInfo>())
                {
                    sum += getValue(a);

                    countActors++;
                }
            }

            if (countActors == 0)
                return 0.0f;

            return sum / countActors;
        }
    }
}
