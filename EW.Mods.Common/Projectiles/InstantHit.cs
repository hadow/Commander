using System;
using System.Linq;
using System.Collections.Generic;
using EW.Graphics;
using EW.Traits;
using EW.Mods.Common.Traits;
namespace EW.Mods.Common.Projectiles
{
    /// <summary>
    /// Simple,invisible,usually direct-on-target projectile.
    /// </summary>
    public class InstantHitInfo:IProjectileInfo,IRulesetLoaded<WeaponInfo>
    {
        /// <summary>
        /// Maximum offset at the maximum range.
        /// </summary>
        public readonly WDist Inaccuracy = WDist.Zero;

        /// <summary>
        /// Projectile can be blocked.
        /// </summary>
        public readonly bool Blockable = false;

        /// <summary>
        /// The width of the projectile.
        /// </summary>
        public readonly WDist Width = new WDist(1);

        public WDist BlockerScanRadius = new WDist(-1);


        public IProjectile Create(ProjectileArgs args)
        {
            return new InstantHit(this, args);
        }

        void IRulesetLoaded<WeaponInfo>.RulesetLoaded(Ruleset rules,WeaponInfo wi){

            if (BlockerScanRadius < WDist.Zero)
                BlockerScanRadius = Util.MinimumRequiredBlockerScanRadius(rules);
        }
    }


    public class InstantHit : IProjectile
    {
        readonly ProjectileArgs args;

        readonly InstantHitInfo info;

        Target target;
        WPos source;

        public InstantHit(InstantHitInfo info,ProjectileArgs args)
        {
            this.args = args;
            this.info = info;
            source = args.Source;


            if (args.Weapon.TargetActorCenter)
            {
                target = args.GuidedTarget;

            }
            else if (info.Inaccuracy.Length > 0)
            {
                var inaccuracy = Util.ApplyPercentageModifiers(info.Inaccuracy.Length, args.InaccuracyModifiers);
                var maxOffset = inaccuracy * (args.PassiveTarget - source).Length / args.Weapon.Range.Length;
                target = Target.FromPos(args.PassiveTarget + WVec.FromPDF(args.SourceActor.World.SharedRandom, 2) * maxOffset / 1024);
            }
            else
                target = Target.FromPos(args.PassiveTarget);

        }


        public void Tick(World world)
        {
            //Check for blocking actors.
            WPos blockedPos;
            if(info.Blockable && BlocksProjectiles.AnyBlockingActorsBetween(world,source,target.CenterPosition,info.Width,info.BlockerScanRadius,out blockedPos) ){
                target = Target.FromPos(blockedPos);
            }

            args.Weapon.Impact(target,args.SourceActor,args.DamagedModifiers);
            world.AddFrameEndTask(w=>w.Remove(this));
        }


        public IEnumerable<IRenderable> Render(WorldRenderer wr)
        {
            return Enumerable.Empty<IRenderable>();
        }
    }
}