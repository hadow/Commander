using System;
using System.Linq;
using EW.Traits;

namespace EW.Mods.Common.Traits
{
    [Desc("Throws particles when the actor is destroyed that do damage on impact.")]
    public class ThrowsShrapnelInfo:ITraitInfo,IRulesetLoaded
    {

        [WeaponReference, FieldLoader.Require]
        [Desc("The weapons used for shrapnel.")]
        public readonly string[] Weapons = { };

        [Desc("The amount of pieces of shrapnel to expel. Two values indicate a range.")]
        public readonly int[] Pieces = { 3, 10 };


        [Desc("The minimum and maximum distances the shrapnel may travel.")]
        public readonly WDist[] Range = { WDist.FromCells(2), WDist.FromCells(5) };

        public WeaponInfo[] WeaponInfos { get; private set; }

        public object Create(ActorInitializer init) { return new ThrowsShrapnel(this); }

        public void RulesetLoaded(Ruleset rules, ActorInfo ai)
        {
            WeaponInfos = Weapons.Select(w =>
            {
                WeaponInfo weapon;
                var weaponToLower = w.ToLowerInvariant();
                if (!rules.Weapons.TryGetValue(weaponToLower, out weapon))
                    throw new YamlException("Weapons Ruleset does not contain an entry '{0}'".F(weaponToLower));
                return weapon;
            }).ToArray();
        }
    }


    public class ThrowsShrapnel:INotifyKilled
    {

        readonly ThrowsShrapnelInfo info;

        public ThrowsShrapnel(ThrowsShrapnelInfo info)
        {
            this.info = info;
        }


        void INotifyKilled.Killed(Actor self,AttackInfo attack){


        }



    }
}