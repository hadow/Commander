using System;
using System.Linq;
using System.Collections.Generic;
using EW.Graphics;

namespace EW.Mods.Common.Projectiles
{
    public class InstantHitInfo:IProjectileInfo
    {


        public IProjectile Create(ProjectileArgs args)
        {
            return new InstantHit(this, args);
        }
    }


    public class InstantHit : IProjectile
    {
        public InstantHit(InstantHitInfo info,ProjectileArgs args)
        {

        }


        public void Tick(World world)
        {

        }


        public IEnumerable<IRenderable> Render(WorldRenderer wr)
        {
            return Enumerable.Empty<IRenderable>();
        }
    }
}