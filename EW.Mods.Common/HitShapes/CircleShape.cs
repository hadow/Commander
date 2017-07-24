using System;
using EW.Graphics;
namespace EW.Mods.Common.HitShapes
{
    public class CircleShape:IHitShape
    {
        [FieldLoader.Require]
        public readonly WDist Radius = new WDist(426);
        public WDist OuterRadius { get { return Radius; } }

        public void Initialize() { }

        public CircleShape() { }

        public CircleShape(WDist radius) { Radius = radius; }

        public WDist DistanceFromEdge(WVec v)
        {
            return new WDist(Math.Max(0, v.Length-Radius.Length));
        }

        public WDist DistanceFromEdge(WPos pos,Actor actor)
        {
            return DistanceFromEdge(pos - actor.CenterPosition);
        }

        public void DrawCombatOverlay(WorldRenderer wr,RgbaColorRenderer wcr,Actor actor)
        {

        }
            
    }
}