using System;
using System.Collections.Generic;
using EW.Graphics;
namespace EW.Mods.Common.Projectiles
{

    public class BulletInfo : IProjectileInfo
    {
        public IProjectile Create(ProjectileArgs args) { return new Bullet(); }
    }
    public class Bullet:IProjectile,ISync
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