using System;
using System.Collections.Generic;
using EW.Traits;
using EW.Graphics;
namespace EW.Effects
{
    public class DelayedImpact:IEffect
    {
        readonly Target target;
        readonly Actor firedBy;
        readonly IEnumerable<int> damageModifiers;
        readonly IWarHead wh;

        int delay;
        

        public DelayedImpact(int delay,IWarHead wh,Target target,Actor firedBy,IEnumerable<int> damageModifiers)
        {
            this.wh = wh;
            this.delay = delay;

            this.target = target;
            this.firedBy = firedBy;
            this.damageModifiers = damageModifiers;
        }
        public void Tick(World world)
        {

        }

        public IEnumerable<IRenderable> Render(WorldRenderer wr)
        {
            yield break;
        }

    }
}