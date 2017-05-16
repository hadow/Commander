using System;
using System.Collections.Generic;
using EW.Graphics;

namespace EW.Mods.Common.Projectiles
{

    public class GravityBombInfo : IProjectileInfo
    {

        public IProjectile Create(ProjectileArgs args)
        {
            return new GravityBomb();
        }
    }
    public class GravityBomb:IProjectile,ISync
    {
        public void Tick(World world)
        {

        }

        public IEnumerable<IRenderable> Render(WorldRenderer wr)
        {
            yield return null;
        }
    }
}