using System;
using System.Drawing;
using System.Collections.Generic;
using EW.Graphics;
using EW.Traits;
using EW.Mods.Common.Traits;
using EW.Mods.Common.Graphics;
namespace EW.Mods.Common.Projectiles
{

    public class LaserZapInfo : IProjectileInfo,IRulesetLoaded<WeaponInfo>
    {
        public readonly BeamRenderableShape Shape = BeamRenderableShape.Cylindrical;

        public readonly WDist Width = new WDist(86);

        public readonly WDist Inaccuracy = WDist.Zero;

        public readonly bool TrackTarget = true;

        public readonly bool Blockable = false;

        public readonly int ZOffset = 0;

        public readonly int Duration = 10;

        public readonly bool UsePlayerColor = false;

        public readonly Color Color = Color.Red;

        /// <summary>
        /// Draw a second beam(for 'glow' effect)
        /// </summary>
        public readonly bool SecondaryBeam = false;

        public readonly bool SecondaryBeamUsePlayerColor = false;

        public readonly Color SecondaryBeamColor = Color.Red;

        public readonly int SecondaryBeamZOffset = 0;

        /// <summary>
        /// The shape of the beam.
        /// </summary>
        public readonly BeamRenderableShape SecondaryBeamShape = BeamRenderableShape.Cylindrical;

        /// <summary>
        /// The width of the zap.
        /// </summary>
        public readonly WDist SecondaryBeamWidth = new WDist(86);

        public readonly string HitAnim = null;

        /// <summary>
        /// Sequence of impact animation to use.
        /// </summary>
        [SequenceReference("HitAnim")]
        public readonly string HitAnimSequence = "idle";

        [PaletteReference]
        public readonly string HitAnimPalette = "effect";

        public WDist BlockerScanRadius = new WDist(-1);

        public IProjectile Create(ProjectileArgs args)
        {
            var c = !UsePlayerColor ? args.SourceActor.Owner.Color.RGB : Color;
            return new LaserZap(this,args,c);
        }

        void IRulesetLoaded<WeaponInfo>.RulesetLoaded(Ruleset rules, WeaponInfo info)
        {
            if (BlockerScanRadius < WDist.Zero)
                BlockerScanRadius = Util.MinimumRequiredBlockerScanRadius(rules);
        }
    }

    /// <summary>
    /// Laser zap.
    /// 击光打击
    /// </summary>
    public class LaserZap:IProjectile,ISync
    {

        readonly ProjectileArgs args;
        readonly LaserZapInfo info;
        readonly Animation hitanim;
        readonly Color color;
        readonly Color secondaryColor;

        int ticks = 0;
        bool doneDamage;
        bool animationComplete;

        [Sync]
        WPos target;
        [Sync]
        WPos source;

        public LaserZap(LaserZapInfo info,ProjectileArgs args,Color color)
        {
            this.args = args;
            this.info = info;
            this.color = color;

            secondaryColor = info.SecondaryBeamUsePlayerColor ? args.SourceActor.Owner.Color.RGB : info.SecondaryBeamColor;
            target = args.PassiveTarget;
            source = args.Source;

            if (info.Inaccuracy.Length > 0)
            {
                var inaccuracy = Util.ApplyPercentageModifiers(info.Inaccuracy.Length, args.InaccuracyModifiers);
                var maxOffset = inaccuracy * (target - source).Length / args.Weapon.Range.Length;
                target += WVec.FromPDF(args.SourceActor.World.SharedRandom, 2) * maxOffset / 1024;

            }

            if (!string.IsNullOrEmpty(info.HitAnim))
                hitanim = new Animation(args.SourceActor.World, info.HitAnim);
        }


        public void Tick(World world)
        {
            //Beam tracks target
            if (info.TrackTarget && args.GuidedTarget.IsValidFor(args.SourceActor))
                target = args.Weapon.TargetActorCenter ? args.GuidedTarget.CenterPosition : args.GuidedTarget.Positions.PositionClosestTo(source);

            //check for blocking actors
            WPos blockedPos;
            if(info.Blockable && BlocksProjectiles.AnyBlockingActorsBetween(world,source,target,info.Width,info.BlockerScanRadius,out blockedPos))
            {
                target = blockedPos;
            }

            if (!doneDamage)
            {
                if (hitanim != null)
                    hitanim.PlayThen(info.HitAnimSequence, () => animationComplete = true);
                else
                    animationComplete = true;

                args.Weapon.Impact(Target.FromPos(target), args.SourceActor, args.DamagedModifiers);
                doneDamage = true;
            }

            if (hitanim != null)
                hitanim.Tick();

            if (++ticks >= info.Duration && animationComplete)
                world.AddFrameEndTask(w => w.Remove(this));
        }


        public IEnumerable<IRenderable> Render(WorldRenderer wr)
        {
            if (wr.World.FogObscures(target) &&
                wr.World.FogObscures(args.Source))
                yield break;

            if (ticks < info.Duration)
            {
                var rc = Color.FromArgb((info.Duration - ticks) * color.A / info.Duration, color);
                yield return new BeamRenderable(args.Source, info.ZOffset, target - args.Source, info.Shape, info.Width, rc);

                if(info.SecondaryBeam)
                {
                    var src = Color.FromArgb((info.Duration - ticks) * secondaryColor.A / info.Duration, secondaryColor);
                    yield return new BeamRenderable(args.Source, info.SecondaryBeamZOffset, target - args.Source,
                        info.SecondaryBeamShape, info.SecondaryBeamWidth, src);
                }
            }

            if (hitanim != null)
                foreach (var r in hitanim.Render(target, wr.Palette(info.HitAnimPalette)))
                    yield return r;
        }

    }
}