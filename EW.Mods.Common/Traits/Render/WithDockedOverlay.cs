using System;
using EW.Traits;
using EW.Graphics;
namespace EW.Mods.Common.Traits.Render
{
    public class WithDockedOverlayInfo:ITraitInfo
    {

        [SequenceReference]
        public readonly string Sequence = "docking-overlay";


        public readonly WVec Offset = WVec.Zero;

        [PaletteReference("IsPlayerPalette")]
        public readonly string Palette = null;

        public readonly bool IsPlayerPalette = false;
        public object Create(ActorInitializer init)
        {
            return new WithDockedOverlay(init.Self,this);
        }
    }

    public class WithDockedOverlay:INotifyDocking,INotifyBuildComplete
    {

        readonly WithDockedOverlayInfo info;
        readonly AnimationWithOffset anim;

        bool buildComplete;
        bool docked;

        public WithDockedOverlay(Actor self,WithDockedOverlayInfo info)
        {
            this.info = info;

            var rs = self.Trait<RenderSprites>();
            var body = self.Trait<BodyOrientation>();

            buildComplete = !self.Info.HasTraitInfo<BuildingInfo>();

            var overlay = new Animation(self.World, rs.GetImage(self));
            overlay.Play(info.Sequence);

            anim = new AnimationWithOffset(overlay, () => body.LocalToWorld(info.Offset.Rotate(body.QuantizeOrientation(self, self.Orientation))), () => !buildComplete || !docked);

            rs.Add(anim, info.Palette, info.IsPlayerPalette);
        }


        void PlayDockingOverlay()
        {
            if (docked)
                anim.Animation.PlayThen(info.Sequence, PlayDockingOverlay);
        }

        void INotifyBuildComplete.BuildingComplete(Actor self)
        {
            buildComplete = true;
        }

        void INotifyDocking.Docked(Actor self, Actor harvester)
        {
            docked = true;
            PlayDockingOverlay();
        }

        void INotifyDocking.Undocked(Actor self, Actor harvester)
        {
            docked = false;
        }
            


    }
}