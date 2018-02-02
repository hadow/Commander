using System;
using System.Collections.Generic;
using System.Drawing;
using EW.Mods.Common.Traits;
using EW.Graphics;
using EW.Mods.Common.Graphics;
using EW.Traits;
using EW.Framework;
namespace EW.Mods.Common.Projectiles
{
    public class AreaBeamInfo : IProjectileInfo,IRulesetLoaded<WeaponInfo>
    {

        /// <summary>
        /// Color of the beam.
        /// </summary>
        public readonly Color Color = Color.Red;

        public readonly bool UsePlayerColor = false;

        /// <summary>
        /// Projectile speed in WDist / tick,two values indicate a randomly picked velocity per beam.
        /// </summary>
        public readonly WDist[] Speed = { new WDist(128) };

        /// <summary>
        /// The maximum duration(in ticks) of each beam burst.
        /// </summary>
        public readonly int Duration = 10;

        /// <summary>
        /// The number of ticks between the beam causing warhead impacts in its area of effect.
        /// </summary>
        public readonly int DamageInterval = 3;

        /// <summary>
        /// The width of the beam.
        /// </summary>
        public readonly WDist Width = new WDist(512);


        /// <summary>
        /// The shape of the beam.Accepts values Cylindrical or Flat
        /// </summary>
        public readonly BeamRenderableShape Shape = BeamRenderableShape.Cylindrical;

        /// <summary>
        /// How far beyond the target the projectile keeps on travelling.
        /// </summary>
        public readonly WDist BeyondTargetRange = new WDist(0);

        /// <summary>
        /// Damage modifier applied at each range step.
        /// </summary>
        public readonly int[] Falloff = { 100, 100 };

        public readonly WDist[] Range = { WDist.Zero, new WDist(int.MaxValue) };

        /// <summary>
        /// Maximum offset at the maximum range.
        /// </summary>
        public readonly WDist Inaccuracy = WDist.Zero;

        public readonly bool Blockable = false;


        public readonly bool TrackTarget = false;

        /// <summary>
        /// Should the beam be visually rendered ? False = Beam is invisible.
        /// </summary>
        public readonly bool RenderBeam = true;

        /// <summary>
        /// Equivalent to sequence ZOffset,Controls Z sorting.
        /// </summary>
        public readonly int ZOffset = 0;


        public WDist BlockerScanRadius = new WDist(-1);


        public WDist AreaVictimScanRadius = new WDist(-1);
        
        public IProjectile Create(ProjectileArgs args)
        {
            var c = UsePlayerColor ? args.SourceActor.Owner.Color.RGB : Color;
            return new AreaBeam(this,args,c);
        }

        void IRulesetLoaded<WeaponInfo>.RulesetLoaded(Ruleset rules, WeaponInfo info)
        {
            if (BlockerScanRadius < WDist.Zero)
                BlockerScanRadius = Util.MinimumRequiredBlockerScanRadius(rules);

            if (AreaVictimScanRadius < WDist.Zero)
                AreaVictimScanRadius = Util.MinimumRequiredBlockerScanRadius(rules);
        }
    }


    public class AreaBeam:IProjectile,ISync
    {
        readonly AreaBeamInfo info;
        readonly ProjectileArgs args;
        readonly AttackBase actorAttackBase;
        readonly Color color;
        readonly WDist speed;

        [Sync]WPos headPos;
        [Sync]WPos tailPos;
        [Sync]WPos target;

        int towardsTargetFacing;
        int headTicks;
        int tailTicks;
        int length;

        bool isHeadTravelling = true;
        bool isTailTravelling;
        bool continueTracking = true;
        public AreaBeam(AreaBeamInfo info,ProjectileArgs args,Color color)
        {
            this.info = info;
            this.args = args;
            this.color = color;

            actorAttackBase = args.SourceActor.Trait<AttackBase>();

            var world = args.SourceActor.World;
            if (info.Speed.Length > 1)
                speed = new WDist(world.SharedRandom.Next(info.Speed[0].Length, info.Speed[1].Length));
            else
                speed = info.Speed[0];

            headPos = args.Source;
            tailPos = headPos;

            target = args.PassiveTarget;
            if(info.Inaccuracy.Length > 0)
            {
                var inaccuracy = Util.ApplyPercentageModifiers(info.Inaccuracy.Length, args.InaccuracyModifiers);
                var maxOffset = inaccuracy * (target - headPos).Length / args.Weapon.Range.Length;
                target += WVec.FromPDF(world.SharedRandom, 2) * maxOffset / 1024;

            }

            towardsTargetFacing = (target - headPos).Yaw.Facing;

            var dir = new WVec(0, -1024, 0).Rotate(WRot.FromFacing(towardsTargetFacing));

            target += dir * info.BeyondTargetRange.Length / 1024;

            length = Math.Max((target - headPos).Length / speed.Length, 1);

        }

        /// <summary>
        /// 追踪目标
        /// </summary>
        void TrackTarget()
        {
            if (!continueTracking)
                return;

            if (args.GuidedTarget.IsValidFor(args.SourceActor))
            {
                var guidedTargetPos = args.Weapon.TargetActorCenter ? args.GuidedTarget.CenterPosition : args.GuidedTarget.Positions.PositionClosestTo(args.Source);
                var targetDistance = new WDist((guidedTargetPos - args.Source).Length);

                //Only continue tracking target if it's within weapon range + BeyondTargetRange to avoid edge case stuttering (start firing and immediately stop again)
                //只有在武器范围+BeyondTargetRange 范围内才能继续追踪目标，以避免边缘情况下的 stuttering（开始射击并立即再次停止）
                if (targetDistance > args.Weapon.Range + info.BeyondTargetRange)
                    StopTargeting();
                else
                {
                    target = guidedTargetPos;
                    towardsTargetFacing = (target - args.Source).Yaw.Facing;

                    // Update the target position with the range we shoot beyond the target by
                    // I.e. we can deliberately overshoot, so aim for that position
                    // 更新目标位置，我们拍摄超出目标的范围.我们可以故意超调，所以喵准那个位置
                    var dir = new WVec(0, -1024, 0).Rotate(WRot.FromFacing(towardsTargetFacing));
                    target += dir * info.BeyondTargetRange.Length / 1024;
                }
            }
        }

        /// <summary>
        /// 停止追踪
        /// </summary>
        void StopTargeting()
        {
            continueTracking = false;
            isTailTravelling = true;
        }

        public void Tick(World world)
        {
            if (info.TrackTarget)
                TrackTarget();

            if (++headTicks >= length)
            {
                headPos = target;
                isHeadTravelling = false;
            }
            else if (isHeadTravelling)
                headPos = WPos.LerpQuadratic(args.Source, target, WAngle.Zero, headTicks, length);

            if(tailTicks<=0 && args.SourceActor.IsInWorld && !args.SourceActor.IsDead)
            {
                args.Source = args.CurrentSource();
                tailPos = args.Source;
            }

            var outOfWeaponRange = args.Weapon.Range + info.BeyondTargetRange < new WDist((args.PassiveTarget - args.Source).Length);

            //While the head is travelling,the tail must start to follow Duration ticks later.
            //Alternatively,also stop emitting the beam if source actor dies or is ordered to stop.
            if ((headTicks >= info.Duration && !isTailTravelling) || args.SourceActor.IsDead || !actorAttackBase.IsAniming || outOfWeaponRange)
                StopTargeting();

            if (isTailTravelling)
            {
                if (++tailTicks >= length)
                {
                    tailPos = target;
                    isTailTravelling = false;
                }
                else
                    tailPos = WPos.LerpQuadratic(args.Source, target, WAngle.Zero, tailTicks, length);
            }

            //Check for blocking actors
            WPos blockedPos;
            if(info.Blockable && BlocksProjectiles.AnyBlockingActorsBetween(world,tailPos,headPos,info.Width,info.BlockerScanRadius,out blockedPos))
            {
                headPos = blockedPos;
                target = headPos;
                length = Math.Min(headTicks, length);
            }

            //Damage is applied to intersected actors every DamageInterval ticks
            if(headTicks % info.DamageInterval == 0)
            {
                var actors = world.FindActorsOnLine(tailPos, headPos, info.Width, info.AreaVictimScanRadius);
                foreach(var actor in actors)
                {
                    var adjustModifiers = args.DamagedModifiers.Append(GetFalloff((args.Source - actor.CenterPosition).Length));
                    args.Weapon.Impact(Target.FromActor(actor), args.SourceActor, adjustModifiers);
                }
            }

            if (IsBeamComplete)
                world.AddFrameEndTask(w => w.Remove(this));

        }

        int GetFalloff(int distance)
        {
            var inner = info.Range[0].Length;

            for(var i = 1; i < info.Range.Length; i++)
            {
                var outer = info.Range[i].Length;
                if (outer > distance)
                    return Int2.Lerp(info.Falloff[i - 1], info.Falloff[i], distance - inner, outer - inner);

                inner = outer;
            }
            return 0;
        }

        bool IsBeamComplete { get { return !isHeadTravelling && headTicks >= length && !isTailTravelling && tailTicks >= length; } }

        public IEnumerable<IRenderable> Render(WorldRenderer wr)
        {
            if(!IsBeamComplete && info.RenderBeam && !(wr.World.FogObscures(tailPos) && wr.World.FogObscures(headPos)))
            {
                var beamRender = new BeamRenderable(headPos, info.ZOffset, tailPos - headPos, info.Shape, info.Width, color);
                return new[] { (IRenderable)beamRender };
                //yield return beamRender;
            }
            return SpriteRenderable.None;
        }
    }
}