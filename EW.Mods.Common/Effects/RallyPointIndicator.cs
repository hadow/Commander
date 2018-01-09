using System;
using System.Collections.Generic;
using EW.Effects;
using EW.Graphics;
using EW.Mods.Common.Traits;

namespace EW.Mods.Common.Effects
{
    class RallyPointIndicator:IEffect,IEffectAboveShroud
    {

        readonly Actor building;
        readonly RallyPoint rp;
        readonly Animation flag;
        readonly Animation circles;
        readonly ExitInfo[] exits;

        readonly WPos[] targetLine = new WPos[2];

        CPos cachedLocation;

        public RallyPointIndicator(Actor building,RallyPoint rp,ExitInfo[] exits)
        {

            this.building = building;
            this.rp = rp;
            this.exits = exits;

        }




        void IEffect.Tick(World world)
        {

        }

        IEnumerable<IRenderable> IEffect.Render(WorldRenderer wr)
        {
            return SpriteRenderable.None;
        }


        IEnumerable<IRenderable> IEffectAboveShroud.RenderAboveShroud(WorldRenderer wr)
        {
            return RenderInner(wr);
        }

        IEnumerable<IRenderable> RenderInner(WorldRenderer wr)
        {

            var palette = wr.Palette(rp.PaletteName);

            if (circles != null)
                foreach (var r in circles.Render(targetLine[1], palette))
                    yield return r;

            if (flag != null)
                foreach (var r in flag.Render(targetLine[1], palette))
                    yield return r;
        }

    }
}