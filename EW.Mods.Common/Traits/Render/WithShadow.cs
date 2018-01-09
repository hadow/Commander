using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using EW.Traits;
using EW.Graphics;
namespace EW.Mods.Common.Traits
{
    /// <summary>
    /// Clones the actor sprite with another palette below it.
    /// </summary>
    public class WithShadowInfo : ConditionalTraitInfo
    {

        [PaletteReference]
        public readonly string Palette = "shadow";

        /// <summary>
        /// Shadow position offset relative to actor position(ground level).
        /// </summary>
        public readonly WVec Offset = WVec.Zero;

        /// <summary>
        /// Shadow Z offset relative to actor sprite.
        /// </summary>
        public readonly int ZOffset = -5;

        public override object Create(ActorInitializer init)
        {
            return new WithShadow(this);
        }
    }
    public class WithShadow:ConditionalTrait<WithShadowInfo>,IRenderModifier
    {
        readonly WithShadowInfo info;
        public WithShadow(WithShadowInfo info) : base(info)
        {
            this.info = info;
        }

        IEnumerable<IRenderable> IRenderModifier.ModifyRender(Actor self, WorldRenderer wr, IEnumerable<IRenderable> r)
        {
            if (IsTraitDisabled)
                return r;

            var height = self.World.Map.DistanceAboveTerrain(self.CenterPosition).Length;

            var shadowSprites = r.Where(s => !s.IsDecoration)
                .Select(a => a.WithPalette(wr.Palette(info.Palette))
                .OffsetBy(info.Offset - new WVec(0, 0, height))
                .WithZOffset(a.ZOffset + (height + info.ZOffset))
                .AsDecoration());

            return shadowSprites.Concat(r);
        }


        IEnumerable<Rectangle> IRenderModifier.ModifyScreenBounds(Actor self, WorldRenderer wr, IEnumerable<Rectangle> bounds)
        {
            foreach (var r in bounds)
                yield return r;

            if (IsTraitDisabled)
                yield break;

            var height = self.World.Map.DistanceAboveTerrain(self.CenterPosition).Length;
            var offset = wr.ScreenPxOffset(info.Offset - new WVec(0, 0, height));
            foreach (var r in bounds)
                yield return new Rectangle(r.X + offset.X, r.Y + offset.Y, r.Width, r.Height);
        }
    }
}