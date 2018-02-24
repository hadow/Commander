using System;
using EW.Graphics;
namespace EW.Mods.Common.HitShapes
{
    public class CircleShape:IHitShape
    {
        [FieldLoader.Require]
        public readonly WDist Radius = new WDist(426);
        public WDist OuterRadius { get { return Radius; } }

        [Desc("Defines the top offset relative to the actor's target point")]
        public readonly int VerticalTopOffset = 0;

        [Desc("Defines the bottom offset relative to the actor's target point")]
        public readonly int VerticalBottomOffset = 0;

        public void Initialize()
        {
            if (VerticalTopOffset < VerticalBottomOffset)
                throw new YamlException("VerticalTopOffset must be equal to or higher than VerticalBottomOffset.");
        }

        public CircleShape() { }

        public CircleShape(WDist radius) { Radius = radius; }

        public WDist DistanceFromEdge(WVec v)
        {
            return new WDist(Math.Max(0, v.Length-Radius.Length));
        }

        public WDist DistanceFromEdge(WPos pos,Actor actor)
        {
            var actorPos = actor.CenterPosition;

            if (pos.Z > actorPos.Z + VerticalTopOffset)
                return DistanceFromEdge(pos - (actorPos + new WVec(0, 0, VerticalTopOffset)));

            if (pos.Z < actorPos.Z + VerticalBottomOffset)
                return DistanceFromEdge(pos - (actorPos + new WVec(0, 0, VerticalBottomOffset)));

            return DistanceFromEdge(pos - actor.CenterPosition);
        }

        public void DrawCombatOverlay(WorldRenderer wr,RgbaColorRenderer wcr,Actor actor)
        {

        }
            
    }
}