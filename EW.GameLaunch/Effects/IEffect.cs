using System;
using System.Collections.Generic;
using EW.Graphics;

namespace EW.Effects
{
    public interface IEffect
    {

        void Tick(World world);

        IEnumerable<IRenderable> Render(WorldRenderer wr);

    }

    public interface IEffectAboveShroud{
        IEnumerable<IRenderable> RenderAboveShroud(WorldRenderer wr);
    }

    public interface ISpatiallyPartitionable{}


}