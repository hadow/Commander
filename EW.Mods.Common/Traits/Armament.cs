using System;
using System.Collections.Generic;
using System.Linq;
using EW.Traits;
namespace EW.Mods.Common.Traits
{

    public class Barrel
    {
        public WVec Offset;
        public WAngle Yaw;
    }
    /// <summary>
    /// Allows you to attach weapons to the unit
    /// </summary>
    public class ArmamentInfo : ConditionalTraitInfo, IRulesetLoaded, Requires<AttackBaseInfo>
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
        public override void RulesetLoaded(Ruleset rules,ActorInfo ai)
        {

        }

        public override object Create(ActorInitializer init)
        {
            return new Armament(init.Self, this);
        }
    }
    public class Armament:ConditionalTrait<ArmamentInfo>,ITick,IExplodeModifier
    {
        public readonly WeaponInfo Weapon;
        public readonly Barrel[] Barrels;
        readonly Actor self;
        AmmoPool ammoPool;


        IEnumerable<int> rangeModifiers;
        IEnumerable<int> reloadModifiers;
        IEnumerable<int> damageModifiers;
        IEnumerable<int> inaccuracyModifiers;

        public Armament(Actor self,ArmamentInfo info):base(info)
        {
            this.self = self;

            Weapon = info.WeaponInfo;
        }



        public void Tick(Actor self) { }

        protected override  void Created(Actor self)
        {
            ammoPool = self.TraitsImplementing<AmmoPool>().FirstOrDefault(la => la.Info.Name == Info.AmmoPoolName);
            
        }

        public bool ShouldExplode(Actor self) { return false; }

        public int FireDelay { get; private set; }
        public virtual bool IsReloading { get { return FireDelay > 0 || IsTraitDisabled; } }

        public virtual bool OutOfAmo
        {
            get
            {
                return ammoPool != null && !ammoPool.Info.SelfReloads;
            }
        }

        public virtual WDist MaxRange()
        {
            return new WDist(Util.ApplyPercentageModifiers(Weapon.Range.Length, rangeModifiers.ToArray()));
        }
       
    }
}