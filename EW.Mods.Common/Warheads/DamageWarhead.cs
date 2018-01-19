using System;
using System.Linq;
using System.Collections.Generic;
using EW.Traits;
using EW.Mods.Common.Traits;
namespace EW.Mods.Common.Warheads
{
    public abstract class DamageWarhead:Warhead
    {
        public readonly int Damage = 0;

        /// <summary>
        /// Types of damage that this warhead causes
        /// </summary>
        public readonly HashSet<string> DamageTypes = new HashSet<string>();

        [FieldLoader.LoadUsing("LoadVersus")]
        public readonly Dictionary<string, int> Versus;

        public static object LoadVersus(MiniYaml yaml)
        {
            var nd = yaml.ToDictionary();
            return nd.ContainsKey("Versus") ? nd["Versus"].ToDictionary(my => FieldLoader.GetValue<int>("(value)", my.Value)) : new Dictionary<string, int>();
        }

        public int DamageVersus(Actor victim)
        {
            var armor = victim.TraitsImplementing<Armor>()
                .Where(a => !a.IsTraitDisabled && a.Info.Type != null && Versus.ContainsKey(a.Info.Type))
                .Select(a => Versus[a.Info.Type]);

            return Util.ApplyPercentageModifiers(100, armor);
        }


        public abstract void DoImpact(WPos pos, Actor firedBy, IEnumerable<int> damageModifiers);

        public virtual void DoImpact(Actor victim,Actor firedBy,IEnumerable<int> damageModifiers)
        {
            if (!IsValidAgainst(victim, firedBy))
                return;

            var damage = Util.ApplyPercentageModifiers(Damage, damageModifiers.Append(DamageVersus(victim)));
            victim.InflictDamage(firedBy, new Damage(damage, DamageTypes));
        }


        public override void DoImpact(Target target, Actor firedBy, IEnumerable<int> damageModifiers)
        {
            //Used by traits that damage a single actor,rather than a position

            if (target.Type == TargetT.Actor)
                DoImpact(target.Actor, firedBy, damageModifiers);
            else if (target.Type != TargetT.Invalid)
                DoImpact(target.CenterPosition, firedBy, damageModifiers);
        }

    }
}