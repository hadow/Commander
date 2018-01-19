using System;
using System.Collections.Generic;
using AI.Fuzzy.Library;
using EW.Traits;
using EW.Mods.Common.Traits;
namespace EW.Mods.Common.AI
{
    sealed class AttackOrFleeFuzzy
    {

        readonly MamdaniFuzzySystem fuzzyEngine = new MamdaniFuzzySystem();

        public AttackOrFleeFuzzy(IEnumerable<string> rulesForNormalOwnHealth,
                                 IEnumerable<string> rulesForInjuredOwnHealth,
                                 IEnumerable<string> rulesForNeadDeadOwnhealth)
        {


        }

        public static readonly AttackOrFleeFuzzy Default = new AttackOrFleeFuzzy(null, null, null);


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
                //inputValues.Add(fuzzyEngine.InputByName("RelativeAttackPower"),)

                var result = fuzzyEngine.Calculate(inputValues);
                attackChance = result[fuzzyEngine.OutputByName("AttackOrFlee")];
            }
            return !double.IsNaN(attackChance) && attackChance < 30.0;
        }
    }
}
