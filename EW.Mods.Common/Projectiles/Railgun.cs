using System;
using System.Collections.Generic;
using EW.Traits;
using EW.Graphics;
namespace EW.Mods.Common.Projectiles
{
    public class RailgunInfo:IProjectileInfo
    {
        public IProjectile Create(ProjectileArgs args)
        {
            return new Railgun();
        }

    }

    public class Railgun:IProjectile
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