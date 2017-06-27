using System;
using EW.Xna.Platforms;
using EW.Graphics;
using EW.Traits;
namespace EW.Mods.Common.Traits
{


    public class ScreenShakerInfo : ITraitInfo
    {
        public readonly Vector2 MinMultiplier = new Vector2(-3, -3);
        public readonly Vector2 MaxMultiplier = new Vector2(3, 3);


        public object Create(ActorInitializer init) { return new ScreenShaker(this); }
    }

    public class ScreenShaker:ITick,IWorldLoaded
    {
        readonly ScreenShakerInfo info;
        WorldRenderer worldRenderer;

        public ScreenShaker(ScreenShakerInfo info)
        {
            this.info = info;
        }


        public void WorldLoaded(World w,WorldRenderer wr)
        {
            worldRenderer = wr;
        }


        public void Tick(Actor self)
        {

        }

    }
}