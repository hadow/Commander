using System;
using System.Collections.Generic;
using EW.Graphics;
using EW.Traits;
using EW.Mods.Common.Graphics;
using EW.Mods.Common.Traits;
using System.Drawing;
using System.Linq;
using EW.Mods.Common.Effects;
namespace EW.Mods.Common.Projectiles
{

    public class BulletInfo : IProjectileInfo,IRulesetLoaded<WeaponInfo>
    {
        /// <summary>
        /// Projectile speed in WDist/tick,two values indicate variable velocity.
        /// </summary>
        public readonly WDist[] Speed = { new WDist(17) };

        public readonly Stance ValidBounceBlockerStances = Stance.Enemy | Stance.Neutral;
        /// <summary>
        /// Maximum offset at the maximum range.
        /// </summary>
        public readonly WDist Inaccuracy = WDist.Zero;

        /// <summary>
        /// Image to display
        /// </summary>
        public readonly string Image = null;

        /// <summary>
        /// Loop a randomly chosen sequence of Image form this list while this projecile is moving.
        /// </summary>
        [SequenceReference("Image")]
        public readonly string[] Sequences = { "idle" };

        /// <summary>
        /// The Palette used to draw this projectile.
        /// </summary>
        [PaletteReference]
        
        public readonly string Palette = "effect";

        /// <summary>
        /// Does this projectile have a shadow.
        /// </summary>
        public readonly bool Shadow = false;

        /// <summary>
        /// Palette to use for this projectile's shadow if Shadow is true.
        /// </summary>
        [PaletteReference]
        public readonly string ShadowPalette = "shadow";

        /// <summary>
        /// Trail animation
        /// </summary>
        public readonly string TrailImage = null;

        [SequenceReference("TrailImage")]
        /// <summary>
        /// Loop a randomly chosen sequence of TrailImage from this list while this projectile is moving.
        /// </summary>
        public readonly string[] TrailSequence = { "idle" };

        /// <summary>
        /// Is this blocked by actors with BlocksProjectiles trait.
        /// </summary>
        public readonly bool Blockable = true;

        /// <summary>
        /// Width of projectile (used for finding blocking actors)
        /// </summary>
        public readonly WDist Width = new WDist(1);

        /// <summary>
        /// The launch angle.
        /// </summary>
        public readonly WAngle[] LaunchAngle = { WAngle.Zero };

        /// <summary>
        /// Up to how many times does this bullet bounce when touching ground without hitting a target.
        /// 0 implies explosing on contact with the originally targeted position.
        /// </summary>
        public readonly int BounceCount = 0;

        /// <summary>
        /// Modify distance of each bounce by the percentage of previous distance.
        /// </summary>
        public readonly int BounceRangeModifier = 60;

        /// <summary>
        /// Interval in ticks between each spawned Trail animation.
        /// </summary>
        public readonly int TrailInterval = 2;

        /// <summary>
        /// Delay in ticks between each spawned Trail animation.
        /// </summary>
        public readonly int TrailDelay = 1;

        [PaletteReference("TrailUsePlayerPalette")]
        /// <summary>
        /// Palette used to render the trail sequence.
        /// </summary>
        public readonly string TrailPalette = "effect";


        public readonly WDist AirburstAltitude = WDist.Zero;//空中高度

        /// <summary>
        /// Use the Player Palette to render the trail sequence.
        /// </summary>
        public readonly bool TrailUsePlayerPalette = false;

        //轨迹
        public readonly int ContrailLength = 0;
        public readonly int ContrailZOffset = 2047;
        public readonly Color ContrailColor = Color.White;
        public readonly bool ContrailUsePlayerColor = false;
        public readonly int ContrailDelay = 1;
        public readonly WDist ContrailWidth = new WDist(64);


        public WDist BlockerScanRadius = new WDist(-1);
        public WDist BounceBlockerScanRadius = new WDist(-1);



        public IProjectile Create(ProjectileArgs args) { return new Bullet(this,args); }


        void IRulesetLoaded<WeaponInfo>.RulesetLoaded(Ruleset rules,WeaponInfo wi){

            if (BlockerScanRadius < WDist.Zero)
                BlockerScanRadius = Util.MinimumRequiredBlockerScanRadius(rules);

            if (BounceBlockerScanRadius < WDist.Zero)
                BounceBlockerScanRadius = Util.MinimumRequiredBlockerScanRadius(rules);
        }
    }


    public class Bullet:IProjectile,ISync
    {
        readonly BulletInfo info;
        readonly ProjectileArgs args;
        readonly Animation anim;
        [Sync]
        readonly WAngle angle;
        [Sync]
        readonly WDist speed;

        string trailPalette;

        ContrailRenderable contrail;


        [Sync]
        WPos pos, target, source;
        int length;
        [Sync]
        int facing;

        int ticks, smokeTicks;

        int remainingBounces;

        public Actor SourceActor { get { return args.SourceActor; } }

        public Bullet(BulletInfo info,ProjectileArgs args)
        {
            this.info = info;
            this.args = args;
            pos = args.Source;
            source = args.Source;

            var world = args.SourceActor.World;

            if (info.LaunchAngle.Length > 1)
            {
                angle = new WAngle(world.SharedRandom.Next(info.LaunchAngle[0].Angle, info.LaunchAngle[1].Angle));
            }
            else
                angle = info.LaunchAngle[0];

            if (info.Speed.Length > 1)
            {
                speed = new WDist(world.SharedRandom.Next(info.Speed[0].Length, info.Speed[1].Length));
            }
            else
                speed = info.Speed[0];


            target = args.PassiveTarget;

            if(info.Inaccuracy.Length>0){

                var inaccuracy = Util.ApplyPercentageModifiers(info.Inaccuracy.Length, args.InaccuracyModifiers);
                var range = Util.ApplyPercentageModifiers(args.Weapon.Range.Length, args.RangeModifiers);
                var maxOffset = inaccuracy * (target - pos).Length / range;
                target += WVec.FromPDF(world.SharedRandom, 2) * maxOffset / 1024;
            }

            if(info.AirburstAltitude>WDist.Zero){
                target += new WVec(WDist.Zero, WDist.Zero, info.AirburstAltitude);
            }

            facing = (target - pos).Yaw.Facing;
            length = Math.Max((target - pos).Length / speed.Length, 1);

            if(!string.IsNullOrEmpty(info.Image)){
                anim = new Animation(world, info.Image, new Func<int>(GetEffectiveFacing));
                anim.PlayRepeating(info.Sequences.Random(world.SharedRandom));
            }

            if(info.ContrailLength>0){

                var color = info.ContrailUsePlayerColor ? ContrailRenderable.ChooseColor(args.SourceActor) : info.ContrailColor;
                contrail = new ContrailRenderable(world, color, info.ContrailWidth, info.ContrailLength, info.ContrailDelay, info.ContrailZOffset);
            }

            trailPalette = info.TrailPalette;
            if (info.TrailUsePlayerPalette)
                trailPalette += args.SourceActor.Owner.InternalName;

            smokeTicks = info.TrailDelay;
            remainingBounces = info.BounceCount;



        }


        int GetEffectiveFacing(){

            var at = (float)ticks / (length - 1);
            var attitude = angle.Tan() * (1 - 2 * at) / (4 * 1024);

            var u = (facing % 128) / 128f;
            var scale = 512 * u * (1 - u);

            return (int)(facing < 128 ? facing - scale * attitude : facing + scale * attitude);

        }
        public void Tick(World world)
        {
            if (anim != null)
                anim.Tick();

            var lastPos = pos;
            pos = WPos.LerpQuadratic(source, target, angle, ticks, length);


            //Check for walls or other blocking obstacles
            var shouldExplode = false;
            WPos blockedPos;
            if(info.Blockable && BlocksProjectiles.AnyBlockingActorsBetween(world,lastPos,pos,info.Width,info.BlockerScanRadius,out blockedPos))
            {
                pos = blockedPos;
                shouldExplode = true;
            }

            if(!string.IsNullOrEmpty(info.TrailImage) && --smokeTicks < 0)
            {
                var delayedPos = WPos.LerpQuadratic(source, target, angle, ticks - info.TrailDelay, length);
                world.AddFrameEndTask(w => w.Add(new SpriteEffect(delayedPos, w, info.TrailImage, info.TrailSequence.Random(world.SharedRandom),
                    trailPalette, false, false, GetEffectiveFacing())));

                smokeTicks = info.TrailInterval;
            }

            if (info.ContrailLength > 0)
                contrail.Update(pos);

            var flightLengthReached = ticks++ >= length;
            var shouldBounce = remainingBounces > 0;

            if(flightLengthReached && shouldBounce)
            {
                shouldExplode |= AnyValidTargetsInRadius(world, pos, info.Width + info.BounceBlockerScanRadius, args.SourceActor, true);
                target += (pos - source) * info.BounceRangeModifier / 100;
                var dat = world.Map.DistanceAboveTerrain(target);
                target += new WVec(0, 0, -dat.Length);
                length = Math.Max((target - pos).Length / speed.Length, 1);
                ticks = 0;
                source = pos;
                remainingBounces--;
            }

            //Flight length reached/exceeded
            shouldExplode |= flightLengthReached && !shouldBounce;

            //Driving into cell with higher height level
            shouldExplode |= world.Map.DistanceAboveTerrain(pos).Length < 0;

            if (remainingBounces < info.BounceCount)
                shouldExplode |= AnyValidTargetsInRadius(world, pos, info.Width + info.BounceBlockerScanRadius, args.SourceActor, true);

            if (shouldExplode)
                Explode(world);

        }

        void Explode(World world)
        {
            if (info.ContrailLength > 0)
                world.AddFrameEndTask(w => w.Add(new ContrailFader(pos, contrail)));

            world.AddFrameEndTask(w => w.Remove(this));

            args.Weapon.Impact(Target.FromPos(pos), args.SourceActor, args.DamagedModifiers);
        }


        bool AnyValidTargetsInRadius(World world,WPos pos,WDist radius,Actor firedBy,bool checkTargetType)
        {
            foreach(var victim in world.FindActorsInCircle(pos, radius))
            {
                if (checkTargetType && !Target.FromActor(victim).IsValidFor(firedBy))
                    continue;

                if (!info.ValidBounceBlockerStances.HasStance(victim.Owner.Stances[firedBy.Owner]))
                    continue;

                //If the impact position is within anyh actor's Hitshape,we have a direct hit
                var activeShapes = victim.TraitsImplementing<HitShape>().Where(Exts.IsTraitEnabled);
                if (activeShapes.Any(i => i.Info.Type.DistanceFromEdge(pos, victim).Length <= 0))
                    return true;
            }

            return false;
        }

        public IEnumerable<IRenderable> Render(WorldRenderer wr)
        {

            if (info.ContrailLength > 0)
                yield return contrail;

            if (anim == null || ticks > length)
                yield break;

            var world = args.SourceActor.World;
            if(!world.FogObscures(pos)){
                
                if(info.Shadow){

                    var dat = world.Map.DistanceAboveTerrain(pos);
                    var shadowPos = pos - new WVec(0, 0, dat.Length);
                    foreach (var r in anim.Render(shadowPos, wr.Palette(info.ShadowPalette)))
                        yield return r;
                }

                var palette = wr.Palette(info.Palette);
                foreach(var r in anim.Render(pos,palette)){
                    yield return r;
                }
            }


        }

    }
}