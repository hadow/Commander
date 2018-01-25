using System;
using System.Collections.Generic;
using System.Linq;
using EW.Graphics;
using EW.Primitives;
using EW.Framework;
namespace EW.Mods.Common.HitShapes
{
    public class RectangleShape:IHitShape
    {

        public WDist OuterRadius { get; private set; }

        [FieldLoader.Require]
        public readonly Int2 TopLeft;

        [FieldLoader.Require]
        public readonly Int2 BottomRight;


        public readonly int VerticalTopOffset = 0;

        public readonly int VerticalBottomOffset = 0;

        public readonly WAngle LocalYaw = WAngle.Zero;

        Int2 quadrantSize;

        Int2 center;

        WVec[] combatOverlayVertsTop;
        WVec[] combatOverlayVertsBottom;


        public void Initialize()
        {
            if (TopLeft.X >= BottomRight.X || TopLeft.Y >= BottomRight.Y)
                throw new YamlException("TopLeft and BottomRight points are invalid.");

            if (VerticalTopOffset < VerticalBottomOffset)
                throw new YamlException("VerticalTopOffset must be equal to or higher than VerticalBottomOffset.");

            quadrantSize = (BottomRight - TopLeft) / 2;
            center = TopLeft + quadrantSize;

            var topRight = new Int2(BottomRight.X, TopLeft.Y);
            var bottomLeft = new Int2(TopLeft.X, BottomRight.Y);
            var corners = new[] { TopLeft, BottomRight, topRight, bottomLeft };
            OuterRadius = new WDist(corners.Select(x => x.Length).Max());

            combatOverlayVertsTop = new WVec[]
            {
                new WVec(TopLeft.X,TopLeft.Y,VerticalTopOffset),
                new WVec(BottomRight.X,TopLeft.Y,VerticalTopOffset),
                new WVec(BottomRight.X,BottomRight.Y,VerticalTopOffset),
                new WVec(TopLeft.X,BottomRight.Y,VerticalTopOffset)
            
            };

            combatOverlayVertsBottom = new WVec[]
            {
                new WVec(TopLeft.X,TopLeft.Y,VerticalBottomOffset),
                new WVec(BottomRight.X,TopLeft.Y,VerticalBottomOffset),
                new WVec(BottomRight.X,BottomRight.Y,VerticalBottomOffset),
                new WVec(TopLeft.X,BottomRight.Y,VerticalBottomOffset),
            };

        }




        public WDist DistanceFromEdge(WVec v)
        {
            var r = new WVec(
                Math.Max(Math.Abs(v.X - center.X) - quadrantSize.X, 0),
                Math.Max(Math.Abs(v.Y - center.Y) - quadrantSize.Y, 0), 0);

            return new WDist(r.HorizontalLength);
        }


        public WDist DistanceFromEdge(WPos pos,Actor actor)
        {
            var actorPos = actor.CenterPosition;
            var orientation = actor.Orientation + WRot.FromYaw(LocalYaw);

            if (pos.Z > actorPos.Z + VerticalTopOffset)
                return DistanceFromEdge((pos - (actorPos + new WVec(0, 0, VerticalTopOffset))).Rotate(-orientation));

            if (pos.Z < actorPos.Z + VerticalBottomOffset)
                return DistanceFromEdge((pos - (actorPos + new WVec(0, 0, VerticalBottomOffset))).Rotate(-orientation));

            return DistanceFromEdge((pos - new WPos(actorPos.X, actorPos.Y, pos.Z)).Rotate(-orientation));
        }

        

        public void DrawCombatOverlay(WorldRenderer wr,RgbaColorRenderer wcr,Actor actor)
        {

        }


    }
}