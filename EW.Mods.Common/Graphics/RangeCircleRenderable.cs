using System;
using System.Collections.Generic;
using EW.Graphics;
namespace EW.Mods.Common.Graphics
{
    public struct RangeCircleRenderable:IRenderable,IFinalizedRenderable
    {

        public IFinalizedRenderable PrepareRender(WorldRenderer wr) { return this; }
    }
}