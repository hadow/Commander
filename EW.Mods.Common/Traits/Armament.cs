using System;
using System.Collections.Generic;
using EW.Traits;
namespace EW.Mods.Common.Traits
{

    public class Barrel
    {
        public WVec Offset;
        public WAngle Yaw;
    }
    public class ArmamentInfo : UpgradableTraitInfo, IRulesetLoaded, Requires<AttackBaseInfo>
    {

        public readonly string Name = "primary";

        [WeaponReference,FieldLoader.Require]
        public readonly string Weapon = null;

        public readonly string AmmoPoolName = "primary";

        public readonly string Turret = "primary";

        public readonly int FireDelay = 0;

        public readonly WVec[] LocalOffset = { };

        public readonly WAngle[] LocalYaw = { };

        public readonly WDist Recoil = WDist.Zero;  //recoil:·´µ¯

        public readonly WDist RecoilRecovery = new WDist(9);

        public readonly string MuzzleSequence = null;//muzzle:Ç¹¿Ú

        public readonly int MuzzleSplitFacings = 0;

        public readonly Stance TargetStances = Stance.Enemy;

        public readonly Stance ForceTargetsStance = Stance.Enemy | Stance.Neutral | Stance.Ally;
        public WeaponInfo WeaponInfo { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="rules"></param>
        /// <param name="ai"></param>
        public void RulesetLoaded(Ruleset rules,ActorInfo ai)
        {

        }

        public override object Create(ActorInitializer init)
        {
            throw new NotImplementedException();
        }
    }
    public class Armament:UpgradableTrait<ArmamentInfo>,INotifyCreated,ITick,IExplodeModifier
    {
        public readonly WeaponInfo Weapon;
        public readonly Barrel[] Barrels;

        public Armament(Actor self,ArmamentInfo info):base(info)
        {

        }


        readonly Actor self;

        public void Tick(Actor self) { }

        public void Created(Actor self) { }

        public bool ShouldExplode(Actor self) { return false; }

        public int FireDelay { get; private set; }
        public bool IsReloading { get { return FireDelay > 0 || IsTraitDisabled; } }
    }
}