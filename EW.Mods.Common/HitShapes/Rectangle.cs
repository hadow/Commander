using System;
using System.Collections.Generic;
using EW.Graphics;
using EW.Primitives;
namespace EW.Mods.Common.HitShapes
{
    public class RectangleShape:IHitShape
    {

        public WDist OuterRadius { get; private set; }


        
        public WDist DistanceFromEdge(WVec v)
        {
            return WDist.Zero;
        }


        public WDist DistanceFromEdge(WPos pos,Actor actor)
        {
            return WDist.Zero;
        }

        public void Initialize()
        {

        }

        public void DrawCombatOverlay(WorldRenderer wr,RgbaColorRenderer wcr,Actor actor)
        {

        }


    }
}