using System;
using System.Collections.Generic;
using EW.Effects;
using EW.Graphics;
using EW.Mods.Common.Graphics;

namespace EW.Mods.Common.Effects
{
    public class ContrailFader:IEffect
    {
        WPos pos;
        ContrailRenderable trail;
        int ticks;

        public ContrailFader(WPos pos,ContrailRenderable trail)
        {
            this.pos = pos;
            this.trail = trail;
        }

        public void Tick(World world)
        {
            if (ticks++ == trail.Length)
                world.AddFrameEndTask(w => w.Remove(this));

            trail.Update(pos);
        }

        public IEnumerable<IRenderable> Render(WorldRenderer wr)
        {
            yield return trail;
        }
    }
}