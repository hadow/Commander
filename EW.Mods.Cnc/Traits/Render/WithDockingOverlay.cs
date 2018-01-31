using System;
using EW.Traits;
using EW.Graphics;
using EW.Mods.Common.Traits;
namespace EW.Mods.Cnc.Traits.Render
{
    /// <summary>
    /// Rendered on the refinery when a voxel harvester is docking and undocking.
    /// </summary>
    public class WithDockingOverlayInfo:ITraitInfo,Requires<RenderSpritesInfo>,Requires<BodyOrientationInfo>
    {
        [SequenceReference]
        public readonly string Sequence = "unload-overlay";

        public readonly WVec Offset = WVec.Zero;

        [PaletteReference("IsPlayerPalette")]
        public readonly string Palette = null;

        public readonly bool IsPlayerPalette = false;

        public object Create(ActorInitializer init)
        {
            return new WithDockingOverlay(init.Self,this);
        }
    }

    public class WithDockingOverlay
    {

        public readonly WithDockingOverlayInfo Info;
        public readonly AnimationWithOffset WithOffset;


        public bool Visible;
        public WithDockingOverlay(Actor self,WithDockingOverlayInfo info)
        {
            Info = info;

            var rs = self.Trait<RenderSprites>();
            var body = self.Trait<BodyOrientation>();

            var overlay = new Animation(self.World, rs.GetImage(self));
            overlay.Play(info.Sequence);

            WithOffset = new AnimationWithOffset(overlay,
                () => body.LocalToWorld(info.Offset.Rotate(body.QuantizeOrientation(self, self.Orientation))),
                () => !Visible);

            rs.Add(WithOffset, info.Palette, info.IsPlayerPalette);
        }
    }
}