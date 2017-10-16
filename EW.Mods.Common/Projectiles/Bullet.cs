using System;
using System.Collections.Generic;
using EW.Graphics;
namespace EW.Mods.Common.Projectiles
{

    public class BulletInfo : IProjectileInfo
    {
        public readonly WDist[] Speed = { new WDist(17) };

        /// <summary>
        /// Maximum offset at the maximum range.
        /// </summary>
        public readonly WDist Inaccuracy = WDist.Zero;

        public readonly string Image = null;

        
        public readonly string[] Sequences = { "idle" };

        
        public readonly string Palette = "effect";

        public readonly bool Shadow = false;

        public readonly string ShadowPalette = "shadow";

        public readonly string TrailImage = null;

        public readonly string[] TrailSequence = { "idle" };

        /// <summary>
        /// Is this blocked by actors with BlocksProjectiles trait.
        /// </summary>
        public readonly bool Blockable = true;

        public readonly WDist Width = new WDist(1);

        public readonly int BounceCount = 0;

        public readonly int BounceRangeModifier = 60;

        public readonly int TrailInterval = 2;

        public readonly int TrailDelay = 1;

        public readonly string TrailPalette = "effect";

        public readonly WDist AirburstAltitude = WDist.Zero;//垂直高度

        /// <summary>
        /// Use the Player Palette to render the trail sequence.
        /// </summary>
        public readonly bool TrailUsePlayerPalette = false;

        public readonly int ContrailLength = 0;
        public readonly int ContrailZOffset = 2047;
        public readonly bool ContrailUsePlayerColor = false;
        public readonly int ContrailDelay = 1;
        public readonly WDist ContrailWidth = new WDist(64);

        public WDist BounceBlockerScanRadius = new WDist(-1);



        public IProjectile Create(ProjectileArgs args) { return new Bullet(this,args); }
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

            var wold = args.SourceActor.World;
        }
        public void Tick(World world)
        {

        }

        public IEnumerable<IRenderable> Render(WorldRenderer wr)
        {
            yield return null;
        }

    }
}