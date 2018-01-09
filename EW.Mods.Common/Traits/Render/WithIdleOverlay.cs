using System;
using System.Collections.Generic;
using EW.Traits;
using EW.Mods.Common.Graphics;
using EW.Graphics;
namespace EW.Mods.Common.Traits.Render
{

    /// <summary>
    /// Renders a decorative animation on units and buildings
    /// </summary>
    public class WithIdleOverlayInfo : PausableConditionalTraitInfo,IRenderActorPreviewSpritesInfo,Requires<RenderSpritesInfo>,Requires<BodyOrientationInfo>
    {
        /// <summary>
        /// Animation to play when the actor is created.
        /// </summary>
        [SequenceReference]
        public readonly string StartSequence = null;

        [SequenceReference]
        public readonly string Sequence = "idle-overlay";

        /// <summary>
        /// Position relative to body.
        /// </summary>
        public readonly WVec Offset = WVec.Zero;

        [PaletteReference("IsPlayerPalette")]
        public readonly string Palette = null;

        public readonly bool IsPlayerPalette = false;

        public readonly bool RenderBeforeBuildComplete = false;

        public override object Create(ActorInitializer init)
        {
            return new WithIdleOverlay(init.Self, this);
        }
        public IEnumerable<IActorPreview> RenderPreviewSprites(ActorPreviewInitializer init,RenderSpritesInfo rs,string image,int facings,PaletteReference p)
        {
            throw new NotImplementedException();
        }
    }
    public class WithIdleOverlay:PausableConditionalTrait<WithIdleOverlayInfo>,INotifyBuildComplete
    {
        readonly Animation overlay;
        bool buildComplete;
        
        public WithIdleOverlay(Actor self,WithIdleOverlayInfo info) : base(info)
        {
            var rs = self.Trait<RenderSprites>();
            var body = self.Trait<BodyOrientation>();

            buildComplete = !self.Info.HasTraitInfo<BuildingInfo>();//always render instantly for units.
            overlay = new Animation(self.World, rs.GetImage(self), () => IsTraitPaused || (!info.RenderBeforeBuildComplete && !buildComplete));

            if (info.StartSequence != null)
                overlay.PlayThen(RenderSprites.NormalizeSequence(overlay, self.GetDamageState(), info.StartSequence),
                    () => overlay.PlayRepeating(RenderSprites.NormalizeSequence(overlay, self.GetDamageState(), info.Sequence)));
            else
                overlay.PlayRepeating(RenderSprites.NormalizeSequence(overlay, self.GetDamageState(), info.Sequence));

            var anim = new AnimationWithOffset(overlay,
                () => body.LocalToWorld(info.Offset.Rotate(body.QuantizeOrientation(self, self.Orientation))),
                () => IsTraitDisabled || (!info.RenderBeforeBuildComplete && !buildComplete), p => RenderUtils.ZOffsetFromCenter(self, p, 1));

            rs.Add(anim, info.Palette, info.IsPlayerPalette);

        }

        void INotifyBuildComplete.BuildingComplete(Actor self)
        {
            buildComplete = true;
        }


    }
}