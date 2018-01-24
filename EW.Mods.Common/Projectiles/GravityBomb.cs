using System;
using System.Collections.Generic;
using EW.Graphics;

namespace EW.Mods.Common.Projectiles
{

    public class GravityBombInfo : IProjectileInfo
    {

        public IProjectile Create(ProjectileArgs args)
        {
            return new GravityBomb(this,args);
        }
    }
    public class GravityBomb:IProjectile,ISync
    {


        public GravityBomb(GravityBombInfo info,ProjectileArgs args)
        {
            
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