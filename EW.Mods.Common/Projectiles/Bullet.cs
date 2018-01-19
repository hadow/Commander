using System;
using System.Collections.Generic;
using EW.Graphics;
using EW.Traits;
using EW.Mods.Common.Graphics;
using System.Drawing;
namespace EW.Mods.Common.Projectiles
{

    public class BulletInfo : IProjectileInfo,IRulesetLoaded<WeaponInfo>
    {
        /// <summary>
        /// Projectile speed in WDist/tick,two values indicate variable velocity.
        /// </summary>
        public readonly WDist[] Speed = { new WDist(17) };

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

            //Check for walls or other blocking obstacles
            var shouldExplode = false;

            WPos blockedPos;

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
                    
                }

                var palette = wr.Palette(info.Palette);
                foreach(var r in anim.Render(pos,palette)){
                    yield return r;
                }
            }


        }

    }
}