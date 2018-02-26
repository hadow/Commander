using System;
using System.Collections.Generic;
using System.Linq;
using EW.Effects;
using EW.Graphics;

namespace EW.Mods.Common.Effects
{
    public class FlashTarget:IEffect
    {

        Actor target;
        Player player;
        int remainingTicks;

        public FlashTarget(Actor target,Player asPlayer = null,int ticks = 4)
        {
            this.target = target;
            player = asPlayer;
            remainingTicks = ticks;
            target.World.RemoveAll(effect =>
            {
                var flashTarget = effect as FlashTarget;
                return flashTarget != null && flashTarget.target == target;
            });
        }


        void IEffect.Tick(World world)
        {

        }

        IEnumerable<IRenderable> IEffect.Render(WorldRenderer wr)
        {
            return SpriteRenderable.None;
        }

    }
}