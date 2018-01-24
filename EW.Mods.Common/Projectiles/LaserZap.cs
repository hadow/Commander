using System;
using System.Collections.Generic;
using EW.Graphics;

namespace EW.Mods.Common.Projectiles
{

    public class LaserZapInfo : IProjectileInfo
    {
        public IProjectile Create(ProjectileArgs args) { return new LaserZap(); }
    }

    /// <summary>
    /// Laser zap.
    /// 击光打击
    /// </summary>
    public class LaserZap:IProjectile,ISync
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