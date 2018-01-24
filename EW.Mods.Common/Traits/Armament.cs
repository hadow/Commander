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


        /// <summary>
        /// 枪口偏移
        /// </summary>
        /// <param name="self"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public WVec MuzzleOffset(Actor self,Barrel b)
        {
            return CalculateMuzzleOffset(self, b);
        }

        protected virtual WVec CalculateMuzzleOffset(Actor self,Barrel b)
        {
            var bodyOrientation = coords.QuantizeOrientation(self, self.Orientation);
            var localOffset = b.Offset + new WVec(-Recoil, WDist.Zero, WDist.Zero);
            if(turret != null)
            {
                var turretOrientation = turret.WorldOrientation(self) - bodyOrientation;
                localOffset = localOffset.Rotate(turretOrientation);
                localOffset += turret.Offset;

            }

            return coords.LocalToWorld(localOffset.Rotate(bodyOrientation));
        }

        /// <summary>
        /// 枪口朝向
        /// </summary>
        /// <param name="self"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public WRot MuzzleOrientation(Actor self,Barrel b)
        {
            return CalculateMuzzleOrientation(self, b);
        }

        protected virtual WRot CalculateMuzzleOrientation(Actor self,Barrel b)
        {
            var orientation = turret != null ? turret.WorldOrientation(self) :
                coords.QuantizeOrientation(self, self.Orientation);

            return orientation + WRot.FromYaw(b.Yaw);
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


        public virtual WDist MaxRange()
        {
            return new WDist(Util.ApplyPercentageModifiers(Weapon.Range.Length, rangeModifiers.ToArray()));
        }

        protected virtual bool CanFire(Actor self,Target target)
        {
            if (IsReloading || IsTraitPaused)
                return false;

            if (turret != null && !turret.HasAchieveDesiredFacing)
                return false;

            if ((!target.IsInRange(self.CenterPosition, MaxRange())) ||
                (Weapon.MinRange != WDist.Zero && target.IsInRange(self.CenterPosition, Weapon.MinRange)))
                return false;

            if (!Weapon.IsValidAgainst(target, self.World, self))
                return false;

            return true;
        }

        //Note:facing is only used by the legacy positioning code
        //The world coordinate model uses Actor.Orientation.
        public virtual Barrel CheckFire(Actor self,IFacing facing,Target target)
        {

            if (!CanFire(self, target))
                return null;

            if (ticksSinceLastShot >= Weapon.ReloadDelay)
                Burst = Weapon.Burst;

            ticksSinceLastShot = 0;

            //If Weapon.Burst == 1,cycle through all LocalOffsets,otherwise use the offset corresponding to current Burst.
            currentBarrel %= barrelCount;

            var barrel = Weapon.Burst == 1 ? Barrels[currentBarrel] : Barrels[Burst % Barrels.Length];
            currentBarrel++;

            FireBarrel(self, facing, target, barrel);

            UpdateBurst(self, target);

            return barrel;

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <param name="facing"></param>
        /// <param name="target"></param>
        /// <param name="barrel"></param>
        protected virtual void FireBarrel(Actor self,IFacing facing,Target target,Barrel barrel)
        {
            Func<WPos> muzzlePosition = () => self.CenterPosition + MuzzleOffset(self, barrel);

            var legacyFacing = MuzzleOrientation(self, barrel).Yaw.Angle / 4;

            var passiveTarget = Weapon.TargetActorCenter ? target.CenterPosition : target.Positions.PositionClosestTo(muzzlePosition());
            var initialOffset = Weapon.FirstBurstTargetOffset;

            if(initialOffset != WVec.Zero)
            {
                initialOffset = new WVec(initialOffset.Y, -initialOffset.X, initialOffset.Z);
                passiveTarget += initialOffset.Rotate(WRot.FromFacing(legacyFacing));
            }

            var followingOffset = Weapon.FollowingBurstTargetOffset;
            if(followingOffset!= WVec.Zero)
            {
                followingOffset = new WVec(followingOffset.Y, -followingOffset.X, followingOffset.Z);
                passiveTarget += ((Weapon.Burst - Burst) * followingOffset).Rotate(WRot.FromFacing(legacyFacing));
            }

            var args = new ProjectileArgs
            {
                Weapon = Weapon,
                Facing = legacyFacing,

                DamagedModifiers = damageModifiers.ToArray(),

                InaccuracyModifiers = inaccuracyModifiers.ToArray(),

                RangeModifiers = rangeModifiers.ToArray(),

                Source = muzzlePosition(),

                CurrentSource = muzzlePosition,

                SourceActor = self,

                PassiveTarget = passiveTarget,

                GuidedTarget = target
            };

            foreach (var na in notifyAttacks)
                na.PreparingAttack(self, target, this, barrel);

            ScheduleDelayedAction(Info.FireDelay, () =>
            {

                if(args.Weapon.Projectile != null)
                {
                    var projectile = args.Weapon.Projectile.Create(args);
                    if (projectile != null)
                        self.World.Add(projectile);

                    if (args.Weapon.Report != null && args.Weapon.Report.Any())
                        WarGame.Sound.Play(SoundType.World, args.Weapon.Report.Random(self.World.SharedRandom), self.CenterPosition);

                    if (Burst == args.Weapon.Burst && args.Weapon.StartBurstReport != null && args.Weapon.StartBurstReport.Any())
                        WarGame.Sound.Play(SoundType.World, args.Weapon.StartBurstReport.Random(self.World.SharedRandom), self.CenterPosition);

                    foreach (var na in notifyAttacks)
                        na.Attacking(self, target, this, barrel);

                    Recoil = Info.Recoil;
                }

            });
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <param name="target"></param>
        protected virtual void UpdateBurst(Actor self,Target target)
        {
            if(--Burst > 0)
            {
                if (Weapon.BurstDelays.Length == 1)
                    FireDelay = Weapon.BurstDelays[0];
                else
                    FireDelay = Weapon.BurstDelays[Weapon.Burst - (Burst + 1)];
            }
            else
            {
                var modifiers = reloadModifiers.ToArray();
                FireDelay = Util.ApplyPercentageModifiers(Weapon.ReloadDelay, modifiers);
                Burst = Weapon.Burst;

                if(Weapon.AfterFireSound != null && Weapon.AfterFireSound.Any())
                {
                    ScheduleDelayedAction(Weapon.AfterFireSoundDelay, () =>
                    {
                        WarGame.Sound.Play(SoundType.World, Weapon.AfterFireSound.Random(self.World.SharedRandom), self.CenterPosition);
                    });
                }

                foreach (var nbc in notifyBurstComplete)
                    nbc.FiredBurst(self, target, this);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="t"></param>
        /// <param name="a"></param>
        protected void ScheduleDelayedAction(int t,Action a)
        {
            if (t > 0)
                delayedActions.Add(Pair.New(t, a));
            else
                a();
        }


        public bool ShouldExplode(Actor self) { return false; }

        public virtual bool IsReloading { get { return FireDelay > 0 || IsTraitDisabled; } }

       
    }
}