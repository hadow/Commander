using System;
using System.Collections.Generic;
using EW.Graphics;
namespace EW.Traits
{

    public class ScreenMapInfo : ITraitInfo
    {
        public readonly int BinSize = 250;

        public object Create(ActorInitializer init) { return new ScreenMap(init.World,this); }
    }

    /// <summary>
    /// 
    /// </summary>
    public class ScreenMap:IWorldLoaded
    {
        WorldRenderer worldRenderer;

        public ScreenMap(World world,ScreenMapInfo info)
        {

        }
        public void WorldLoaded(World w,WorldRenderer wr)
        {
            worldRenderer = wr;
        }

    }
}