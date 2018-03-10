using System;
using System.Collections.Generic;
using EW.Graphics;
using EW.Effects;
namespace EW.Mods.Common.Effects
{
    public class CrateEffect:IEffect,ISpatiallyPartitionable
    {

        readonly string palette;
        readonly Actor a;
        readonly Animation anim;


        public CrateEffect(Actor a, string seq, string palette)
        {
            this.a = a;
            this.palette = palette;

            anim = new Animation(a.World, "crate-effects");
            anim.PlayThen(seq, () => a.World.AddFrameEndTask(w => { w.Remove(this); w.ScreenMap.Remove(this); }));
            a.World.ScreenMap.Add(this, a.CenterPosition, anim.Image);
        }


        void IEffect.Tick(World world){

            anim.Tick();
            world.ScreenMap.Update(this, a.CenterPosition, anim.Image);
        }

        public IEnumerable<IRenderable> Render(WorldRenderer wr)
        {
            if (!a.IsInWorld || a.World.FogObscures(a.CenterPosition))
                return SpriteRenderable.None;

            return anim.Render(a.CenterPosition, wr.Palette(palette));
        }
    }
}
