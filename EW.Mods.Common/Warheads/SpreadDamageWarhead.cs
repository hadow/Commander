using System;
using System.Collections.Generic;
using System.Linq;
using EW.Framework;
using EW.Traits;
using EW.Mods.Common.Traits;
namespace EW.Mods.Common.Warheads
{
    public class SpreadDamageWarhead:DamageWarhead,IRulesetLoaded<WeaponInfo>
    {
        public readonly WDist Spread = new WDist(43);

        /// <summary>
        /// Damage percentage at each range step
        /// </summary>
        public readonly int[] Falloff = { 100, 37, 14, 5, 0 };

        public WDist[] Range = null;

        public WDist VictimScanRadius = new WDist(-1);

        void IRulesetLoaded<WeaponInfo>.RulesetLoaded(Ruleset rules, WeaponInfo info)
        {
            if (VictimScanRadius < WDist.Zero)
                VictimScanRadius = Util.MinimumRequiredBlockerScanRadius(rules);

            if (Range != null)
            {
                if (Range.Length != 1 && Range.Length != Falloff.Length)
                    throw new YamlException("Number of range values must be 1 or equal to the number of Falloff values.");

                for (var i = 0; i < Range.Length - 1; i++)
                {
                    if (Range[i] > Range[i + 1])
                        throw new YamlException("Range values must be specified in an increasing order.");
                }
            }
            else
                Range = Exts.MakeArray(Falloff.Length, i => i * Spread);
        }

        public override void DoImpact(WPos pos, Actor firedBy, IEnumerable<int> damagedModifiers)
        {
            var world = firedBy.World;


            var hitActors = world.FindActorsInCircle(pos, Range[Range.Length - 1]+ VictimScanRadius);

            foreach(var victim in hitActors)
            {
                //Cannot be damaged without a Health trait.
                var healthInfo = victim.Info.TraitInfoOrDefault<HealthInfo>();
                if (healthInfo == null)
                    continue;

                //Cannot be damaged without an active HitShape
                var activeShapes = victim.TraitsImplementing<HitShape>().Where(Exts.IsTraitEnabled);

                if (!activeShapes.Any())
                    continue;

                var distance = activeShapes.Min(t => t.Info.Type.DistanceFromEdge(pos, victim));
                var localModifiers = damagedModifiers.Append(GetDamageFalloff(distance.Length));

                DoImpact(victim, firedBy, localModifiers);

            }
        }

        int GetDamageFalloff(int distance)
        {
            var inner = Range[0].Length;
            for(var i = 1; i < Range.Length; i++)
            {
                var outer = Range[i].Length;
                if (outer > distance)
                    return Int2.Lerp(Falloff[i - 1], Falloff[i], distance - inner, outer - inner);

                inner = outer;
            }

            return 0;
        }
    }
}