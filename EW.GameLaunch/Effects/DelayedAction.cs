using System;
using System.Collections.Generic;
using EW.Graphics;

namespace EW.Effects
{
    public class DelayedAction:IEffect
    {

        Action a;
        int delay;

        public DelayedAction(int delay,Action a)
        {
            this.a = a;
            this.delay = delay;
        }


        public void Tick(World world)
        {
            if (--delay <= 0)
                world.AddFrameEndTask(w => { w.Remove(this); a(); });
        }



        public IEnumerable<IRenderable> Render(WorldRenderer wr)
        {
            yield break;
        }


    }
}