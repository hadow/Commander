using System;
using System.Collections.Generic;
using System.Drawing;

using EW.Graphics;
namespace EW.Mods.Common.Projectiles
{
    public class AreaBeamInfo : IProjectileInfo
    {

        public readonly Color Color = Color.Red;
        public readonly bool UsePlayerColor = false;

        public IProjectile Create(ProjectileArgs args)
        {
            var c = UsePlayerColor ? args.SourceActor.Owner.Color.RGB : Color;
            return new AreaBeam(this,args,c);
        }
    }


    public class AreaBeam:IProjectile,ISync
    {

        public AreaBeam(AreaBeamInfo info,ProjectileArgs args,Color color)
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