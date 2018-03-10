using System;
using System.Collections.Generic;
using System.Linq;
using EW.Traits;
namespace EW.Mods.Common.Traits
{

    public enum ExplosionType { Footprint,CenterPosition}

    [Desc("This actor explodes when killed.")]
    public class ExplodesInfo : ConditionalTraitInfo,Requires<HealthInfo>
    {
        [WeaponReference,FieldLoader.Require,]
        public readonly string Weapon = null;

        [WeaponReference]
        public readonly string EmptyWeapon = "UnitExplode";

        /// <summary>
        /// Chance that the explosion will use Weapon instead of EmptyWeapon when exploding
        /// </summary>
        public readonly int LoadedChance = 100;

        public readonly int Chance = 100;

        /// <summary>
        /// Health level at which actor will explode.
        /// </summary>
        public readonly int DamageThreshold = 0;

        /// <summary>
        /// DeathType(s) that trigger the explosion.Leave empty to always trigger an explosion.
        /// </summary>
        public readonly HashSet<string> DeathTypes = new HashSet<string>();

        public readonly ExplosionType Type = ExplosionType.CenterPosition;

        public WeaponInfo WeaponInfo { get; private set; }

        public WeaponInfo EmptyWeaponInfo { get; private set; }




        public override object Create(ActorInitializer init)
        {
            return new Explodes(this, init.Self);
        }

        public override void RulesetLoaded(Ruleset rules, ActorInfo info)
        {

            if (!string.IsNullOrEmpty(Weapon))
            {
                WeaponInfo weapon;
                var weaponToLower = Weapon.ToLowerInvariant();
                if (!rules.Weapons.TryGetValue(weaponToLower, out weapon))
                    throw new YamlException("Weapons Ruleset does not contain an entry '{0}'".F(weaponToLower));

                WeaponInfo = weapon;
            }


            if (!string.IsNullOrEmpty(EmptyWeapon))
            {
                WeaponInfo emptyWeapon;
                var emptyWeaponToLower = EmptyWeapon.ToLowerInvariant();
                if (!rules.Weapons.TryGetValue(emptyWeaponToLower, out emptyWeapon))
                    throw new YamlException("Weapons Ruleset does not contain an entry '{0}'".F(emptyWeaponToLower));

                EmptyWeaponInfo = emptyWeapon;
            }

            base.RulesetLoaded(rules, info);
        }
    }
    public class Explodes:ConditionalTrait<ExplodesInfo>,INotifyKilled,INotifyDamage,INotifyCreated
    {

        readonly Health health;
        BuildingInfo buildingInfo;

        public Explodes(ExplodesInfo info,Actor self) : base(info)
        {
            health = self.Trait<Health>();
        }

        void INotifyCreated.Created(Actor self)
        {
            buildingInfo = self.Info.TraitInfoOrDefault<BuildingInfo>();
        }

        void INotifyKilled.Killed(Actor self, AttackInfo attackInfo)
        {
            if (IsTraitDisabled || !self.IsInWorld)
                return;

            if (self.World.SharedRandom.Next(100) > Info.Chance)
                return;

            if (Info.DeathTypes.Count > 0 && !attackInfo.Damage.DamageTypes.Overlaps(Info.DeathTypes))
                return;

            var weapon = ChooseWeaponForExplosion(self);
            if (weapon == null)
                return;

            if (weapon.Report != null && weapon.Report.Any())
                WarGame.Sound.Play(SoundType.World, weapon.Report.Random(attackInfo.Attacker.World.SharedRandom), self.CenterPosition);

            if(Info.Type == ExplosionType.Footprint && buildingInfo != null)
            {
                var cells = buildingInfo.UnpathableTiles(self.Location);
                foreach (var c in cells)
                    weapon.Impact(Target.FromPos(self.World.Map.CenterOfCell(c)), attackInfo.Attacker, Enumerable.Empty<int>());

                return;
            }

            //Use .FromPos since this actor is killed,Cannot use Target.FromActor.
            weapon.Impact(Target.FromPos(self.CenterPosition),attackInfo.Attacker,Enumerable.Empty<int>());

        }

        void INotifyDamage.Damaged(Actor self, AttackInfo attackInfo)
        {
            if (IsTraitDisabled || !self.IsInWorld)
                return;

            if (Info.DamageThreshold == 0)
                return;

            if (health.HP * 100 < Info.DamageThreshold * health.MaxHP)
                self.World.AddFrameEndTask(w => self.Kill(attackInfo.Attacker));
        }

        WeaponInfo ChooseWeaponForExplosion(Actor self)
        {
            var shouldExplode = self.TraitsImplementing<IExplodeModifier>().All(a => a.ShouldExplode(self));
            var useFullExplosion = self.World.SharedRandom.Next(100) <= Info.LoadedChance;
            return (shouldExplode && useFullExplosion) ? Info.WeaponInfo : Info.EmptyWeaponInfo;
        }




    }
}