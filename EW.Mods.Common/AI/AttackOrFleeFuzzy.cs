using System;
using System.Collections.Generic;

namespace EW.Mods.Common.AI
{
    sealed class AttackOrFleeFuzzy
    {
        public AttackOrFleeFuzzy(IEnumerable<string> rulesForNormalOwnHealth,
                                 IEnumerable<string> rulesForInjuredOwnHealth,
                                 IEnumerable<string> rulesForNeadDeadOwnhealth)
        {


        }

        public static readonly AttackOrFleeFuzzy Default = new AttackOrFleeFuzzy(null, null, null);



        public bool CanAttack(IEnumerable<Actor> ownUnits,IEnumerable<Actor> enemyUnits){

            double attackChance;

            return !double.IsNaN(attackChance) && attackChance < 30.0;
        }
    }
}
