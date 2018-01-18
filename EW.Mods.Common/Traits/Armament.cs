using System;

using System.Collections.Generic;
using System.Linq;
using EW.Traits;
using EW.Primitives;
namespace EW.Mods.Common.Traits
{
    /// <summary>
    /// 枪管
    /// </summary>
    public class Barrel
    {
        public WVec Offset;
        public WAngle Yaw;
    }
    /// <summary>
    /// Allows you to attach weapons to the unit
    /// </summary>
    public class ArmamentInfo : PausableConditionalTraitInfo, IRulesetLoaded, Requires<AttackBaseInfo>
    {

        public readonly string Name = "primary";

        [WeaponReference,FieldLoader.Require]
        public readonly string Weapon = null;

        public readonly string AmmoPoolName = "primary";

        public readonly string Turret = "primary";

        public readonly int FireDelay = 0;      //Time (in frames) until the weapon can fire again.

        public readonly WVec[] LocalOffset = { };   //Muzzle position relative to turret or body.

        public readonly WAngle[] LocalYaw = { };    //Muzzle yaw relative to turret or body.

        public readonly WDist Recoil = WDist.Zero;  //recoil:反弹 Move the turret backwards when firing.

        public readonly WDist RecoilRecovery = new WDist(9);    //Recoil recovery per-frame. 反弹每帧恢复

        public readonly string MuzzleSequence = null;//muzzle:枪口 // Muzzle flash sequence to render

        public readonly int MuzzleSplitFacings = 0;     // Use multiple muzzle images if non-zero.

        [PaletteReference]
        public readonly string MuzzlePalette = "effect";//Palette to render Muzzle flash sequence in.

        public readonly Stance TargetStances = Stance.Enemy;

        public readonly Stance ForceTargetsStance = Stance.Enemy | Stance.Neutral | Stance.Ally;
       
        public WeaponInfo WeaponInfo { get; private set; }

        public WDist ModifiedRange { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rules"></param>
        /// <param name="ai"></param>
        public override void RulesetLoaded(Ruleset rules,ActorInfo ai)
        {
            WeaponInfo weaponInfo;

            var weaponToLower = Weapon.ToLowerInvariant();
            if (!rules.Weapons.TryGetValue(weaponToLower, out weaponInfo))
                throw new YamlException("Weapons Ruleset does not contain an entry '{0}' ".F(weaponToLower));

            WeaponInfo = weaponInfo;
            ModifiedRange = new WDist(Util.ApplyPercentageModifiers(
                WeaponInfo.Range.Length,
                ai.TraitInfos<IRangeModifierInfo>().Select(m => m.GetRangeModifierDefault())
            ));

            if (WeaponInfo.Burst > 1 && WeaponInfo.BurstDelays.Length > 1 && (WeaponInfo.BurstDelays.Length != WeaponInfo.Burst - 1))
                throw new YamlException("Weapon '{0}' has an invalid number of BurstDelays ,must be single entry or Burst-1".F(weaponToLower));
            
            base.RulesetLoaded(rules,ai);
        }
        

        public override object Create(ActorInitializer init)
        {
            return new Armament(init.Self, this);
        }
    }
    public class Armament:PausableConditionalTrait<ArmamentInfo>,ITick,IExplodeModifier
    {
        public readonly WeaponInfo Weapon;
        public readonly Barrel[] Barrels;
        readonly Actor self;

        Turreted turret;
        BodyOrientation coords;

        INotifyAttack[] notifyAttacks;

        INotifyBurstComplete[] notifyBurstComplete;



        List<Pair<int, Action>> delayedActions = new List<Pair<int, Action>>();

        int currentBarrel;
        int barrelCount;
        int ticksSinceLastShot;


        IEnumerable<int> rangeModifiers;
        IEnumerable<int> reloadModifiers;
        IEnumerable<int> damageModifiers;
        IEnumerable<int> inaccuracyModifiers;

        public WDist Recoil;

        public int Burst { get; protected set; }

        public int FireDelay { get; protected set; }

        public Armament(Actor self,ArmamentInfo info):base(info)
        {
            this.self = self;

            Weapon = info.WeaponInfo;
            Burst = Weapon.Burst;

            var barrels = new List<Barrel>();
            for (var i = 0; i < info.LocalOffset.Length;i++)
            {
                barrels.Add(new Barrel()
                {

                    Offset = info.LocalOffset[i],
                    Yaw = info.LocalYaw.Length > 1 ? info.LocalYaw[i] : WAngle.Zero,
                });
            }

            if (barrels.Count == 0)
                barrels.Add(new Barrel() { Offset = WVec.Zero, Yaw = WAngle.Zero });

            barrelCount = barrels.Count;

            Barrels = barrels.ToArray();



        }

        void ITick.Tick(Actor self){

            Tick(self);
        }



        protected virtual void Tick(Actor self) 
        {
            if (IsTraitDisabled)
                return;

            if (ticksSinceLastShot < Weapon.ReloadDelay)
                ++ticksSinceLastShot;

            if (FireDelay > 0)
                FireDelay--;

            Recoil = new WDist(Math.Max(0, Recoil.Length - Info.RecoilRecovery.Length));

            for (var i = 0;i < delayedActions.Count;i++){
                var x = delayedActions[i];

                if (--x.First <= 0)
                    x.Second();
                delayedActions[i] = x;

                delayedActions.RemoveAll(a=>a.First<=0);
            }
        
        }

        protected override  void Created(Actor self)
        {
            //ammoPool = self.TraitsImplementing<AmmoPool>().FirstOrDefault(la => la.Info.Name == Info.AmmoPoolName);

            turret = self.TraitsImplementing<Turreted>().FirstOrDefault(t => t.Name == Info.Turret);
            coords = self.Trait<BodyOrientation>();

            notifyAttacks = self.TraitsImplementing<INotifyAttack>().ToArray();
            notifyBurstComplete = self.TraitsImplementing<INotifyBurstComplete>().ToArray();

            rangeModifiers = self.TraitsImplementing<IRangeModifier>().ToArray().Select(m => m.GetRangeModifier());
            reloadModifiers = self.TraitsImplementing<IReloadModifier>().ToArray().Select(m => m.GetReloadModifier());
            damageModifiers = self.TraitsImplementing<IFirepowerModifier>().ToArray().Select(m => m.GetFirepowerModifier());
            inaccuracyModifiers = self.TraitsImplementing<IInaccuracyModifier>().ToArray().Select(m => m.GetInaccuracyModifier());

            base.Created(self);
        }

        public bool ShouldExplode(Actor self) { return false; }

        public virtual bool IsReloading { get { return FireDelay > 0 || IsTraitDisabled; } }


        public virtual WDist MaxRange()
        {
            return new WDist(Util.ApplyPercentageModifiers(Weapon.Range.Length, rangeModifiers.ToArray()));
        }
       
    }
}