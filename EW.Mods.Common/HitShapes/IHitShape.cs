using System;
using EW.Graphics;

namespace EW.Mods.Common.HitShapes
{
    public interface IHitShape
    {

        WDist OuterRadius { get; }

        WDist DistanceFromEdge(WVec v);

        WDist DistanceFromEdge(WPos pos, Actor actor);

        void Initialize();

        void DrawCombatOverlay(WorldRenderer wr, RgbaColorRenderer acr, Actor actor);

    }
}